using System.Text.Json;
using USR2.Entities;

namespace USR2.Services
{
    public class ModuloApiService : IModuloApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usr4ApiUrl;
        private readonly ILogger<ModuloApiService> _logger;

        public ModuloApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ModuloApiService> logger)
        {
            _httpClient = httpClient;
            _usr4ApiUrl = configuration["USR4ApiUrl"] ?? "http://localhost:5290";
            _logger = logger;

            _logger.LogInformation("ModuloApiService inicializado con URL: {Url}", _usr4ApiUrl);
        }

        public async Task<ModuloDto?> ObtenerPorIdAsync(int idModulo, string? token = null)
        {
            try
            {
                var url = $"{_usr4ApiUrl}/modulo/{idModulo}";
                _logger.LogInformation("USR2→USR4: Solicitando módulo {IdModulo} a: {Url}", idModulo, url);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(token))
                {
                    var authToken = token;
                    if (!token.StartsWith("Bearer "))
                    {
                        authToken = $"Bearer {token}";
                    }

                    request.Headers.Add("Authorization", authToken);
                    _logger.LogDebug("USR2→USR4: Token enviado: {Token}",
                        authToken.Substring(0, Math.Min(40, authToken.Length)) + "...");
                }
                else
                {
                    _logger.LogWarning("USR2→USR4: No se proporciono token de autorización");
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("USR2←USR4: Error al obtener módulo {IdModulo}: {StatusCode} - {Error}",
                        idModulo, response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("USR2←USR4: Contenido: {Content}",
                    content.Substring(0, Math.Min(100, content.Length)));

                // BusinessLogicResponse y extraer responseObject
                using var doc = JsonDocument.Parse(content);

                // Verificar si tiene la estructura de BusinessLogicResponse
                if (doc.RootElement.TryGetProperty("responseObject", out var responseObj))
                {
                    // Es BusinessLogicResponse, deserializar el responseObject
                    var modulo = JsonSerializer.Deserialize<ModuloDto>(responseObj.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (modulo != null)
                    {
                        _logger.LogDebug("USR2←USR4: Módulo {IdModulo} obtenido de BusinessLogicResponse: {Nombre} (Activo: {Activo})",
                            idModulo, modulo.Nombre, modulo.Activo);
                    }
                    else
                    {
                        _logger.LogWarning("USR2←USR4: No se pudo deserializar módulo {IdModulo} desde responseObject", idModulo);
                    }

                    return modulo;
                }
                else
                {
                    // intentar deserializar directamente
                    var modulo = JsonSerializer.Deserialize<ModuloDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (modulo != null)
                    {
                        _logger.LogDebug("USR2←USR4: Módulo {IdModulo} obtenido (formato directo): {Nombre} (Activo: {Activo})",
                            idModulo, modulo.Nombre, modulo.Activo);
                    }
                    else
                    {
                        _logger.LogWarning("USR2←USR4: No se pudo deserializar módulo {IdModulo}", idModulo);
                    }

                    return modulo;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "USR2←USR4: Error de conexión al obtener módulo {IdModulo}", idModulo);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "USR2←USR4: Excepción al obtener módulo {IdModulo}", idModulo);
                return null;
            }
        }
    }
}