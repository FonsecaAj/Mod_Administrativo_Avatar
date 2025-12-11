using Avatar_Mod_Administración.Entities;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class RolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usr2ApiUrl;
        private readonly ILogger<RolService> _logger;

        public RolService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RolService> logger)
        {
            _httpClient = httpClient;
            _usr2ApiUrl = configuration["USR2ApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR2/";
            _logger = logger;
        }

        public async Task<List<RolApi>> ObtenerTodosAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol");
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

                var roles = JsonSerializer.Deserialize<List<RolApi>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RolApi>();

                return roles;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener roles");
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<RolApi?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol/{id}");
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

                return JsonSerializer.Deserialize<RolApi>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener rol {Id}", id);
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(RolCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_usr2ApiUrl}/rol");
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
                _logger.LogError(ex, "Error de conexión al crear rol");
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, RolCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_usr2ApiUrl}/rol/{id}");
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
                _logger.LogError(ex, "Error de conexión al actualizar rol {Id}", id);
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
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_usr2ApiUrl}/rol/{id}");
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
                _logger.LogError(ex, "Error de conexión al eliminar rol {Id}", id);
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<List<RolModuloDetalleDto>> ObtenerModulosPorRolAsync(int idRol, string token)
        {
            try
            {
                var url = $"{_usr2ApiUrl}/rol/{idRol}/modulos";

                var authToken = token;
                if (!string.IsNullOrEmpty(token) && !token.StartsWith("Bearer "))
                {
                    authToken = $"Bearer {token}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

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

                var modulos = JsonSerializer.Deserialize<List<RolModuloDetalleDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RolModuloDetalleDto>();

                return modulos;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener módulos del rol {IdRol}", idRol);
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<bool> AsignarModulosAsync(int idRol, List<int> idsModulos, string token)
        {
            try
            {
                var dto = new { IdsModulos = idsModulos };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_usr2ApiUrl}/rol/{idRol}/modulos");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        string msg = doc.RootElement.GetProperty("message").GetString() ?? "Error desconocido";
                        throw new Exception(msg);
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Error {response.StatusCode}");
                    }
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al asignar módulos al rol {IdRol}", idRol);
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<MenuDto?> ObtenerMenuPorRolAsync(int idRol, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol/{idRol}/menu");
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

                return JsonSerializer.Deserialize<MenuDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener menú del rol {IdRol}", idRol);
                throw new Exception("Error de conexión con el servidor");
            }
        }
    }
}