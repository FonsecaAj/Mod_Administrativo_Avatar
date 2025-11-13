using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;

namespace Avatar_Mod_Administración.Services
{

    public class HistorialAcademicoApiClient : IHistorialAcademicoApiClient
    {
        private readonly HttpClient _http;
        private readonly string _token;

        public HistorialAcademicoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            _token = config["ServiciosApi:AccessToken"]
                ?? throw new InvalidOperationException("Token no configurado en appsettings.json");
        }

        // Llamada real a la API de historial académico
        public async Task<(bool ok, int statusCode, string? message, List<HistorialAcademicoDto>? data)>
            ObtenerHistorialAsync(string tipo, string identificacion, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/historialacademico?tipo={tipo}&identificacion={identificacion}", ct);

            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;

                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";
                List<HistorialAcademicoDto>? datos = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    datos = System.Text.Json.JsonSerializer.Deserialize<List<HistorialAcademicoDto>>(
                        arr.GetRawText(),
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                return (statusCode == 200, statusCode, message, datos);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error al procesar respuesta: {ex.Message}", null);
            }
        }
    }
}
