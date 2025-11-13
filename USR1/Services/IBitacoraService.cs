namespace USR1.Services
{
    public interface IBitacoraService
    {
        Task RegistrarAsync(string usuario, string descripcion, string? tipoAccion = null);
    }

    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _gen1ApiUrl;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(HttpClient httpClient, IConfiguration configuration, ILogger<BitacoraService> logger)
        {
            _httpClient = httpClient;
            _gen1ApiUrl = configuration["GEN1ApiUrl"] ?? "http://localhost:5155";
            _logger = logger;
        }

        public async Task RegistrarAsync(string usuario, string descripcion, string? tipoAccion = null)
        {
            try
            {
                var request = new
                {
                    usuario,
                    descripcion,
                    tipo_Accion = tipoAccion
                };

                var response = await _httpClient.PostAsJsonAsync($"{_gen1ApiUrl}/api/bitacora", request);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al registrar bitácora. Status: {StatusCode}, Response: {Content}",
                        response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al registrar bitácora en {Url}", $"{_gen1ApiUrl}/api/bitacora");
            }
        }
    }
}