using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class ExpedienteApiClient : IExpedienteApiClient
    {
        private readonly HttpClient _http;
        private readonly IAuthService _authService;
        private readonly string _baseUrl;

        public ExpedienteApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _authService = authService;

            // BaseUrl = host del ApiMAT3, sin ruta
            _baseUrl = config["Mat_Expediente:BaseUrl"]
                       ?? throw new InvalidOperationException("Mat_Expediente:BaseUrl no configurado");
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

        // =============== CRUD EXPEDIENTES ===============

        public async Task<IEnumerable<ExpedienteDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/expediente");
                if (!response.IsSuccessStatusCode)
                    return new List<ExpedienteDto>();

                var data = await response.Content.ReadFromJsonAsync<IEnumerable<ExpedienteDto>>();
                return data ?? new List<ExpedienteDto>();
            }
            catch
            {
                return new List<ExpedienteDto>();
            }
        }

        public async Task<ExpedienteDto?> ObtenerPorIdAsync(string numeroIdentificacion)
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/expediente/{numeroIdentificacion}");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ExpedienteDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool ok, int statusCode, string message)> CrearAsync(ExpedienteDto expediente)
        {
            AplicarToken();

            try
            {
                var response = await _http.PostAsJsonAsync($"{_baseUrl}/expediente", expediente);
                var texto = await response.Content.ReadAsStringAsync();

                // En tu ApiMAT3 los POST devuelven solo un string tipo "Expediente creado correctamente."
                var msg = string.IsNullOrWhiteSpace(texto) ? "Respuesta vacía del servidor" : texto;

                return (response.IsSuccessStatusCode, (int)response.StatusCode, msg);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string message)> ActualizarAsync(ExpedienteDto expediente)
        {
            AplicarToken();

            try
            {
                var response = await _http.PutAsJsonAsync($"{_baseUrl}/expediente", expediente);
                var texto = await response.Content.ReadAsStringAsync();
                var msg = string.IsNullOrWhiteSpace(texto) ? "Respuesta vacía del servidor" : texto;

                return (response.IsSuccessStatusCode, (int)response.StatusCode, msg);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string message)> EliminarAsync(string numeroIdentificacion)
        {
            AplicarToken();

            try
            {
                var response = await _http.DeleteAsync($"{_baseUrl}/expediente/{numeroIdentificacion}");
                var texto = await response.Content.ReadAsStringAsync();
                var msg = string.IsNullOrWhiteSpace(texto) ? "Respuesta vacía del servidor" : texto;

                return (response.IsSuccessStatusCode, (int)response.StatusCode, msg);
            }
            catch (Exception ex)
            {
                return (false, 500, $"Error llamando al API: {ex.Message}");
            }
        }

        // =============== CATÁLOGOS DIRECCIÓN ===============

        public async Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync()
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/provincias");
                if (!response.IsSuccessStatusCode)
                    return new List<ProvinciaDto>();

                var data = await response.Content.ReadFromJsonAsync<IEnumerable<ProvinciaDto>>();
                return data ?? new List<ProvinciaDto>();
            }
            catch
            {
                return new List<ProvinciaDto>();
            }
        }

        public async Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia)
        {
            AplicarToken();

            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/cantones?provincia={idProvincia}");
                if (!response.IsSuccessStatusCode)
                    return new List<CantonDto>();

                var data = await response.Content.ReadFromJsonAsync<IEnumerable<CantonDto>>();
                return data ?? new List<CantonDto>();
            }
            catch
            {
                return new List<CantonDto>();
            }
        }

        public async Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton)
        {
            AplicarToken();

            try
            {
                var url = $"{_baseUrl}/distritos?provincia={idProvincia}&canton={idCanton}";
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<DistritoDto>();

                var data = await response.Content.ReadFromJsonAsync<IEnumerable<DistritoDto>>();
                return data ?? new List<DistritoDto>();
            }
            catch
            {
                return new List<DistritoDto>();
            }
        }
    }
}
