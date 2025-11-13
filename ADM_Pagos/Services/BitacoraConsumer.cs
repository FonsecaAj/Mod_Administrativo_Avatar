using System.Text.Json;

namespace ADM_Pagos.Services
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public BitacoraConsumer(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _baseUrl = cfg["BitacoraService:BaseUrl"] ?? "http://localhost:5293";
        }

        public async Task RegistrarAsync(string usuario, string tipo, object detalle)
        {
            try
            {
                var body = new
                {
                    Usuario = usuario,
                    Tipo_Accion = string.IsNullOrWhiteSpace(tipo) ? "INSERT" : tipo,
                    Descripcion = JsonSerializer.Serialize(detalle)
                };

                var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/bitacora", body);
                resp.EnsureSuccessStatusCode();
            }
            catch { /* swallow per requerimiento */ }
        }
    }
}
