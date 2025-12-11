using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class ListadoEstudiantesApiClient : IListadoEstudiantesApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<ListadoEstudiantesApiClient> _logger;

        public ListadoEstudiantesApiClient(HttpClient http, IConfiguration config, ILogger<ListadoEstudiantesApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiUrls:ServiciosApi:Listado_Estudiantes"]
                ?? throw new InvalidOperationException("Base URL de Listado_Estudiantes no configurada");
        }

        public async Task<(bool ok, int statusCode, string? message, List<EstudiantesListadoDto>? data)>
            ObtenerListadoAsync(int periodo, string token, CancellationToken ct = default)
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

                var url = $"{_baseUrl}/api/estudiantes/listadoestudiantes?periodo={periodo}";
                _logger.LogInformation("Consultando listado de estudiantes en: {Url}", url);

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
                List<EstudiantesListadoDto>? listado = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    listado = JsonSerializer.Deserialize<List<EstudiantesListadoDto>>(
                        arr.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                _logger.LogInformation("Listado obtenido correctamente. {Count} registros", listado?.Count ?? 0);
                return (statusCode == 200, statusCode, message, listado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando respuesta del listado de estudiantes");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
    }
}
