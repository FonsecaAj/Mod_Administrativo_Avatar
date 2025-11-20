using Avatar_Mod_Administración.Entities;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class ParametroService : IParametroService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<ParametroService> _logger;

        public ParametroService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ParametroService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["USR3ApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR3/";
            _logger = logger;
        }

        public async Task<List<ParametroApi>> ObtenerTodosAsync(string token, int pagina = 1, int porPagina = 30)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_apiUrl}/parametro?pagina={pagina}&porPagina={porPagina}");
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

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var respuesta = JsonSerializer.Deserialize<RespuestaPaginada<ParametroApi>>(content, options);

                return respuesta?.Datos ?? new List<ParametroApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener parámetros");
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<int> ObtenerTotalAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_apiUrl}/parametro?pagina=1&porPagina=1");
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

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var respuesta = JsonSerializer.Deserialize<RespuestaPaginada<ParametroApi>>(content, options);

                return respuesta?.Paginacion?.TotalRegistros ?? 0;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener total de parámetros");
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<ParametroApi?> ObtenerPorIdAsync(string id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/parametro/{id}");
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

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<ParametroApi>(content, options);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener parámetro {Id}", id);
                throw new Exception("Error de conexión con el servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(ParametroCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/parametro");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { idParametro = dto.IdParametro, valor = dto.Valor });

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = JsonDocument.Parse(responseContent);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? string.Empty;

                return (response.IsSuccessStatusCode, statusCode, msg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al crear parámetro");
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(string id, ParametroCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/parametro/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { idParametro = dto.IdParametro, valor = dto.Valor });

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = JsonDocument.Parse(responseContent);
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? string.Empty;

                return (response.IsSuccessStatusCode, statusCode, msg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al actualizar parámetro {Id}", id);
                return (false, 500, "Error de conexión con el servidor");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta");
                return (false, 500, "Error al procesar respuesta del servidor");
            }
        }

        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(string id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/parametro/{id}");
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
                _logger.LogError(ex, "Error de conexión al eliminar parámetro {Id}", id);
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