using Avatar_Mod_Administración.Entities;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _usr2ApiUrl;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UsuarioService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["USR1ApiUrl"] ?? "http://localhost:5205";
            _usr2ApiUrl = configuration["USR2ApiUrl"] ?? "http://localhost:5204";
            _logger = logger;
        }

        public async Task<List<Usuario>> ObtenerTodosAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios");

                var tokenLimpio = ObtenerTokenLimpio(token);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/usuario");
                request.Headers.Add("Authorization", $"Bearer {tokenLimpio}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener usuarios: {Status}, {Content}", response.StatusCode, errorContent);
                    return new List<Usuario>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Usuarios obtenidos exitosamente");

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var usuarios = JsonSerializer.Deserialize<List<Usuario>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Usuario>();

                    await EnriquecerUsuariosAsync(usuarios, token);
                    return usuarios;
                }

                return new List<Usuario>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return new List<Usuario>();
            }
        }

        private string ObtenerTokenLimpio(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return token;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring(7);
            }

            return token;
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/usuario/{email}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var usuario = JsonSerializer.Deserialize<Usuario>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (usuario != null)
                    {
                        await EnriquecerUsuariosAsync(new List<Usuario> { usuario }, token);
                    }

                    return usuario;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario");
                return null;
            }
        }

        // ✅ IGUAL QUE FACTURA: Devolver (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(UsuarioCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/usuario");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(dto);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear usuario: {Status}, {Content}", response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al crear usuario";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al crear usuario");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // ✅ Parsear BusinessLogicResponse igual que Factura
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Usuario creado exitosamente";

                _logger.LogInformation("Usuario creado. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // ✅ IGUAL QUE FACTURA: Devolver (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(string email, UsuarioCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario {Email}", email);

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/usuario/{email}");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(dto);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar usuario: {Status}, {Error}",
                        response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al actualizar usuario";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al actualizar usuario");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // ✅ Parsear BusinessLogicResponse igual que Factura
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Usuario actualizado exitosamente";

                _logger.LogInformation("Usuario {Email} actualizado. Estado {Status}: {Mensaje}", email, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // ✅ IGUAL QUE FACTURA: Devolver (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(string email, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/usuario/{email}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar usuario: {Status}, {Content}", response.StatusCode, errorContent);

                    // Intentar extraer el mensaje de error del BusinessLogicResponse
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al eliminar usuario";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al eliminar usuario");
                    }
                }

                var content = await response.Content.ReadAsStringAsync();

                // ✅ Parsear BusinessLogicResponse igual que Factura
                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Usuario eliminado exitosamente";

                _logger.LogInformation("Usuario {Email} eliminado. Estado {Status}: {Mensaje}", email, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public async Task<List<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipo, string token)
        {
            try
            {
                _logger.LogInformation("Filtrando usuarios");

                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(identificacion))
                    query.Add($"identificacion={Uri.EscapeDataString(identificacion)}");
                if (!string.IsNullOrWhiteSpace(nombre))
                    query.Add($"nombre={Uri.EscapeDataString(nombre)}");
                if (tipo.HasValue)
                    query.Add($"tipo={tipo.Value}");

                var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
                var url = $"{_apiUrl}/usuario/filtrar{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al filtrar usuarios: {Status}, {Content}", response.StatusCode, errorContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("Token no autorizado - posiblemente expirado");
                    }

                    return new List<Usuario>();
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parsear BusinessLogicResponse
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    var usuarios = JsonSerializer.Deserialize<List<Usuario>>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Usuario>();

                    _logger.LogInformation("Usuarios filtrados: {Count}", usuarios.Count);

                    await EnriquecerUsuariosAsync(usuarios, token);
                    return usuarios;
                }

                return new List<Usuario>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error excepcional al filtrar usuarios");
                return new List<Usuario>();
            }
        }

        public async Task<List<TipoIdentificacion>> ObtenerTiposIdentificacionAsync(string token)
        {
            try
            {
                var tipos = new List<TipoIdentificacion>();
                var idsComunes = new[] { 1, 2, 3 };

                var tasks = idsComunes.Select(async id =>
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/tipoidentificacion/{id}");
                        request.Headers.Add("Authorization", token);

                        var response = await _httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();

                            // Parsear BusinessLogicResponse
                            using var doc = JsonDocument.Parse(content);
                            if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                            {
                                return JsonSerializer.Deserialize<TipoIdentificacion>(responseObj.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al obtener tipo identificación {Id}", id);
                    }
                    return null;
                }).ToList();

                var resultados = await Task.WhenAll(tasks);
                tipos.AddRange(resultados.Where(t => t != null)!);

                return tipos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de identificación");
                return new List<TipoIdentificacion>();
            }
        }

        public async Task<List<Rol>> ObtenerRolesAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr2ApiUrl}/rol");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return new List<Rol>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Rol>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return new List<Rol>();
            }
        }

        private async Task EnriquecerUsuariosAsync(List<Usuario> usuarios, string token)
        {
            try
            {
                var rolesTask = ObtenerRolesAsync(token);
                var tiposTask = ObtenerTiposIdentificacionAsync(token);

                await Task.WhenAll(rolesTask, tiposTask);

                var roles = await rolesTask;
                var tipos = await tiposTask;

                var rolesDict = roles.ToDictionary(r => r.IdRol, r => r.Nombre);
                var tiposDict = tipos.ToDictionary(t => t.IdTipoIdentificacion, t => t.Nombre);

                foreach (var usuario in usuarios)
                {
                    usuario.RolNombre = rolesDict.TryGetValue(usuario.IdRol, out var rolNombre)
                        ? rolNombre
                        : "Desconocido";

                    usuario.TipoIdentificacion = tiposDict.TryGetValue(usuario.IdTipoIdentificacion, out var tipoNombre)
                        ? tipoNombre
                        : "Desconocido";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enriquecer usuarios");
            }
        }
    }
}