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
            _usr4ApiUrl = configuration.GetValue<string>("USR4ApiUrl") ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR4/";
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
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        string msg = doc.RootElement.GetProperty("message").GetString() ?? "Error desconocido";
                        throw new Exception(msg);
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Error {response.StatusCode}");
                    }
                }

                // Parsear BusinessLogicResponse y extraer responseObject
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var modulos = JsonSerializer.Deserialize<List<Modulo>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Modulo>();

                    return modulos;
                }

                return new List<Modulo>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener módulos");
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<Modulo?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        string msg = doc.RootElement.GetProperty("message").GetString() ?? "Error desconocido";
                        throw new Exception(msg);
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Error {response.StatusCode}");
                    }
                }

                // Parsear BusinessLogicResponse
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    return JsonSerializer.Deserialize<Modulo>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener módulo {Id}", id);
                throw new Exception("Error de conexión con el servidor");
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
                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = JsonDocument.Parse(responseContent);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? string.Empty;

                return (response.IsSuccessStatusCode, statusCode, msg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al crear módulo");
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, ModuloCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = JsonDocument.Parse(responseContent);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? string.Empty;

                return (response.IsSuccessStatusCode, statusCode, msg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al actualizar módulo {Id}", id);
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = JsonDocument.Parse(responseContent);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? string.Empty;

                return (response.IsSuccessStatusCode, statusCode, msg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al eliminar módulo {Id}", id);
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }
    }
}