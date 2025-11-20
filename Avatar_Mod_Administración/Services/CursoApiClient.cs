using Avatar_Mod_Administración.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

            _http.DefaultRequestHeaders.Remove("Authorization");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        
        public async Task<IEnumerable<CursoDto>> ObtenerPorCarreraAsync(int idCarrera)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/carrera/{idCarrera}";
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<CursoDto>();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>();
                return data?.ResponseObject ?? new List<CursoDto>();
            }
            catch
            {
                return new List<CursoDto>();
            }
        }

        public async Task<IEnumerable<CursoDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync(_baseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<CursoDto>();
                }

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>();
                return data?.ResponseObject ?? new List<CursoDto>();
            }
            catch
            {
                return new List<CursoDto>();
            }
        }

        

        public async Task<LookupsResponse?> ObtenerLookupsAsync()
        {
            AplicarToken();

            var url = $"{_baseUrl}/lookups";

            try
            {
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<LookupsResponse>>();
                return data?.ResponseObject;
            }
            catch
            {
                return null;
            }
        }

        
        public async Task<CursoDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<CursoDto>>();
                return data?.ResponseObject;
            }
            catch
            {
                return null;
            }
        }

        

        public async Task<(bool ok, int statusCode, string message)>
            CrearAsync(CursoDto curso)
        {
            AplicarToken();

            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(curso),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _http.PostAsync(_baseUrl, contenido);

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";

                return (status == 201, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        

        public async Task<(bool ok, int statusCode, string message)>
            ActualizarAsync(CursoDto curso)
        {
            AplicarToken();

            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(curso),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _http.PutAsync($"{_baseUrl}/{curso.ID_Curso}", contenido);

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        

        public async Task<(bool ok, int statusCode, string message)>
            EliminarAsync(int id)
        {
            AplicarToken();

            try
            {
                var response = await _http.DeleteAsync($"{_baseUrl}/{id}");

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }
    }
}
