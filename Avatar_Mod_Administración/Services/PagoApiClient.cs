using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class PagoApiClient : IPagoApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<PagoApiClient> _logger;

        public PagoApiClient(HttpClient http, IConfiguration config, ILogger<PagoApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiUrls:ServiciosApi:Pagos"]
                ?? throw new InvalidOperationException("Base URL de Pagos no configurada");
        }

        public async Task<(bool ok, int statusCode, string? message, PagoDto? pago)>
            CrearPagoAsync(int idFactura, string metodo, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, 401, "Token no proporcionado", null);

                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}/api/pago";
                _logger.LogInformation("Registrando pago en: {Url}", url);

                var body = JsonSerializer.Serialize(new { id_factura = idFactura, metodo_pago = metodo });
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                int status = json.RootElement.GetProperty("statusCode").GetInt32();
                string message = json.RootElement.GetProperty("message").GetString() ?? "";

                PagoDto? pago = null;
                if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                {
                    pago = JsonSerializer.Deserialize<PagoDto>(
                        obj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                return (status == 201, status, message, pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }

        public async Task<(bool ok, int statusCode, string? message)>
            ReversarPagoAsync(int idPago, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, 401, "Token no proporcionado");

                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}/api/pago/{idPago}/reversar";
                _logger.LogInformation("Reversando pago en: {Url}", url);

                var response = await _http.PutAsync(url, null);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}");
                }

                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                int status = json.RootElement.GetProperty("statusCode").GetInt32();
                string message = json.RootElement.GetProperty("message").GetString() ?? "";

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reversar pago");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message, PagoDto? pago)>
            ObtenerPagoAsync(int idPago, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, 401, "Token no proporcionado", null);

                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}/api/pago/{idPago}";
                _logger.LogInformation("Consultando pago en: {Url}", url);

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                int status = json.RootElement.GetProperty("statusCode").GetInt32();
                string message = json.RootElement.GetProperty("message").GetString() ?? "";

                PagoDto? pago = null;
                if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                {
                    pago = JsonSerializer.Deserialize<PagoDto>(
                        obj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                return (status == 200, status, message, pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pago");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
    }
}
