using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class FacturaApiClient : IFacturaApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<FacturaApiClient> _logger;

        public FacturaApiClient(HttpClient http, IConfiguration config, ILogger<FacturaApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiUrls:ServiciosApi:Adm_Facturacion"]
                ?? throw new InvalidOperationException("Base URL de Adm_Facturacion no configurada");
        }

        public async Task<(bool ok, int statusCode, string? message, int? idFactura)>
            CrearFacturaAsync(string identificacion, string token, CancellationToken ct = default)
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

                var url = $"{_baseUrl}/api/factura";
                _logger.LogInformation("Creando factura en: {Url}", url);

                var content = new StringContent(JsonSerializer.Serialize(identificacion), Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(url, content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";
                int? id = doc.RootElement.TryGetProperty("responseObject", out var ro) ? ro.GetInt32() : null;

                return (status == 201, status, message, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear factura");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }

        public async Task<(bool ok, int statusCode, string? message)>
    ReversarFacturaAsync(int idFactura, string motivo, string token, CancellationToken ct = default)
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

                var url = $"{_baseUrl}/api/factura/{idFactura}/reversar";
                _logger.LogInformation("Reversando factura en: {Url} con motivo: {Motivo}", url, motivo);

                var response = await _http.PutAsync(url, contenido, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}: {err}");
                }

                using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                int status = payload.RootElement.GetProperty("statusCode").GetInt32();
                string message = payload.RootElement.GetProperty("message").GetString() ?? "";

                _logger.LogInformation("Factura {FacturaId} reversada correctamente. Estado {Status}: {Mensaje}", idFactura, status, message);

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reversar factura con ID {FacturaId}", idFactura);
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message, FacturaDto? factura)>
            ObtenerFacturaAsync(int idFactura, string token, CancellationToken ct = default)
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

                var url = $"{_baseUrl}/api/factura/{idFactura}";
                _logger.LogInformation("Consultando factura en: {Url}", url);

                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                int status = payload.RootElement.GetProperty("statusCode").GetInt32();
                string message = payload.RootElement.GetProperty("message").GetString() ?? "";
                FacturaDto? factura = null;

                if (payload.RootElement.TryGetProperty("responseObject", out var obj) && obj.ValueKind == JsonValueKind.Object)
                    factura = JsonSerializer.Deserialize<FacturaDto>(obj.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (status == 200, status, message, factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
        // ====== Listar Facturas por Periodo/Estado (GET) ======
        public async Task<(bool ok, int statusCode, string? message, List<FacturaDto>? facturas)>
            ListarFacturasAsync(DateTime inicio, DateTime fin, string? estado, string token, CancellationToken ct = default)
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

                var inicioStr = inicio.ToString("yyyy/MM/dd");
                var finStr = fin.ToString("yyyy/MM/dd");

                var url = $"{_baseUrl}/api/factura/listado?inicio={inicioStr}&fin={finStr}";

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    url += $"&estado={Uri.EscapeDataString(estado)}";
                }

                _logger.LogInformation("Consultando listado de facturas en: {Url}", url);

                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error {Status}: {Error}", response.StatusCode, err);
                    return (false, (int)response.StatusCode, $"Error {response.StatusCode}", null);
                }

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";
                List<FacturaDto>? facturas = null; //

                if (doc.RootElement.TryGetProperty("responseObject", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    // ⭐ Deserializamos directamente la lista de FacturaDto
                    facturas = JsonSerializer.Deserialize<List<FacturaDto>>(arr.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return (status == 200, status, message, facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar facturas");
                return (false, 500, $"Error procesando respuesta: {ex.Message}", null);
            }
        }
    }
}
