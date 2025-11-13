namespace Estudiantes_Matriculados.Services
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BitacoraConsumer(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task RegistrarAccionAsync(string usuario, string accion, object detalle)
        {
            try
            {
                var baseUrl = _configuration["Bitacora:BaseUrl"] ?? "http://localhost:5293";

                var payload = new
                {
                    Usuario = usuario,
                    Accion = accion,
                    Detalle = detalle
                };

                await _httpClient.PostAsJsonAsync($"{baseUrl}/bitacora", payload);
            }
            catch
            {
                // No lanzar excepción si la bitácora falla
            }
        }

    }
}
