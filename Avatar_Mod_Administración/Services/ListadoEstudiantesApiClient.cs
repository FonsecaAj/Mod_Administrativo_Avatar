using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class ListadoEstudiantesApiClient : IListadoEstudiantesApiClient
    {


        private readonly HttpClient _http;
        private readonly string _token;

        public ListadoEstudiantesApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            _token = config["ServiciosApi:AccessToken"]
                ?? throw new InvalidOperationException("Token no configurado en appsettings.json");
        }

        public async Task<(bool ok, int statusCode, string? message, List<EstudiantesListadoDto>? data)> ObtenerListadoAsync(int periodo, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/estudiantes/listadoestudiantes?periodo={periodo}", ct);

            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                var root = doc.RootElement;
                int statusCode = root.GetProperty("statusCode").GetInt32();
                string message = root.GetProperty("message").GetString() ?? "";
                List<EstudiantesListadoDto>? listado = null;

                if (root.TryGetProperty("responseObject", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    listado = JsonSerializer.Deserialize<List<EstudiantesListadoDto>>(arr.GetRawText(), options);
                }

                return (statusCode == 200, statusCode, message, listado);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }

    }
}
