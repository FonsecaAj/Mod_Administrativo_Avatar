using Avatar_Mod_Administración.Entities;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class InstitucionService : IInstitucionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<InstitucionService> _logger;

        public InstitucionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InstitucionService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ACD1ApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/ACD1/";
            _logger = logger;
        }

        public async Task<List<Institucion>> ObtenerTodosAsync(string token, string? nombre = null)
        {
            try
            {
                var url = $"{_apiUrl}/institucion";

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrimmed = nombre.Trim();
                    url += $"?nombre={Uri.EscapeDataString(nombreTrimmed)}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                string messageFromApi = doc.RootElement.GetProperty("message").GetString() ?? "Error desconocido";

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener instituciones: {Status}, {Content}", response.StatusCode, content);
                    throw new Exception(messageFromApi);
                }

                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var instituciones = JsonSerializer.Deserialize<List<Institucion>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Institucion>();

                    return instituciones;
                }

                return new List<Institucion>();
            }
            catch (JsonException)
            {
                _logger.LogError("Error al parsear respuesta de la API");
                throw new Exception("Error al procesar la respuesta del servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener instituciones");
                throw;
            }
        }

        public async Task<Institucion?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                string messageFromApi = doc.RootElement.GetProperty("message").GetString() ?? "Error desconocido";

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener institución: {Status}, {Content}", response.StatusCode, content);
                    throw new Exception(messageFromApi);
                }

                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    return JsonSerializer.Deserialize<Institucion>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (JsonException)
            {
                _logger.LogError("Error al parsear respuesta de la API");
                throw new Exception("Error al procesar la respuesta del servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener institución");
                throw;
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(InstitucionCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/institucion");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { nombre = dto.Nombre });

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = JsonDocument.Parse(content);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Error desconocido";

                _logger.LogInformation("Institución creada. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (JsonException)
            {
                _logger.LogError("Error al parsear respuesta de la API");
                return (false, 500, "Error al procesar la respuesta del servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear institución");
                return (false, 500, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, InstitucionCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Actualizando institución {Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { nombre = dto.Nombre });

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = JsonDocument.Parse(content);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Error desconocido";

                _logger.LogInformation("Institución {Id} actualizada. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (JsonException)
            {
                _logger.LogError("Error al parsear respuesta de la API");
                return (false, 500, "Error al procesar la respuesta del servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar institución");
                return (false, 500, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = JsonDocument.Parse(content);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Error desconocido";

                _logger.LogInformation("Institución {Id} eliminada. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (JsonException)
            {
                _logger.LogError("Error al parsear respuesta de la API");
                return (false, 500, "Error al procesar la respuesta del servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar institución");
                return (false, 500, $"Error de conexión: {ex.Message}");
            }
        }
    }
}