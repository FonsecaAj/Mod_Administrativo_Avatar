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
                _logger.LogInformation("Obteniendo todos los usuarios - Token length: {TokenLength}", token?.Length);

                var tokenLimpio = ObtenerTokenLimpio(token);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/usuario");
                request.Headers.Add("Authorization", $"Bearer {tokenLimpio}");

                _logger.LogInformation("Enviando request a: {Url}", $"{_apiUrl}/usuario");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener usuarios: {Status}, {Content}", response.StatusCode, errorContent);
                    return new List<Usuario>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Usuarios obtenidos exitosamente: {ContentLength} chars", content.Length);

                var usuarios = JsonSerializer.Deserialize<List<Usuario>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Usuario>();

                await EnriquecerUsuariosAsync(usuarios, token);
                return usuarios;
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
                return token.Substring(7).Trim();
            }

            return token.Trim();
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
                var usuario = JsonSerializer.Deserialize<Usuario>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (usuario != null)
                {
                    await EnriquecerUsuariosAsync(new List<Usuario> { usuario }, token);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario");
                return null;
            }
        }

        public async Task<bool> CrearAsync(UsuarioCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/usuario");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(dto);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(string email, UsuarioCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario {Email} - Contraseña provista: {TieneContrasenna}",
                    email, !string.IsNullOrEmpty(dto.Contrasenna));

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/usuario/{email}");
                request.Headers.Add("Authorization", token);

                // Siempre enviar el DTO completo
                // El backend se encarga de manejar la contraseña vacía
                request.Content = JsonContent.Create(dto);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar usuario: {Status}, {Error}",
                        response.StatusCode, errorContent);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(string email, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/usuario/{email}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return false;
            }
        }

        public async Task<List<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipo, string token)
        {
            try
            {
                _logger.LogInformation("Intentando filtrar usuarios con token: {TokenLength}", token?.Length);

                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(identificacion))
                    query.Add($"identificacion={Uri.EscapeDataString(identificacion)}");
                if (!string.IsNullOrWhiteSpace(nombre))
                    query.Add($"nombre={Uri.EscapeDataString(nombre)}");
                if (tipo.HasValue)
                    query.Add($"tipo={tipo.Value}");

                var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
                var url = $"{_apiUrl}/usuario/filtrar{queryString}";

                _logger.LogInformation("URL de filtrado: {Url}", url);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("Respuesta de filtrado: {StatusCode}", response.StatusCode);

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
                _logger.LogInformation("Contenido recibido: {ContentLength} caracteres", content.Length);

                var usuarios = JsonSerializer.Deserialize<List<Usuario>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Usuario>();

                _logger.LogInformation("Usuarios deserializados: {Count}", usuarios.Count);

                await EnriquecerUsuariosAsync(usuarios, token);
                return usuarios;
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

                foreach (var id in idsComunes)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/tipoidentificacion/{id}");
                    request.Headers.Add("Authorization", token);

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var tipo = JsonSerializer.Deserialize<TipoIdentificacion>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (tipo != null)
                            tipos.Add(tipo);
                    }
                }

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
                var roles = await ObtenerRolesAsync(token);
                var tipos = await ObtenerTiposIdentificacionAsync(token);

                foreach (var usuario in usuarios)
                {
                    var rol = roles.FirstOrDefault(r => r.IdRol == usuario.IdRol);
                    usuario.RolNombre = rol?.Nombre ?? "Desconocido";

                    var tipo = tipos.FirstOrDefault(t => t.IdTipoIdentificacion == usuario.IdTipoIdentificacion);
                    usuario.TipoIdentificacion = tipo?.Nombre ?? "Desconocido";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enriquecer usuarios");
            }
        }
    }
}