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
                    return refreshResponse;
                }

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
                    ? token.Substring(7)
                    : token;

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
            if (session == null) return;

            var tokenConBearer = response.AccessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? response.AccessToken
                : $"Bearer {response.AccessToken}";

            // Operaciones de sesión agrupadas
            session.SetString("AccessToken", tokenConBearer);
            session.SetString("RefreshToken", response.RefreshToken);
            session.SetString("UsuarioID", response.UsuarioID);
            session.SetString("ExpiresIn", response.ExpiresIn.ToString("o"));

            // Cookies solo si recordar = true
            if (recordar)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax
                };

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.Cookies.Append("RefreshToken", response.RefreshToken, cookieOptions);
                    httpContext.Response.Cookies.Append("UsuarioID", response.UsuarioID, cookieOptions);
                }
            }
            else
            {
                // Limpiar cookies si existen
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.Cookies.Delete("RefreshToken");
                    httpContext.Response.Cookies.Delete("UsuarioID");
                }
            }
        }

        public LoginResponse? ObtenerSesionActual()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var accessToken = session.GetString("AccessToken");
            var refreshToken = session.GetString("RefreshToken");

            // Retorno rápido si no hay tokens
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return null;

            var usuarioID = session.GetString("UsuarioID");
            var expiresInStr = session.GetString("ExpiresIn");

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UsuarioID = usuarioID ?? "",
                ExpiresIn = DateTime.TryParse(expiresInStr, out var expiry) ? expiry : DateTime.UtcNow
            };
        }

        public async Task<bool> RestaurarSesionDesdeCookiesAsync()
        {
            try
            {
                var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
                if (cookies == null) return false;

                var refreshToken = cookies["RefreshToken"];
                var usuarioID = cookies["UsuarioID"];

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(usuarioID))
                    return false;

                var refreshResponse = await RefreshTokenAsync(refreshToken);
                if (refreshResponse == null)
                {
                    // Limpiar cookies inválidas
                    _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                    _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");
                    return false;
                }

                var nuevaSesion = new LoginResponse
                {
                    AccessToken = refreshResponse.AccessToken,
                    RefreshToken = refreshResponse.RefreshToken,
                    UsuarioID = usuarioID,
                    ExpiresIn = refreshResponse.ExpiresIn
                };

                GuardarSesion(nuevaSesion, true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar sesión desde cookies");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");
                return false;
            }
        }

        public async Task CerrarSesionAsync()
        {
            try
            {
                var sesion = ObtenerSesionActual();

                // Limpiar sesión local PRIMERO
                _httpContextAccessor.HttpContext?.Session.Clear();
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("UsuarioID");

                // Llamar al servidor en segundo plano (fire-and-forget)
                if (sesion != null && !string.IsNullOrEmpty(sesion.AccessToken))
                {
                    // No esperar - ejecutar en background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/logout");
                            request.Headers.Add("Authorization", sesion.AccessToken);
                            await _httpClient.SendAsync(request);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al llamar logout en servidor");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CerrarSesionAsync");
            }
        }

        public async Task<bool> RenovarSesionAutomaticaAsync()
        {
            var sesion = ObtenerSesionActual();
            if (sesion == null) return false;

            var tokenValido = await ValidateTokenAsync(sesion.AccessToken);
            if (tokenValido) return true;

            var refreshResponse = await RefreshTokenAsync(sesion.RefreshToken);
            if (refreshResponse == null) return false;

            var nuevaSesion = new LoginResponse
            {
                AccessToken = refreshResponse.AccessToken,
                RefreshToken = refreshResponse.RefreshToken,
                UsuarioID = sesion.UsuarioID,
                ExpiresIn = refreshResponse.ExpiresIn
            };

            var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
            var teníaCookies = cookies != null &&
                              !string.IsNullOrEmpty(cookies["RefreshToken"]) &&
                              !string.IsNullOrEmpty(cookies["UsuarioID"]);

            GuardarSesion(nuevaSesion, teníaCookies);
            return true;
        }
    }

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