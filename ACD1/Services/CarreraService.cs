// CarreraService.cs
using System.Text.Json;

namespace ACD1.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _acd2ApiUrl;
        private readonly ILogger<CarreraService> _logger;

        public CarreraService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CarreraService> logger)
        {
            _httpClient = httpClient;
            // Leer la URL del API ACD2 desde appsettings.json
            _acd2ApiUrl = configuration["ACD2ApiUrl"] ?? throw new InvalidOperationException("ACD2ApiUrl no configurada");
            _logger = logger;
        }

        public async Task<int> ContarCarrerasPorInstitucionAsync(int idInstitucion, string? authorization)
        {
            try
            {
                _logger.LogInformation("Consultando carreras de institución {IdInstitucion} en API ACD2", idInstitucion);

                // Consume endpoint GET /carrera/institucion/{id} del API ACD2
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_acd2ApiUrl}/carrera/institucion/{idInstitucion}");

                // Pasar el token de autorización
                if (!string.IsNullOrEmpty(authorization))
                    request.Headers.Add("Authorization", authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al consultar carreras en ACD2: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                var carreras = JsonSerializer.Deserialize<List<CarreraDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<CarreraDto>();

                _logger.LogInformation("Se encontraron {Count} carreras para la institución {IdInstitucion}",
                    carreras.Count, idInstitucion);

                return carreras.Count;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con API ACD2 al consultar carreras de institución {IdInstitucion}", idInstitucion);
                // En caso de error de conexión,no permitir eliminar 
                throw new InvalidOperationException("No se puede verificar las carreras asociadas. El servicio ACD2 no está disponible.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al contar carreras de la institución {IdInstitucion}", idInstitucion);
                throw;
            }
        }
    }

    // DTO para deserializar la respuesta del API ACD2
    public class CarreraDto
    {
        public int IdCarrera { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdInstitucion { get; set; }
        public int IdDirector { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}