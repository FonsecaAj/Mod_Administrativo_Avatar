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
            ReversarPagoAsync(int idPago, string motivo, string token)
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

                // Serializar el motivo (detalle del reverso)
                var contenido = new StringContent(JsonSerializer.Serialize(motivo), Encoding.UTF8, "application/json");

                var url = $"{_baseUrl}/api/pago/{idPago}/reversar";
                _logger.LogInformation("Reversando pago en: {Url} con motivo: {Motivo}", url, motivo); // Log de motivo

                var response = await _http.PutAsync(url, contenido); // Se envía el contenido

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}");
                }

                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                int status = json.RootElement.GetProperty("statusCode").GetInt32();
                string message = json.RootElement.GetProperty("message").GetString() ?? "";

                _logger.LogInformation("Pago {PagoId} reversado correctamente. Estado {Status}: {Mensaje}", idPago, status, message);

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reversar pago");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message, PagoDto? pago, IEnumerable<PagoDetalleDto>? detalles)>
    ObtenerPagoAsync(int idPago, string token) // ⭐ CAMBIO: Ahora retorna detalles
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    // ⭐ CAMBIO: Retorna null para los detalles también
                    return (false, 401, "Token no proporcionado", null, null);

                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                // Si _http es compartido, es buena práctica remover y establecer el Authorization
                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);

                var url = $"{_baseUrl}/api/pago/{idPago}";
                _logger.LogInformation("Consultando pago en: {Url}", url);

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    // ⭐ CAMBIO: Retorna null para los detalles también
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null, null);
                }

                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                int status = json.RootElement.GetProperty("statusCode").GetInt32();
                string message = json.RootElement.GetProperty("message").GetString() ?? "";

                PagoDto? pago = null;
                // ⭐ NUEVO: Inicializar lista de detalles
                IEnumerable<PagoDetalleDto>? detalles = Enumerable.Empty<PagoDetalleDto>();

                if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // 1. Extraer y deserializar el objeto 'pago' principal
                    if (obj.TryGetProperty("pago", out var pagoElement) && pagoElement.ValueKind == JsonValueKind.Object)
                    {
                        pago = JsonSerializer.Deserialize<PagoDto>(
                            pagoElement.GetRawText(),
                            options
                        );
                    }
                    else
                    {
                        _logger.LogWarning("La propiedad 'pago' no se encontró dentro de 'responseObject' para ID: {idPago}", idPago);
                    }

                    // ⭐ 2. NUEVO: Extraer y deserializar el array 'detalles'
                    if (obj.TryGetProperty("detalles", out var detallesElement) && detallesElement.ValueKind == JsonValueKind.Array)
                    {
                        detalles = JsonSerializer.Deserialize<List<PagoDetalleDto>>(
                            detallesElement.GetRawText(),
                            options
                        );
                    }
                }

                // ⭐ CAMBIO: Retorna el pago y la lista de detalles
                return (status == 200, status, message, pago, detalles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pago con ID {idPago}", idPago);
                // ⭐ CAMBIO: Retorna null para los detalles también
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null, null);
            }
        }


        public async Task<(bool ok, int statusCode, string? message, IEnumerable<PagoDto>? pagos)>
            ListarPagosPorPeriodoAsync(DateTime inicio, DateTime fin, string token)
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

                // Formato de fecha esperado por la API (YYYY/MM/DD) y codificación URL
                var inicioStr = inicio.ToString("yyyy/MM/dd");
                var finStr = fin.ToString("yyyy/MM/dd");

                var url = $"{_baseUrl}/api/pago/listado?inicio={inicioStr}&fin={finStr}";
                _logger.LogInformation("Consultando listado de pagos en: {Url}", url);

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

                IEnumerable<PagoDto>? pagos = Enumerable.Empty<PagoDto>();
                if (json.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Array)
                {
                    pagos = JsonSerializer.Deserialize<IEnumerable<PagoDto>>(
                        obj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                return (status == 200, status, message, pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar pagos por periodo");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
    }
}
