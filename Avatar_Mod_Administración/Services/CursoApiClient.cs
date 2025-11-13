using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Avatar_Mod_Administración.Services
{
    public class CursoApiClient : ICursoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public CursoApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _baseUrl = $"{_config["Adm_Cursos:BaseUrl"]}/api/curso";
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

        public async Task<IEnumerable<CursoDto>> ObtenerTodosAsync()
        {
            AplicarToken();
            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>(_baseUrl);
            return response?.ResponseObject ?? new List<CursoDto>();
        }

        public async Task<IEnumerable<CursoDto>> ObtenerPorCarreraAsync(int idCarrera)
        {
            AplicarToken();
            var url = $"{_baseUrl}/carrera/{idCarrera}";
            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>(url);
            return response?.ResponseObject ?? new List<CursoDto>();
        }

        public async Task<CursoDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();
            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<CursoDto>>($"{_baseUrl}/{id}");
            return response?.ResponseObject;
        }

        public async Task<bool> CrearAsync(CursoDto curso)
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync(_baseUrl, curso);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarAsync(CursoDto curso)
        {
            AplicarToken();
            var response = await _http.PutAsJsonAsync($"{_baseUrl}/{curso.ID_Curso}", curso);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR API PUT] Status: {response.StatusCode}, Detalle: {detalle}");
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

        public async Task<LookupsResponse?> ObtenerLookupsAsync()
        {
            AplicarToken();
            var response = await _http.GetFromJsonAsync<BusinessLogicResponse<LookupsResponse>>($"{_baseUrl}/lookups");
            return response?.ResponseObject;
        }
    }
}
