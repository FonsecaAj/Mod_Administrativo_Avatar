using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class PeriodoApiClient : IPeriodoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;
        private readonly string _accessToken;

        public PeriodoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            _baseUrl = $"{_config["Adm_Periodos:BaseUrl"]}/api/periodo";
            _accessToken = _config["Adm_Periodos:AccessToken"] ?? string.Empty;
        }

        private void AplicarToken()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
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

         
            var httpResponse = await _http.PutAsJsonAsync(_baseUrl, periodo);

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
