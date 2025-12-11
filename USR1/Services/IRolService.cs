using System.Text.Json;

namespace USR1.Services
{
    public interface IRolService
    {
        Task<RolDto?> ObtenerRolAsync(int idRol, string? authToken);
        Task<RolDto?> ObtenerRolPorNombreAsync(string nombreRol, string? authToken);
    }

    public class RolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly string _rolApiUrl;
        private readonly ILogger<RolService> _logger;

        public RolService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RolService> logger)
        {
            _httpClient = httpClient;
            _rolApiUrl = configuration["RolApiUrl"] ?? "http://localhost:5204";
            _logger = logger;
        }

        public async Task<RolDto?> ObtenerRolAsync(int idRol, string? authToken)
        {
            try
            {
                _logger.LogInformation("Consultando rol {IdRol} en API USR2", idRol);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_rolApiUrl}/rol/{idRol}");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al consultar rol en API USR2: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    return JsonSerializer.Deserialize<RolDto>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return await response.Content.ReadFromJsonAsync<RolDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con API USR2 al consultar rol {IdRol}", idRol);
                throw new InvalidOperationException(
                    "No se puede verificar el rol. El servicio de roles no está disponible.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener rol {IdRol}", idRol);
                throw;
            }
        }

        public async Task<RolDto?> ObtenerRolPorNombreAsync(string nombreRol, string? authToken)
        {
            try
            {
                _logger.LogInformation("Consultando rol por nombre '{NombreRol}' en API USR2", nombreRol);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_rolApiUrl}/rol");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al consultar roles en API USR2: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                List<RolDto>? roles = null;

                // Parsear BusinessLogicResponse si existe
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    roles = JsonSerializer.Deserialize<List<RolDto>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    roles = JsonSerializer.Deserialize<List<RolDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var rolEncontrado = roles?.FirstOrDefault(r =>
                    r.Nombre.Trim().Equals(nombreRol.Trim(), StringComparison.OrdinalIgnoreCase));

                if (rolEncontrado == null)
                {
                    _logger.LogWarning("No se encontró el rol con nombre '{NombreRol}'", nombreRol);
                }

                return rolEncontrado;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con API USR2 al buscar rol '{NombreRol}'", nombreRol);
                throw new InvalidOperationException(
                    "No se puede verificar el rol. El servicio de roles no está disponible.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar rol '{NombreRol}'", nombreRol);
                throw;
            }
        }
    }

    public record RolDto(int IdRol, string Nombre, DateTime FechaCreacion, DateTime? FechaModificacion);
}