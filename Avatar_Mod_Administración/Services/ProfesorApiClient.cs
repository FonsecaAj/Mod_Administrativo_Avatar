using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Avatar_Mod_Administración.Services
{
    public class ProfesorApiClient : IProfesorApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public ProfesorApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _baseUrl = $"{_config["Adm_Profesores:BaseUrl"]}/api/profesor";
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

        
        public async Task<IEnumerable<ProfesorDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<ProfesorDto>>>(_baseUrl);

            return response?.ResponseObject ?? new List<ProfesorDto>();
        }

     
        public async Task<ProfesorDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<ProfesorDto>>($"{_baseUrl}/{id}");

            return response?.ResponseObject;
        }

      
        public async Task<bool> CrearAsync(ProfesorDto profesor)
        {
            AplicarToken();

            var response = await _http.PostAsJsonAsync(_baseUrl, profesor);

            return response.IsSuccessStatusCode;
        }

       
        public async Task<bool> ActualizarAsync(ProfesorDto profesor)
        {
            AplicarToken();

            var response = await _http.PutAsJsonAsync($"{_baseUrl}/{profesor.IdProfesor}", profesor);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API PUT PROFESOR] Status: {response.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }

    
        public async Task<bool> EliminarAsync(int id)
        {
            AplicarToken();

            var response = await _http.DeleteAsync($"{_baseUrl}/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
