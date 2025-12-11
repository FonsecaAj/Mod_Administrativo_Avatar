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

        // ============== TOKEN ==============

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

        // ============== PROVINCIAS ==============

        public async Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync()
        {
            AplicarToken();

            var wrapper =
                await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<ProvinciaDto>>>(
                    "api/provincias");

            return wrapper?.ResponseObject ?? Enumerable.Empty<ProvinciaDto>();
        }

        // ============== CANTONES ==============

        public async Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia)
        {
            AplicarToken();

            var wrapper =
                await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<CantonDto>>>(
                    $"api/cantones?provincia={idProvincia}");

            return wrapper?.ResponseObject ?? Enumerable.Empty<CantonDto>();
        }

        // ============== DISTRITOS ==============

        public async Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton)
        {
            AplicarToken();

            var wrapper =
                await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<DistritoDto>>>(
                    $"api/distritos?provincia={idProvincia}&canton={idCanton}");

            return wrapper?.ResponseObject ?? Enumerable.Empty<DistritoDto>();
        }
    }
}
