using Avatar_Mod_Administración.Entities;
using System.Net;
using System.Net.Http.Headers;

namespace Avatar_Mod_Administración.Services
{
    public class RubroApiClient : IRubroApiClient
    {
        private readonly HttpClient _http;
        private readonly string _token;

        public RubroApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            // Token temporal para pruebas 
            _token = config["Adm_Notas:AccessToken"]
                ?? throw new InvalidOperationException("Token no configurado en appsettings.json");
        }


        public async Task<(bool ok, int statusCode, string? message)> CargarDesglose(DesgloseRequest request, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.PostAsJsonAsync("/api/rubros/cargardesglose", request, ct);

            var result = new
            {
                statusCode = (int)response.StatusCode,
                message = "",
                responseObject = (object?)null
            };

            try
            {
                var payload = await response.Content.ReadFromJsonAsync(result.GetType(), cancellationToken: ct);
                if (payload != null)
                {
                    var statusProp = (int)payload.GetType().GetProperty("statusCode")?.GetValue(payload)!;
                    var messageProp = (string?)payload.GetType().GetProperty("message")?.GetValue(payload);
                    return (response.IsSuccessStatusCode, statusProp, messageProp);
                }
            }
            catch
            {
                // fallback si no se pudo parsear el JSON
                return (response.IsSuccessStatusCode, (int)response.StatusCode, "No se pudo leer la respuesta del servidor.");
            }

            return (response.IsSuccessStatusCode, (int)response.StatusCode, null);
        }


        // Obtener Desglose
        public async Task<(bool ok, int statusCode, string? message, List<RubroDto>? data)> ObtenerDesglose(int idGrupo, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/rubros/obtenerdesglose?idGrupo={idGrupo}", ct);

            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: ct);

                var root = doc.RootElement;
                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";
                List<RubroDto>? rubros = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    rubros = System.Text.Json.JsonSerializer.Deserialize<List<RubroDto>>(arr.GetRawText(),
    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                }

                return (statusCode == 200, statusCode, message, rubros);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error al procesar respuesta: {ex.Message}", null);
            }
        }




    }
}
