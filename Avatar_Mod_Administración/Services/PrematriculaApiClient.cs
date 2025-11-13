using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Avatar_Mod_Administración.Services
{
    public class PrematriculaApiClient : IPrematriculaApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _endpoint = "/api/prematricula";

        public PrematriculaApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            var accessToken = _config["Adm_Prematricula:AccessToken"];

            if (!string.IsNullOrWhiteSpace(accessToken))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<IEnumerable<PrematriculaDto>> ObtenerTodosAsync(PrematriculaFiltro filtro)
        {
            var url = $"{_endpoint}?";

            if (filtro.ID_Periodo.HasValue) url += $"idPeriodo={filtro.ID_Periodo}&";
            if (filtro.ID_Carrera.HasValue) url += $"idCarrera={filtro.ID_Carrera}&";
            if (filtro.ID_Curso.HasValue) url += $"idCurso={filtro.ID_Curso}&";
            if (!string.IsNullOrWhiteSpace(filtro.Estudiante))
                url += $"estudiante={Uri.EscapeDataString(filtro.Estudiante)}&";

            var response = await _http.GetFromJsonAsync<
                BusinessLogicResponse<IEnumerable<PrematriculaDto>>
            >(url);

            return response?.ResponseObject ?? Enumerable.Empty<PrematriculaDto>();
        }

        public async Task<PrematriculaDto?> ObtenerPorIdAsync(int id)
        {
            var response = await _http.GetFromJsonAsync<
                BusinessLogicResponse<PrematriculaDto>
            >($"{_endpoint}/{id}");

            return response?.ResponseObject;
        }

        public async Task<bool> CrearAsync(PrematriculaDto dto)
        {
            var response = await _http.PostAsJsonAsync(_endpoint, dto);

            var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<object>>();

            if (!response.IsSuccessStatusCode || data?.StatusCode != 200)
            {
                Console.WriteLine($"[PREMATRICULA CREAR] StatusCode HTTP: {(int)response.StatusCode}, " +
                                  $"StatusCode BL: {data?.StatusCode}, Mensaje: {data?.Message}");
                return false;
            }

            return true;
        }


        public async Task<bool> ActualizarAsync(int id, PrematriculaDto dto)
        {
            
            dto.ID_Prematricula = id; 

            
            var response = await _http.PutAsJsonAsync(_endpoint, dto);

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<object>>();
            return data?.StatusCode == 200;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var response = await _http.DeleteAsync($"{_endpoint}/{id}");

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<object>>();
            return data?.StatusCode == 200;
        }
    }
}
