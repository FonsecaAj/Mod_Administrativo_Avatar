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
            _apiUrl = configuration["ACD1ApiUrl"] ?? "http://localhost:5001";
            _logger = logger;
        }

        public async Task<List<Institucion>> ObtenerTodosAsync(string token, string? nombre = null)
        {
            try
            {
                var url = $"{_apiUrl}/institucion";

                // Construir query string solo si hay búsqueda
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrimmed = nombre.Trim();
                    url += $"?nombre={Uri.EscapeDataString(nombreTrimmed)}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener instituciones: {Status}, {Content}", response.StatusCode, errorContent);
                    return new List<Institucion>();
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener instituciones");
                return new List<Institucion>();
            }
        }

        public async Task<Institucion?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener institución: {Status}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    return JsonSerializer.Deserialize<Institucion>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener institución");
                return null;
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear institución: {Status}, {Content}", response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al crear institución";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al crear institución");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Institución creada exitosamente";

                _logger.LogInformation("Institución creada. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear institución");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar institución: {Status}, {Error}",
                        response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al actualizar institución";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al actualizar institución");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Institución actualizada exitosamente";

                _logger.LogInformation("Institución {Id} actualizada. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar institución");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Devolver (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar institución: {Status}, {Content}", response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al eliminar institución";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al eliminar institución");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Institución eliminada exitosamente";

                _logger.LogInformation("Institución {Id} eliminada. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar institución");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }
    }
}