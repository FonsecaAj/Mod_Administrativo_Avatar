using System.Text.Json;

namespace Adm_Notificaciones.Service
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public BitacoraConsumer(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["BitacoraService:BaseUrl"];
        }

        public async Task RegistrarAccionAsync(string usuario, string tipoAccion, object detalle)
        {
            try
            {
                var body = new
                {
                    Usuario = usuario,
                    Tipo_Accion = tipoAccion,
                    Descripcion = JsonSerializer.Serialize(detalle)
                };

                await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/bitacora", body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando bitácora: {ex.Message}");
            }
        }
    }
}
