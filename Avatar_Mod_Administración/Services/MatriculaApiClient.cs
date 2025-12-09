using Avatar_Mod_Administración.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class MatriculaApiClient : IMatriculaApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public MatriculaApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            var baseUrl = _config["Mat_Matricula:BaseUrl"]
                          ?? throw new InvalidOperationException("Mat_Matricula:BaseUrl no configurado");

            // Queda algo como http://localhost:5049/api/matricula
            _baseUrl = $"{baseUrl.TrimEnd('/')}/api/matricula";
        }

        // ================= TOKEN =================

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

        // ================= LISTADO POR CURSO / GRUPO =================

        public async Task<IEnumerable<MatriculaDto>> ObtenerPorCursoYGrupoAsync(int idCurso, int idGrupo)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/{idCurso}/{idGrupo}";
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<MatriculaDto>();

                var data = await response.Content
                    .ReadFromJsonAsync<BusinessLogicResponse<IEnumerable<MatriculaDto>>>();

                return data?.ResponseObject ?? new List<MatriculaDto>();
            }
            catch
            {
                return new List<MatriculaDto>();
            }
        }

        // ================= LOOKUPS (PERIODOS / CURSOS / GRUPOS) =================

        public async Task<MatriculaLookupsResponse?> ObtenerLookupsAsync()
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/lookups");

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content
                    .ReadFromJsonAsync<BusinessLogicResponse<MatriculaLookupsResponse>>();

                return data?.ResponseObject;
            }
            catch
            {
                return null;
            }
        }

        // ================= CREAR =================

        public async Task<(bool ok, int statusCode, string message)> CrearAsync(MatriculaRequestDto request)
        {
            AplicarToken();

            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                // POST /api/matricula
                var response = await _http.PostAsync(_baseUrl, contenido);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";

                // En MAT2 pusimos StatusCode = 201 al crear
                return (status == 201, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // ================= ACTUALIZAR =================

        public async Task<(bool ok, int statusCode, string message)> ActualizarAsync(MatriculaRequestDto request)
        {
            AplicarToken();

            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                // PUT /api/matricula  (el ID va en el body)
                var response = await _http.PutAsync(_baseUrl, contenido);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                string message = doc.RootElement.GetProperty("message").GetString() ?? "";

                return (status == 200, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // ================= ELIMINAR =================

        public async Task<(bool ok, int statusCode, string message)> EliminarAsync(int id)
        {
            AplicarToken();

            try
            {
                // DELETE /api/matricula/{id}
                var response = await _http.DeleteAsync($"{_baseUrl}/{id}");
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
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
