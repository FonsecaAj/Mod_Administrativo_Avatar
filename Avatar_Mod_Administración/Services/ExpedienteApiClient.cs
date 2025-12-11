using System.Net.Http.Headers;
using System.Text.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class ExpedienteApiClient : IExpedienteApiClient
    {
        private readonly HttpClient _http;
        private readonly IAuthService _authService;

        public ExpedienteApiClient(HttpClient http, IAuthService authService)
        {
            _http = http;
            _authService = authService;
        }

        private void AplicarToken()
        {
            var sesion = _authService.ObtenerSesionActual();
            var token = sesion?.AccessToken;

            _http.DefaultRequestHeaders.Remove("Authorization");

            if (!string.IsNullOrWhiteSpace(token))
            {
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = token.Substring(7).Trim();

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ========== OBTENER TODOS ==========

        public async Task<IEnumerable<ExpedienteDto>> ObtenerTodosAsync()
        {
            AplicarToken();

            var lista = await _http.GetFromJsonAsync<IEnumerable<ExpedienteDto>>("expediente");
            return lista ?? Enumerable.Empty<ExpedienteDto>();
        }

        // ========== OBTENER POR ID ==========

        public async Task<ExpedienteRequestDto?> ObtenerPorIdAsync(string id)
        {
            AplicarToken();

            var exp = await _http.GetFromJsonAsync<ExpedienteRequestDto>($"expediente/{id}");
            return exp;
        }

        // ========== CREAR ==========

        public async Task<(bool ok, int statusCode, string message)> CrearAsync(ExpedienteRequestDto request)
        {
            AplicarToken();

            var response = await _http.PostAsJsonAsync("expediente", request);
            var raw = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Intentar interpretar como BusinessLogicResponse
                try
                {
                    var wrapper = JsonSerializer.Deserialize<
                        BusinessLogicResponse<object>
                    >(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (wrapper != null && wrapper.StatusCode != 0)
                        return (wrapper.StatusCode is 200 or 201, wrapper.StatusCode, wrapper.Message ?? "Operación exitosa");
                }
                catch
                {
                    // Ignorar y usar el texto plano
                }

                // Si no era BusinessLogicResponse, usar el raw
                return (true, (int)response.StatusCode,
                    string.IsNullOrWhiteSpace(raw) ? "Expediente creado correctamente." : raw);
            }

            return (false, (int)response.StatusCode,
                string.IsNullOrWhiteSpace(raw) ? "Error al crear el expediente." : raw);
        }

        // ========== ACTUALIZAR ==========

        public async Task<(bool ok, int statusCode, string message)> ActualizarAsync(ExpedienteRequestDto request)
        {
            AplicarToken();

            var response = await _http.PutAsJsonAsync("expediente", request);
            var raw = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var wrapper = JsonSerializer.Deserialize<
                        BusinessLogicResponse<object>
                    >(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (wrapper != null && wrapper.StatusCode != 0)
                        return (wrapper.StatusCode == 200, wrapper.StatusCode, wrapper.Message ?? "Operación exitosa");
                }
                catch
                {
                }

                return (true, (int)response.StatusCode,
                    string.IsNullOrWhiteSpace(raw) ? "Expediente actualizado correctamente." : raw);
            }

            return (false, (int)response.StatusCode,
                string.IsNullOrWhiteSpace(raw) ? "Error al actualizar el expediente." : raw);
        }

        // ========== ELIMINAR ==========

        public async Task<(bool ok, int statusCode, string message)> EliminarAsync(string id)
        {
            AplicarToken();

            var response = await _http.DeleteAsync($"expediente/{id}");
            var raw = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var wrapper = JsonSerializer.Deserialize<
                        BusinessLogicResponse<object>
                    >(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (wrapper != null && wrapper.StatusCode != 0)
                        return (wrapper.StatusCode == 200, wrapper.StatusCode, wrapper.Message ?? "Operación exitosa");
                }
                catch
                {
                }

                return (true, (int)response.StatusCode,
                    string.IsNullOrWhiteSpace(raw) ? "Expediente eliminado correctamente." : raw);
            }

            return (false, (int)response.StatusCode,
                string.IsNullOrWhiteSpace(raw) ? "Error al eliminar el expediente." : raw);
        }
    }
}
