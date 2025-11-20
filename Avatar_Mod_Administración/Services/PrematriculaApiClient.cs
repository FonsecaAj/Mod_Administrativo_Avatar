using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Avatar_Mod_Administración.Services
{
    public class PrematriculaApiClient : IPrematriculaApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _endpoint;

        public PrematriculaApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _endpoint = $"{_config["Adm_Prematricula:BaseUrl"]}/api/prematricula";
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

        public async Task<IEnumerable<PrematriculaDto>> ObtenerTodosAsync(PrematriculaFiltro filtro)
        {
            AplicarToken();

            var url = $"{_endpoint}?";

            if (filtro.ID_Periodo.HasValue) url += $"idPeriodo={filtro.ID_Periodo}&";
            if (filtro.ID_Carrera.HasValue) url += $"idCarrera={filtro.ID_Carrera}&";
            if (filtro.ID_Curso.HasValue) url += $"idCurso={filtro.ID_Curso}&";

            if (!string.IsNullOrWhiteSpace(filtro.Estudiante))
                url += "estudiante=" + Uri.EscapeDataString(filtro.Estudiante) + "&";

            var response = await _http.GetFromJsonAsync<
                BusinessLogicResponse<IEnumerable<PrematriculaDto>>
            >(url);

            return response?.ResponseObject ?? Enumerable.Empty<PrematriculaDto>();
        }

        public async Task<PrematriculaDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            var response = await _http.GetFromJsonAsync<
                BusinessLogicResponse<PrematriculaDto>
            >($"{_endpoint}/{id}");

            return response?.ResponseObject;
        }

        public async Task<bool> CrearAsync(PrematriculaDto dto)
        {
            AplicarToken();

            var response = await _http.PostAsJsonAsync(_endpoint, dto);
            var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<object>>();

            return response.IsSuccessStatusCode && data?.StatusCode == 201;
        }

        public async Task<bool> ActualizarAsync(int id, PrematriculaDto dto)
        {
            AplicarToken();

            dto.ID_Prematricula = id;

            
            var response = await _http.PutAsJsonAsync($"{_endpoint}/{id}", dto);

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response
                .Content
                .ReadFromJsonAsync<BusinessLogicResponse<object>>();

            return data?.StatusCode == 200;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            AplicarToken();

            var response = await _http.DeleteAsync($"{_endpoint}/{id}");

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response
                .Content
                .ReadFromJsonAsync<BusinessLogicResponse<object>>();

            return data?.StatusCode == 200;
        }
    }
}
