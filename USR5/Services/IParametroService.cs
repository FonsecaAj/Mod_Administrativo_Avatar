namespace USR5.Services
{
    public interface IParametroService
    {
        Task<int> ObtenerTiempoExpiracionJwtAsync();
        Task<int> ObtenerTiempoExpiracionRefreshAsync();
    }

    public class ParametroService : IParametroService
    {
        private readonly HttpClient _httpClient;
        private readonly string _parametroApiUrl;

        public ParametroService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _parametroApiUrl = configuration["ParametroApiUrl"] ?? "http://localhost:5279";
        }

        public async Task<int> ObtenerTiempoExpiracionJwtAsync()
        {
            try
            {
                // Usar endpoint público que no requiere autenticación
                var response = await _httpClient.GetAsync($"{_parametroApiUrl}/parametro/public/JWTEXPMIN");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"No se pudo obtener JWTEXPMIN. Status: {response.StatusCode}");
                    return 30;
                }

                var parametro = await response.Content.ReadFromJsonAsync<ParametroDto>();

                if (parametro == null || !int.TryParse(parametro.Valor, out int minutos))
                {
                    Console.WriteLine($"Formato de JWTEXPMIN inválido");
                    return 30;
                }

                return minutos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener JWTEXPMIN: {ex.Message}.");
                return 30;
            }
        }

        public async Task<int> ObtenerTiempoExpiracionRefreshAsync()
        {
            try
            {
                // Usar endpoint público que no requiere autenticación
                var response = await _httpClient.GetAsync($"{_parametroApiUrl}/parametro/public/REFEXPMIN");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"No se pudo obtener REFEXPMIN. Status: {response.StatusCode}.");
                    return 1440;
                }

                var parametro = await response.Content.ReadFromJsonAsync<ParametroDto>();

                if (parametro == null || !int.TryParse(parametro.Valor, out int minutos))
                {
                    Console.WriteLine($"Formato de REFEXPMIN inválido.");
                    return 1440;
                }

                Console.WriteLine($"REFEXPMIN obtenido: {minutos} minutos");
                return minutos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener REFEXPMIN: {ex.Message}.");
                return 1440;
            }
        }
    }

    public record ParametroDto(string IdParametro, string Valor);
}