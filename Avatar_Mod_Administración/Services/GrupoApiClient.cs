using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class GrupoApiClient : IGrupoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public GrupoApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _baseUrl = $"{_config["Adm_Grupos:BaseUrl"]}/api/grupo";
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

    
        public async Task<IEnumerable<GrupoDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            var response = await _http.GetAsync(_baseUrl);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET GRUPOS] Status: {response.StatusCode}, Detalle: {detalle}");
                return new List<GrupoDto>();
            }

            var wrapper = await response.Content
                .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<GrupoDto>>>();

            return wrapper?.ResponseObject ?? new List<GrupoDto>();
        }

        
        public async Task<GrupoDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            var response = await _http.GetAsync($"{_baseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API GET GRUPO/{id}] Status: {response.StatusCode}, Detalle: {detalle}");
                return null;
            }

            var wrapper = await response.Content
                .ReadFromJsonAsync<BusinessLogicResponse<GrupoDto>>();

            return wrapper?.ResponseObject;
        }

      
        public async Task<bool> CrearAsync(GrupoDto grupo)
        {
            AplicarToken();

            var response = await _http.PostAsJsonAsync(_baseUrl, grupo);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API POST GRUPO] Status: {response.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }

        public async Task<bool> ActualizarAsync(GrupoDto grupo)
        {
            AplicarToken();

            var response = await _http.PutAsJsonAsync($"{_baseUrl}/{grupo.IdGrupo}", grupo);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API PUT GRUPO/{grupo.IdGrupo}] Status: {response.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }

      
        public async Task<bool> EliminarAsync(int id)
        {
            AplicarToken();

            var response = await _http.DeleteAsync($"{_baseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API DELETE GRUPO/{id}] Status: {response.StatusCode}, Detalle: {detalle}");
                return false;
            }

            return true;
        }
    }
}
