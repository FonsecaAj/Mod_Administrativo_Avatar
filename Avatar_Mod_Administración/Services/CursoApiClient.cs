using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Avatar_Mod_Administración.Services
{
    public class CursoApiClient : ICursoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;
        private readonly string _accessToken;

        public CursoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            _baseUrl = $"{_config["Adm_Cursos:BaseUrl"]}/api/curso";
            _accessToken = _config["Adm_Cursos:AccessToken"] ?? string.Empty;
        }

      
        private void AplicarToken()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
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
