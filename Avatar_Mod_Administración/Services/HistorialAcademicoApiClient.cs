using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{

    public class HistorialAcademicoApiClient : IHistorialAcademicoApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<HistorialAcademicoApiClient> _logger;

        public HistorialAcademicoApiClient(HttpClient http, IConfiguration config, ILogger<HistorialAcademicoApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiUrls:ServiciosApi:HistorialAcademico"]
                ?? throw new InvalidOperationException("Base URL de Historial no configurada");
        }

        // Llamada real a la API de historial académico

        public async Task<(bool ok, int statusCode, string? message, List<HistorialAcademicoDto>? data)>
            ObtenerHistorialAsync(string tipo, string identificacion, string token, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, 401, "Token no proporcionado", null);

                // Asegurarse de limpiar el token
                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}/api/historialacademico?tipo={tipo}&identificacion={identificacion}";
                _logger.LogInformation("Llamando a {Url}", url);

                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;

                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";
                List<HistorialAcademicoDto>? datos = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    datos = JsonSerializer.Deserialize<List<HistorialAcademicoDto>>(
                        arr.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                return (statusCode == 200, statusCode, message, datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar respuesta del historial académico");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }


    }
}
