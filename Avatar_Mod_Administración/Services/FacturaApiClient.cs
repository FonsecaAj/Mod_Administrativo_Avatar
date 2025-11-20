using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class FacturaApiClient : IFacturaApiClient
    {

        private readonly HttpClient _http;
        private readonly string _token;

        public FacturaApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _token = config["ServiciosApi:AccessToken"]
                ?? throw new InvalidOperationException("Token no configurado en appsettings.json");
        }

        public async Task<(bool ok, int statusCode, string? message, int? idFactura)> CrearFacturaAsync(string identificacion, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Enviamos un string literal en el cuerpo
            var content = new StringContent(JsonSerializer.Serialize(identificacion), Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/factura", content, ct);

            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            int statusCode = doc.RootElement.GetProperty("statusCode").GetInt32();
            string message = doc.RootElement.GetProperty("message").GetString() ?? "";
            int? id = doc.RootElement.TryGetProperty("responseObject", out var ro) ? ro.GetInt32() : null;

            return (statusCode == 201, statusCode, message, id);
        }

        public async Task<(bool ok, int statusCode, string? message)> ReversarFacturaAsync(int idFactura, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.PutAsync($"/api/factura/{idFactura}/reversar", null, ct);
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}");

            var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            int status = payload.RootElement.GetProperty("statusCode").GetInt32();
            string message = payload.RootElement.GetProperty("message").GetString() ?? "";

            return (status == 200, status, message);
        }

        public async Task<(bool ok, int statusCode, string? message, FacturaDto? factura)> ObtenerFacturaAsync(int idFactura, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _http.GetAsync($"/api/factura/{idFactura}", ct);
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            int status = payload.RootElement.GetProperty("statusCode").GetInt32();
            string message = payload.RootElement.GetProperty("message").GetString() ?? "";
            FacturaDto? factura = null;

            if (payload.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                    factura = JsonSerializer.Deserialize<FacturaDto>(
                        obj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

            return (status == 200, status, message, factura);
        }



    }
}
