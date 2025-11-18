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
            _usr2ApiUrl = configuration["USR2ApiUrl"] ?? "http://localhost:5009";
            _logger = logger;
        }

        public async Task<List<RolApi>> ObtenerTodosAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol");
                request.Headers.Add("Authorization", token);

                _logger.LogDebug("Solicitando roles a: {Url}", $"{_usr2ApiUrl}/rol");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener roles: {StatusCode}", response.StatusCode);
                    return new List<RolApi>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<RolApi>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RolApi>();

                _logger.LogInformation("Obtenidos {Count} roles", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return new List<RolApi>();
            }
        }

        public async Task<RolApi?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener rol {Id}: {StatusCode}", id, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RolApi>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol {Id}", id);
                return null;
            }
        }

        // Devolver (ok, status, message)
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear rol: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al crear rol";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al crear rol");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Rol creado exitosamente";

                _logger.LogInformation("Rol creado. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Devolver (ok, status, message)
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar rol: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al actualizar rol";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al actualizar rol");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Rol actualizado exitosamente";

                _logger.LogInformation("Rol {Id} actualizado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol {Id}", id);
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Devolver (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_usr2ApiUrl}/rol/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar rol: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al eliminar rol";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al eliminar rol");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Rol eliminado exitosamente";

                _logger.LogInformation("Rol {Id} eliminado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol {Id}", id);
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Los métodos de consulta (GET) no cambian
        public async Task<List<RolModuloDetalleDto>> ObtenerModulosPorRolAsync(int idRol, string token)
        {
            try
            {
                var url = $"{_usr2ApiUrl}/rol/{idRol}/modulos";

                _logger.LogInformation("Solicitando módulos para rol {IdRol} a: {Url}", idRol, url);

                var authToken = token;
                if (!string.IsNullOrEmpty(token) && !token.StartsWith("Bearer "))
                {
                    authToken = $"Bearer {token}";
                    _logger.LogDebug("Se agrego prefijo 'Bearer ' al token");
                }

                _logger.LogDebug("Token enviado: {Token}",
                    authToken.Substring(0, Math.Min(40, authToken.Length)) + "...");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al obtener módulos del rol {IdRol}: {StatusCode} - {Error}",
                        idRol, response.StatusCode, errorContent);

                    return new List<RolModuloDetalleDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta de USR2: {Content}",
                    content.Substring(0, Math.Min(200, content.Length)));

                var modulos = JsonSerializer.Deserialize<List<RolModuloDetalleDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RolModuloDetalleDto>();

                _logger.LogInformation("Obtenidos {Count} módulos para rol {IdRol}", modulos.Count, idRol);

                return modulos;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión al obtener módulos del rol {IdRol}", idRol);
                return new List<RolModuloDetalleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener módulos del rol {IdRol}", idRol);
                return new List<RolModuloDetalleDto>();
            }
        }

        public async Task<bool> AsignarModulosAsync(int idRol, List<int> idsModulos, string token)
        {
            try
            {
                var dto = new { IdsModulos = idsModulos };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Asignando {Count} módulos al rol {IdRol}", idsModulos.Count, idRol);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_usr2ApiUrl}/rol/{idRol}/modulos");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var exito = response.IsSuccessStatusCode;

                if (exito)
                    _logger.LogInformation("Modulos asignados exitosamente al rol {IdRol}", idRol);
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al asignar módulos: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                }

                return exito;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar módulos al rol {IdRol}", idRol);
                return false;
            }
        }

        public async Task<MenuDto?> ObtenerMenuPorRolAsync(int idRol, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol/{idRol}/menu");
                request.Headers.Add("Authorization", token);

                _logger.LogDebug("Solicitando menú para rol {IdRol}", idRol);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener menú del rol {IdRol}: {StatusCode}",
                        idRol, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var menu = JsonSerializer.Deserialize<MenuDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Menú obtenido para rol {IdRol}", idRol);
                return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener menú del rol {IdRol}", idRol);
                return null;
            }
        }
    }
}