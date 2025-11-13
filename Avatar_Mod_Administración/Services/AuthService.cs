using Avatar_Mod_Administración.Entities;
using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiUrl;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiUrl = configuration["USR5ApiUrl"] ?? "http://localhost:5233";
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string contrasenna)
        {
            try
            {
                _logger.LogInformation("Intentando login para: {Email}", email);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/login");
                request.Headers.Add("usuario", email);
                request.Headers.Add("contrasenna", contrasenna);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta de login recibida: {Content}", content.Substring(0, Math.Min(100, content.Length)));

                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
                    };

                    var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(content, options);

                    if (loginResponse == null)
                    {
                        _logger.LogError("No se pudo deserializar la respuesta de login");
                        return null;
                    }

                    _logger.LogInformation("Login exitoso para: {Email}", email);

                    return new LoginResponse
                    {
                        ExpiresIn = loginResponse.ExpiresIn,
                        AccessToken = loginResponse.AccessToken,
                        RefreshToken = loginResponse.RefreshToken,
                        UsuarioID = loginResponse.UsuarioID
                    };
                }

                _logger.LogWarning("Login fallido para: {Email}, Status: {Status}", email, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoginAsync para: {Email}", email);
                return null;
            }
        }

        public async Task<RefreshResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Renovando token...");

                var request = new { refresh_token = refreshToken };
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}/refresh", request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
                    };

                    var refreshResponse = System.Text.Json.JsonSerializer.Deserialize<RefreshResponse>(content, options);
                    _logger.LogInformation("Token renovado exitosamente");
                    return refreshResponse;
                }

                _logger.LogWarning("Fallo al renovar token. Status: {Status}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RefreshTokenAsync");
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();

                var request = new { token = tokenLimpio };
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}/validate", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ValidateTokenAsync");
                return false;
            }
        }

        public void GuardarSesion(LoginResponse response, bool recordar)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                _logger.LogError("No se pudo obtener la sesión del HttpContext");
                return;
            }

            // Guardar el token con Bearer
            var tokenConBearer = response.AccessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? response.AccessToken
                : $"Bearer {response.AccessToken}";

            _logger.LogInformation("Guardando sesión para usuario: {UsuarioID}", response.UsuarioID);
            _logger.LogDebug("Token guardado (primeros 50 chars): {Token}",
                tokenConBearer.Substring(0, Math.Min(50, tokenConBearer.Length)));

            session.SetString("AccessToken", tokenConBearer);
            session.SetString("RefreshToken", response.RefreshToken);
            session.SetString("UsuarioID", response.UsuarioID);
            session.SetString("ExpiresIn", response.ExpiresIn.ToString("o"));

            // Guardar cookies solo si recordar = true
            if (recordar)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = false, // Cambiar a true en producción con HTTPS
                    SameSite = SameSiteMode.Lax
                };

                _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", response.RefreshToken, cookieOptions);
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("UsuarioID", response.UsuarioID, cookieOptions);

                _logger.LogInformation("Cookies de sesión guardadas (7 días) para: {UsuarioID}", response.UsuarioID);
            }
            else
            {
                // Asegurar que no haya cookies si no se marcó recordar
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");

                _logger.LogInformation("Sesión guardada SIN cookies para: {UsuarioID}", response.UsuarioID);
            }
        }

        public LoginResponse? ObtenerSesionActual()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                _logger.LogWarning("No se pudo obtener la sesión del HttpContext");
                return null;
            }

            var accessToken = session.GetString("AccessToken");
            var refreshToken = session.GetString("RefreshToken");
            var usuarioID = session.GetString("UsuarioID");
            var expiresInStr = session.GetString("ExpiresIn");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogDebug("No hay sesión activa (token o refresh token vacíos)");
                return null;
            }

            _logger.LogDebug("Sesión recuperada para usuario: {UsuarioID}", usuarioID);
            _logger.LogDebug("Token recuperado (primeros 50 chars): {Token}",
                accessToken.Substring(0, Math.Min(50, accessToken.Length)));

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UsuarioID = usuarioID ?? "",
                ExpiresIn = DateTime.TryParse(expiresInStr, out var expiry) ? expiry : DateTime.UtcNow
            };
        }

        // Restaurar sesión desde cookies
        public async Task<bool> RestaurarSesionDesdeCookiesAsync()
        {
            try
            {
                var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
                if (cookies == null)
                {
                    _logger.LogDebug("No hay cookies disponibles");
                    return false;
                }

                var refreshToken = cookies["RefreshToken"];
                var usuarioID = cookies["UsuarioID"];

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(usuarioID))
                {
                    _logger.LogDebug("No hay cookies de sesión guardadas");
                    return false;
                }

                _logger.LogInformation("Intentando restaurar sesión desde cookies para: {UsuarioID}", usuarioID);

                // Intentar renovar el token con el refresh token de la cookie
                var refreshResponse = await RefreshTokenAsync(refreshToken);
                if (refreshResponse == null)
                {
                    _logger.LogWarning("No se pudo restaurar sesión - refresh token inválido o expirado");

                    // Limpiar cookies inválidas
                    _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                    _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");

                    return false;
                }

                // Crear nueva sesión con los tokens renovados
                var nuevaSesion = new LoginResponse
                {
                    AccessToken = refreshResponse.AccessToken,
                    RefreshToken = refreshResponse.RefreshToken,
                    UsuarioID = usuarioID,
                    ExpiresIn = refreshResponse.ExpiresIn
                };

                // Guardar sesión y mantener las cookies
                GuardarSesion(nuevaSesion, true);

                _logger.LogInformation("Sesión restaurada exitosamente desde cookies para: {UsuarioID}", usuarioID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar sesión desde cookies");

                // Limpiar cookies en caso de error
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");

                return false;
            }
        }

        public async Task CerrarSesionAsync()
        {
            _logger.LogInformation("Cerrando sesión");

            try
            {
                // Obtener el token actual antes de limpiar la sesión
                var sesion = ObtenerSesionActual();

                if (sesion != null && !string.IsNullOrEmpty(sesion.AccessToken))
                {
                    _logger.LogInformation("Llamando al endpoint /logout en USR5 para: {UsuarioID}", sesion.UsuarioID);

                    // Llamar al endpoint de logout en USR5
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/logout");
                    request.Headers.Add("Authorization", sesion.AccessToken);

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Logout exitoso en servidor USR5");
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Logout fallo en servidor USR5. Status: {StatusCode}, Response: {Content}",
                            response.StatusCode, content);
                    }
                }
                else
                {
                    _logger.LogWarning("No hay token disponible para logout en servidor");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar al endpoint de logout en USR5");
            }
            finally
            {
                // limpiar la sesión local y las cookies, incluso si falla el logout en el servidor
                _httpContextAccessor.HttpContext?.Session.Clear();
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");

                _logger.LogInformation("Sesión local y cookies limpiadas completamente");
            }
        }

        public async Task<bool> RenovarSesionAutomaticaAsync()
        {
            var sesion = ObtenerSesionActual();
            if (sesion == null)
            {
                _logger.LogDebug("No hay sesión para renovar");
                return false;
            }

            var tokenValido = await ValidateTokenAsync(sesion.AccessToken);
            if (tokenValido)
            {
                _logger.LogDebug("Token aún válido, no es necesario renovar");
                return true;
            }

            _logger.LogInformation("Token inválido o expirado, intentando renovar...");

            var refreshResponse = await RefreshTokenAsync(sesion.RefreshToken);
            if (refreshResponse == null)
            {
                _logger.LogWarning("No se pudo renovar el token");
                return false;
            }

            var nuevaSesion = new LoginResponse
            {
                AccessToken = refreshResponse.AccessToken,
                RefreshToken = refreshResponse.RefreshToken,
                UsuarioID = sesion.UsuarioID,
                ExpiresIn = refreshResponse.ExpiresIn
            };

            // verifica si había cookies antes de renovar para mantener la persistencia
            var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
            var teníaCookies = cookies != null &&
                              !string.IsNullOrEmpty(cookies["RefreshToken"]) &&
                              !string.IsNullOrEmpty(cookies["UsuarioID"]);

            GuardarSesion(nuevaSesion, teníaCookies);
            _logger.LogInformation("Sesión renovada exitosamente");
            return true;
        }
    }

    // DTO para deserializar la respuesta de la API
    internal class LoginResponseDto
    {
        [JsonPropertyName("expires_in")]
        public DateTime ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("usuarioID")]
        public string UsuarioID { get; set; } = string.Empty;
    }
}