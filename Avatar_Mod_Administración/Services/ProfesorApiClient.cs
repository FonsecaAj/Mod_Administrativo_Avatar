
using System.Net.Http.Headers;

using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class ProfesorApiClient : IProfesorApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;
        private readonly string _accessToken;

        public ProfesorApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

       
            _baseUrl = $"{_config["Adm_Profesores:BaseUrl"]}/api/profesor";
            _accessToken = _config["Adm_Profesores:AccessToken"] ?? string.Empty;
        }

        private void AplicarToken()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }

    
        public async Task<IEnumerable<ProfesorDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            var httpResponse = await _http.GetAsync(_baseUrl);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET TODOS PROFESOR] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return new List<ProfesorDto>();
            }

            var wrapper = await httpResponse.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<ProfesorDto>>>();

            return wrapper?.ResponseObject ?? new List<ProfesorDto>();
        }

     
        public async Task<ProfesorDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            var httpResponse = await _http.GetAsync($"{_baseUrl}/{id}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET PROFESOR/{id}] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return null;
            }

            var wrapper = await httpResponse.Content
                .ReadFromJsonAsync<BusinessLogicResponse<ProfesorDto>>();

            return wrapper?.ResponseObject;
        }

        public async Task<bool> CrearAsync(ProfesorDto profesor)
        {
            AplicarToken();

            var httpResponse = await _http.PostAsJsonAsync(_baseUrl, profesor);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API POST PROFESOR] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }

        public async Task<bool> ActualizarAsync(ProfesorDto profesor)
        {
            AplicarToken();

          
            var httpResponse = await _http.PutAsJsonAsync(_baseUrl, profesor);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var detalle = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API PUT PROFESOR] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
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
                Console.WriteLine($"[ERROR API DELETE PROFESOR/{id}] Status: {httpResponse.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }
    }
}
