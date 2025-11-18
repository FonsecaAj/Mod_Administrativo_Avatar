using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class PagoApiClient : IPagoApiClient
    {

        private readonly HttpClient _http;
        private readonly string _token;

        public PagoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _token = config["ServiciosApi:AccessToken"] ?? throw new InvalidOperationException("Falta token de acceso");
        }

        // Crear pago
        public async Task<(bool ok, int statusCode, string? message, PagoDto? pago)> CrearPagoAsync(int idFactura, string metodo)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var body = JsonSerializer.Serialize(new { id_factura = idFactura, metodo_pago = metodo });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/pago", content);
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            int status = json.RootElement.GetProperty("statusCode").GetInt32();
            string message = json.RootElement.GetProperty("message").GetString() ?? "";

            PagoDto? pago = null;
            if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                pago = JsonSerializer.Deserialize<PagoDto>(obj.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (status == 201, status, message, pago);
        }

        // Reversar pago
        public async Task<(bool ok, int statusCode, string? message)> ReversarPagoAsync(int idPago)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _http.PutAsync($"/api/pago/{idPago}/reversar", null);
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}");

            var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            int status = json.RootElement.GetProperty("statusCode").GetInt32();
            string message = json.RootElement.GetProperty("message").GetString() ?? "";

            return (status == 200, status, message);
        }

        // Consultar pago
        public async Task<(bool ok, int statusCode, string? message, PagoDto? pago)> ObtenerPagoAsync(int idPago)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _http.GetAsync($"/api/pago/{idPago}");
            if (!response.IsSuccessStatusCode)
                return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);

            var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            int status = json.RootElement.GetProperty("statusCode").GetInt32();
            string message = json.RootElement.GetProperty("message").GetString() ?? "";

            PagoDto? pago = null;
            if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                pago = JsonSerializer.Deserialize<PagoDto>(obj.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (status == 200, status, message, pago);
        }

    }
}
