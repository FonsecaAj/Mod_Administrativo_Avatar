using Avatar_Mod_Administración.Entities;
using System.Net;
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

            // ⭐️ Se mantiene la construcción de la URL base
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

        // --- MÉTODOS DE OBTENCIÓN (MODIFICADOS PARA MANEJAR 404 Y ERRORES) ---

        public async Task<IEnumerable<CursoDto>> ObtenerTodosAsync()
        {
            AplicarToken();
            try
            {
                // Usamos GetAsync para manejar el estado antes de deserializar
                var response = await _http.GetAsync(_baseUrl);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<CursoDto>(); // Devolver vacío en lugar de crashear

                // Lanza excepción si no es 2xx
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>();
                return data?.ResponseObject ?? new List<CursoDto>();
            }
            catch (HttpRequestException)
            {
                // Capturar otros errores HTTP (como 401, 500)
                return new List<CursoDto>();
            }
        }

        public async Task<IEnumerable<CursoDto>> ObtenerPorCarreraAsync(int idCarrera)
        {
            AplicarToken();
            var url = $"{_baseUrl}/carrera/{idCarrera}";
            try
            {
                var response = await _http.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<CursoDto>();

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<CursoDto>>>();
                return data?.ResponseObject ?? new List<CursoDto>();
            }
            catch (HttpRequestException)
            {
                return new List<CursoDto>();
            }
        }

        public async Task<CursoDto?> ObtenerPorIdAsync(int id)
        {
            AplicarToken();
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/{id}");

                // Si no se encuentra, retornamos null
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<CursoDto>>();
                return data?.ResponseObject;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // ⭐️ MÉTODO ESPECÍFICO CORREGIDO ⭐️
        public async Task<LookupsResponse?> ObtenerLookupsAsync()
        {
            AplicarToken();
            // Asegúrate de que la URL aquí es correcta. 
            // Si el 404 persiste, la URL debe ser ajustada (e.g., quitar _baseUrl si ya está configurado)
            var url = $"{_baseUrl}/lookups";
            try
            {
                // 1. Usar GetAsync para obtener la respuesta sin lanzar excepción inmediatamente
                var response = await _http.GetAsync(url);

                // 2. Verificar específicamente el 404 (Not Found)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Devolver null en lugar de lanzar una excepción
                    return null;
                }

                // 3. Lanzar excepción si hay otros errores (401, 500, etc.)
                response.EnsureSuccessStatusCode();

                // 4. Si es exitoso (2xx), leer el contenido
                var data = await response.Content.ReadFromJsonAsync<BusinessLogicResponse<LookupsResponse>>();
                return data?.ResponseObject;
            }
            catch (HttpRequestException ex)
            {
                // Capturar otros fallos de la red o del servidor
                Console.WriteLine($"[ERROR API LOOKUPS] Detalle: {ex.Message}");
                return null;
            }
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
    }
}