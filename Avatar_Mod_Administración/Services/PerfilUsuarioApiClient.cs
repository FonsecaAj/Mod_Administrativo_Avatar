using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class PerfilUsuarioApiClient : IPerfilUsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<PerfilUsuarioApiClient> _logger;

        public PerfilUsuarioApiClient(HttpClient http, IConfiguration config, ILogger<PerfilUsuarioApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiUrls:ServiciosApi:Perfil_Usuario"]
                ?? throw new InvalidOperationException("Base URL de Perfil_Usuario no configurada");
        }

        public async Task<(bool ok, int statusCode, string? message, PerfilUsuarioDto? data)>
            ObtenerPerfilAsync(string email, string token, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, 401, "Token no proporcionado", null);

                // Limpieza del token
                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}api/perfil?email={email}";
                _logger.LogInformation("Consultando perfil del usuario en: {Url}", url);

                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Error {StatusCode}: {Error}", response.StatusCode, error);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                var root = doc.RootElement;

                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";

                PerfilUsuarioDto? perfil = null;

                if (root.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                {
                    perfil = JsonSerializer.Deserialize<PerfilUsuarioDto>(
                        obj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                _logger.LogInformation("Perfil obtenido correctamente: {Email}", perfil?.Email);

                return (statusCode == 200, statusCode, message, perfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando respuesta del servicio Perfil Usuario");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
    }
}
