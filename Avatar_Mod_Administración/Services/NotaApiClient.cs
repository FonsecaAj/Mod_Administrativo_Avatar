using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;

namespace Avatar_Mod_Administración.Services
{
    public class NotaApiClient : INotaApiClient
    {

        private readonly HttpClient _http;
        private readonly string _token;

        public NotaApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            // Token temporal para pruebas 
            _token = config["Adm_Notas:AccessToken"]
                ?? throw new InvalidOperationException("Token no configurado en appsettings.json");
        }

        // Obtener notas por estudiante y curso
        public async Task<(bool ok, int statusCode, string? message, List<NotaDto>? data)> ObtenerNotas(int idEstudiante, int idCurso, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/rubros/obtenernotas?idEstudiante={idEstudiante}&idCurso={idCurso}", ct);

            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: ct);

                var root = doc.RootElement;
                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";
                List<NotaDto>? notas = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    notas = System.Text.Json.JsonSerializer.Deserialize<List<NotaDto>>(arr.GetRawText());
                }

                return (statusCode == 200, statusCode, message, notas);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error al procesar respuesta: {ex.Message}", null);
            }
        }

        // Asignar o modificar nota de rubro
        public async Task<(bool ok, int statusCode, string? message)> AsignarNota(NotaRequest request, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.PostAsJsonAsync("/api/rubros/asignarnotarubro", request, ct);

            string? msg = null;
            try
            {
                var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>(cancellationToken: ct);
                if (payload != null && payload.TryGetValue("message", out var m))
                    msg = m?.ToString();
            }
            catch { }

            return (response.IsSuccessStatusCode, (int)response.StatusCode, msg);
        }





    }
}
