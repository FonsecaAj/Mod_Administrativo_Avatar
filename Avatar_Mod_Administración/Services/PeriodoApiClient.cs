using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class PeriodoApiClient : IPeriodoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public PeriodoApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _baseUrl = $"{_config["Adm_Periodos:BaseUrl"]}/api/periodo";
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

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

       
        public async Task<IEnumerable<PeriodoDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            var httpResponse = await _http.GetAsync(_baseUrl);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET TODOS PERIODO] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return new List<PeriodoDto>();
            }

            var wrapper = await httpResponse.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<PeriodoDto>>>();

            return wrapper?.ResponseObject ?? new List<PeriodoDto>();
        }

        public async Task<PeriodoDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            var httpResponse = await _http.GetAsync($"{_baseUrl}/{id}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET PERIODO/{id}] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return null;
            }

            var wrapper = await httpResponse.Content
                .ReadFromJsonAsync<BusinessLogicResponse<PeriodoDto>>();

            return wrapper?.ResponseObject;
        }

       
        public async Task<bool> CrearAsync(PeriodoDto periodo)
        {
            AplicarToken();

            var httpResponse = await _http.PostAsJsonAsync(_baseUrl, periodo);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API POST PERIODO] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }

       
        public async Task<bool> ActualizarAsync(PeriodoDto periodo)
        {
            AplicarToken();

            var httpResponse = await _http.PutAsJsonAsync($"{_baseUrl}/{periodo.IdPeriodo}", periodo);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API PUT PERIODO/{periodo.IdPeriodo}] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }


        public async Task<bool> EliminarAsync(int id)
        {
            AplicarToken();

            var httpResponse = await _http.DeleteAsync($"{_baseUrl}/{id}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API DELETE PERIODO/{id}] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }
    }
}
