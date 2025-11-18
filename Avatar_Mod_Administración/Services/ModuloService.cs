using Avatar_Mod_Administración.Entities;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class ModuloService : IModuloService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usr4ApiUrl;
        private readonly ILogger<ModuloService> _logger;

        public ModuloService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ModuloService> logger)
        {
            _httpClient = httpClient;
            _usr4ApiUrl = configuration.GetValue<string>("USR4ApiUrl") ?? "http://localhost:5290";
            _logger = logger;
        }

        public async Task<List<Modulo>> ObtenerTodosAsync(string token, string? nombre = null)
        {
            try
            {
                var url = $"{_usr4ApiUrl}/modulo";
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    url += $"?nombre={Uri.EscapeDataString(nombre.Trim())}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener módulos: {StatusCode}", response.StatusCode);
                    return new List<Modulo>();
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse y extraer responseObject
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var modulos = JsonSerializer.Deserialize<List<Modulo>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Modulo>();

                    _logger.LogInformation("Obtenidos {Count} módulos", modulos.Count);
                    return modulos;
                }

                _logger.LogWarning("No se encontró responseObject en la respuesta");
                return new List<Modulo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos");
                return new List<Modulo>();
            }
        }

        public async Task<Modulo?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    return JsonSerializer.Deserialize<Modulo>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo {Id}", id);
                return null;
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(ModuloCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_usr4ApiUrl}/modulo");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear módulo: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al crear módulo";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al crear módulo");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Módulo creado exitosamente";

                _logger.LogInformation("Módulo creado. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear módulo");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, ModuloCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Actualizando módulo {Id}", id);

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar módulo: {Status}, {Error}",
                        response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al actualizar módulo";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al actualizar módulo");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Módulo actualizado exitosamente";

                _logger.LogInformation("Módulo {Id} actualizado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar módulo");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar módulo: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al eliminar módulo";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al eliminar módulo");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Módulo eliminado exitosamente";

                _logger.LogInformation("Módulo {Id} eliminado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar módulo");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }
    }
}