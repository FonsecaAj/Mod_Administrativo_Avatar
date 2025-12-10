using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class UbicacionesApiClient : IUbicacionesApiClient
    {
        private readonly HttpClient _http;
        private readonly IAuthService _authService;

        public UbicacionesApiClient(HttpClient http, IAuthService authService)
        {
            _http = http;
            _authService = authService;
        }

        private string ObtenerTokenLimpio()
        {
            var sesion = _authService.ObtenerSesionActual();

            if (sesion == null || string.IsNullOrWhiteSpace(sesion.AccessToken))
                return string.Empty;

            var token = sesion.AccessToken;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7).Trim();

            return token;
        }

        private void AplicarToken()
        {
            var token = ObtenerTokenLimpio();

            _http.DefaultRequestHeaders.Remove("Authorization");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ============ PROVINCIAS ============

        public async Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync()
        {
            AplicarToken();

            var response = await _http.GetAsync("api/provincias");

            if (!response.IsSuccessStatusCode)
            {
                // opcional: log rápido para depurar
                Console.WriteLine($"[UbicacionesApiClient] Provincias Status: {response.StatusCode}");
                return Enumerable.Empty<ProvinciaDto>();
            }

            var wrapper = await response.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<ProvinciaDto>>>();

            return wrapper?.ResponseObject ?? Enumerable.Empty<ProvinciaDto>();
        }

        // ============ CANTONES ============

        public async Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia)
        {
            AplicarToken();

            var response = await _http.GetAsync($"api/cantones?provincia={idProvincia}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[UbicacionesApiClient] Cantones Status: {response.StatusCode}");
                return Enumerable.Empty<CantonDto>();
            }

            var wrapper = await response.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<CantonDto>>>();

            return wrapper?.ResponseObject ?? Enumerable.Empty<CantonDto>();
        }

        // ============ DISTRITOS ============

        public async Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton)
        {
            AplicarToken();

            var response = await _http.GetAsync($"api/distritos?provincia={idProvincia}&canton={idCanton}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[UbicacionesApiClient] Distritos Status: {response.StatusCode}");
                return Enumerable.Empty<DistritoDto>();
            }

            var wrapper = await response.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<DistritoDto>>>();

            return wrapper?.ResponseObject ?? Enumerable.Empty<DistritoDto>();
        }
    }
}
