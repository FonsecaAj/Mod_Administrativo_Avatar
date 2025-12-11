using System.Net.Http.Json;

namespace ApiACD3.Services
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public BitacoraConsumer(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["BitacoraService:BaseUrl"] ?? "http://localhost:5293";
        }

   
        public async Task RegistrarAccionAsync(string usuario, string tipo, object detalle)
        {
            var body = new
            {
                Usuario = usuario,
                Tipo = tipo,
                Detalle = detalle, 
                Fecha = DateTime.UtcNow
            };

            await _http.PostAsJsonAsync($"{_baseUrl}/bitacora", body);
        }
    }
}
