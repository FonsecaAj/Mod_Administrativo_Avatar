using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class ExpedienteApiClient : IExpedienteApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;   // http://localhost:5094

        public ExpedienteApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            // MISMA clave que en Program: "Mat_Expediente:BaseUrl"
            _baseUrl = _config["Mat_Expediente:BaseUrl"]
                       ?? throw new InvalidOperationException("Mat_Expediente:BaseUrl no configurado");
        }

        // ============ Token ============

        private string ObtenerTokenLimpio()
        {
            var sesion = _authService.ObtenerSesionActual();
            if (sesion == null || string.IsNullOrWhiteSpace(sesion.AccessToken))
                return string.Empty;

            var token = sesion.AccessToken;
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token[7..].Trim();

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

        // ============ GET ============

        public async Task<IEnumerable<ExpedienteDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/expediente";
                var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                    return Enumerable.Empty<ExpedienteDto>();

                var data = await resp.Content.ReadFromJsonAsync<IEnumerable<ExpedienteDto>>();
                return data ?? Enumerable.Empty<ExpedienteDto>();
            }
            catch
            {
                return Enumerable.Empty<ExpedienteDto>();
            }
        }

        public async Task<ExpedienteDto?> ObtenerPorIdAsync(string numeroIdentificacion)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/expediente/{numeroIdentificacion}";
                var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                    return null;

                return await resp.Content.ReadFromJsonAsync<ExpedienteDto>();
            }
            catch
            {
                return null;
            }
        }

        // ============ POST ============

        public async Task<(bool ok, int statusCode, string message)> CrearAsync(ExpedienteRequestDto expediente)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/expediente";
                var resp = await _http.PostAsJsonAsync(url, expediente);
                var texto = await resp.Content.ReadAsStringAsync();

                int status = (int)resp.StatusCode;
                string message = string.IsNullOrWhiteSpace(texto)
                    ? (resp.IsSuccessStatusCode ? "Expediente creado correctamente." : "Error al crear expediente.")
                    : texto.Trim('"');

                return (resp.IsSuccessStatusCode, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }

        // ============ PUT ============

        public async Task<(bool ok, int statusCode, string message)> ActualizarAsync(ExpedienteRequestDto expediente)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/expediente";
                var resp = await _http.PutAsJsonAsync(url, expediente);
                var texto = await resp.Content.ReadAsStringAsync();

                int status = (int)resp.StatusCode;
                string message = string.IsNullOrWhiteSpace(texto)
                    ? (resp.IsSuccessStatusCode ? "Expediente actualizado correctamente." : "Error al actualizar expediente.")
                    : texto.Trim('"');

                return (resp.IsSuccessStatusCode, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }

        // ============ DELETE ============

        public async Task<(bool ok, int statusCode, string message)> EliminarAsync(string numeroIdentificacion)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/expediente/{numeroIdentificacion}";
                var resp = await _http.DeleteAsync(url);
                var texto = await resp.Content.ReadAsStringAsync();

                int status = (int)resp.StatusCode;
                string message = string.IsNullOrWhiteSpace(texto)
                    ? (resp.IsSuccessStatusCode ? "Expediente eliminado correctamente." : "Error al eliminar expediente.")
                    : texto.Trim('"');

                return (resp.IsSuccessStatusCode, status, message);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }
    }
}
