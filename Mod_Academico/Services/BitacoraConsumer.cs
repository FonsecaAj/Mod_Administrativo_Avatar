using System.Text.Json;

namespace Mod_Academico.Services
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

        // Registrar (INSERT, UPDATE, DELETE, SELECT)
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

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/bitacora", body);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar bitácora: {ex.Message}");
            }
        }
    }
}
