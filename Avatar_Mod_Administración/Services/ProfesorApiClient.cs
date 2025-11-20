using Avatar_Mod_Administración.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
            try
            {
                var response = await _http.GetAsync(_baseUrl);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<ProfesorDto>();

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<ProfesorDto>>>();
                return data?.ResponseObject ?? new List<ProfesorDto>();
            }
            catch (HttpRequestException)
            {
                return new List<ProfesorDto>();
            }
        }

        public async Task<ProfesorDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<ProfesorDto>>();
                return data?.ResponseObject;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<bool> CrearAsync(ProfesorDto profesor)
        {
            AplicarToken();

            var response = await _http.PostAsJsonAsync(_baseUrl, profesor);

            return response.IsSuccessStatusCode;
        }
        public async Task<(bool ok, int statusCode, string message)> ActualizarAsync(ProfesorDto profesor)
        {
            AplicarToken();

            var response = await _http.PutAsJsonAsync($"{_baseUrl}/{profesor.IdProfesor}", profesor);

            var json = await response.Content.ReadAsStringAsync();

            try
            {
                var data = JsonSerializer.Deserialize<BusinessLogicResponse<object>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                if (data == null)
                    return (false, (int)response.StatusCode, "Error desconocido procesando la respuesta");

                return (data.StatusCode == 200, data.StatusCode, data.Message);
            }
            catch
            {
                return (false, (int)response.StatusCode, "No se pudo interpretar la respuesta del servidor.");
            }
        }


        public async Task<bool> EliminarAsync(int id)
        {
            AplicarToken();

            var response = await _http.DeleteAsync($"{_baseUrl}/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}