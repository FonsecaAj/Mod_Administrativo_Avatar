using ADM_Pagos.Entities;

namespace ADM_Pagos.Services
{
    public class FacturaCliente
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public FacturaCliente(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _baseUrl = cfg["FacturacionServiceUrl"] ?? "http://localhost:5183";
        }

        public async Task<FacturaDatos?> ObtenerFacturaAsync(int idFactura, string? authorization)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/factura/{idFactura}");
            if (!string.IsNullOrWhiteSpace(authorization))
                req.Headers.Add("Authorization", authorization);

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var envelope = await resp.Content.ReadFromJsonAsync<EnvelopeResponse<FacturaDatos>>();
            return envelope?.ResponseObject;
        }

        public async Task<bool> MarcarFacturaPagadaAsync(int idFactura, string? authorization)
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/api/factura/{idFactura}/pagada");
            if (!string.IsNullOrWhiteSpace(authorization))
                req.Headers.Add("Authorization", authorization);

            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> MarcarFacturaPendienteAsync(int idFactura, string? authorization)
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/api/factura/{idFactura}/pendiente");
            if (!string.IsNullOrWhiteSpace(authorization))
                req.Headers.Add("Authorization", authorization);

            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
    }
}
