using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class NotaApiClient : INotaApiClient
    {

        private readonly HttpClient _http;
        private readonly string _token;

        public NotaApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _token = config["ServiciosApi:AccessToken"] ?? throw new InvalidOperationException("Falta token de acceso");
        }

        // ====== Obtener notas por estudiante y curso ======
        public async Task<(bool ok, int statusCode, string? message, List<NotaDto>? data)> ObtenerNotas(int idEstudiante, int idCurso)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/rubros/obtenernotas?idEstudiante={idEstudiante}&idCurso={idCurso}");
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            int status = payload.RootElement.GetProperty("statusCode").GetInt32();
            string message = payload.RootElement.GetProperty("message").GetString() ?? "";

            List<NotaDto>? notas = null;
            if (payload.RootElement.TryGetProperty("responseObject", out var arr) && arr.ValueKind == JsonValueKind.Array)
                notas = JsonSerializer.Deserialize<List<NotaDto>>(arr.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (status == 200, status, message, notas);
        }

        // ====== Asignar o editar nota ======
        public async Task<(bool ok, int statusCode, string? message)> AsignarNota(NotaRequest request)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/rubros/asignarnotarubro", content);
            var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            int status = payload.RootElement.GetProperty("statusCode").GetInt32();
            string message = payload.RootElement.GetProperty("message").GetString() ?? "";

            return (status == 201 || status == 200, status, message);
        }


    }
}
