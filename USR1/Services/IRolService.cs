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

        public RolService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _rolApiUrl = configuration["RolApiUrl"] ?? "http://localhost:5204";
        }

        public async Task<RolDto?> ObtenerRolAsync(int idRol, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_rolApiUrl}/rol/{idRol}");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<RolDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<RolDto?> ObtenerRolPorNombreAsync(string nombreRol, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_rolApiUrl}/rol");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var roles = await response.Content.ReadFromJsonAsync<List<RolDto>>();

                return roles?.FirstOrDefault(r =>
                    r.Nombre.Trim().Equals(nombreRol.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }
    }

    public record RolDto(int IdRol, string Nombre, DateTime FechaCreacion, DateTime? FechaModificacion);
}