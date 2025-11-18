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
            _apiUrl = configuration["USR3ApiUrl"] ?? "http://localhost:5279";
            _logger = logger;
        }

        public async Task<List<ParametroApi>> ObtenerTodosAsync(string token, int pagina = 1, int porPagina = 30)
        {
            try
            {
                _logger.LogInformation("Obteniendo parámetros - Página: {Pagina}, PorPágina: {PorPagina}", pagina, porPagina);

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_apiUrl}/parametro?pagina={pagina}&porPagina={porPagina}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener parámetros: {Status}, {Content}", response.StatusCode, errorContent);
                    return new List<ParametroApi>();
                }

                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Respuesta recibida: {Content}", content.Substring(0, Math.Min(200, content.Length)));

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var respuesta = JsonSerializer.Deserialize<RespuestaPaginada<ParametroApi>>(content, options);

                if (respuesta?.Datos == null)
                {
                    _logger.LogWarning("La respuesta no contiene datos o es nula");
                    return new List<ParametroApi>();
                }

                _logger.LogInformation("Parámetros obtenidos: {Count} de {Total}",
                    respuesta.Datos.Count, respuesta.Paginacion?.TotalRegistros ?? 0);

                return respuesta.Datos;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error al deserializar la respuesta JSON");
                return new List<ParametroApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener parámetros");
                return new List<ParametroApi>();
            }
        }

        public async Task<int> ObtenerTotalAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obteniendo total de parámetros");

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_apiUrl}/parametro?pagina=1&porPagina=1");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener total de parámetros: {Status}", response.StatusCode);
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var respuesta = JsonSerializer.Deserialize<RespuestaPaginada<ParametroApi>>(content, options);

                var total = respuesta?.Paginacion?.TotalRegistros ?? 0;
                _logger.LogInformation("Total de parámetros: {Total}", total);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de parámetros");
                return 0;
            }
        }

        public async Task<ParametroApi?> ObtenerPorIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obteniendo parámetro {Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/parametro/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener parámetro: {Status}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<ParametroApi>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener parámetro");
                return null;
            }
        }

        // Se devuelve (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> CrearAsync(ParametroCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Creando parámetro {IdParametro}", dto.IdParametro);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/parametro");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { idParametro = dto.IdParametro, valor = dto.Valor });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear parámetro: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al crear parámetro";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al crear parámetro");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Parámetro creado exitosamente";

                _logger.LogInformation("Parámetro creado. Estado {Status}: {Mensaje}", statusCode, msg);

                return (statusCode == 201, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear parámetro");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Se devuelve (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> ActualizarAsync(string id, ParametroCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Actualizando parámetro {Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/parametro/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { idParametro = dto.IdParametro, valor = dto.Valor });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar parámetro: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al actualizar parámetro";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al actualizar parámetro");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Parámetro actualizado exitosamente";

                _logger.LogInformation("Parámetro {Id} actualizado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parámetro");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }

        // Se devuelve (ok, status, message)
        public async Task<(bool ok, int statusCode, string? message)> EliminarAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Eliminando parámetro {Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/parametro/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al eliminar parámetro: {Status}, {Content}", response.StatusCode, errorContent);

                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        int status = doc.RootElement.GetProperty("statusCode").GetInt32();
                        string message = doc.RootElement.GetProperty("message").GetString() ?? "Error al eliminar parámetro";
                        return (false, status, message);
                    }
                    catch
                    {
                        return (false, (int)response.StatusCode, "Error al eliminar parámetro");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var payload = await JsonDocument.ParseAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseContent)));
                int statusCode = payload.RootElement.GetProperty("statusCode").GetInt32();
                string msg = payload.RootElement.GetProperty("message").GetString() ?? "Parámetro eliminado exitosamente";

                _logger.LogInformation("Parámetro {Id} eliminado. Estado {Status}: {Mensaje}", id, statusCode, msg);

                return (statusCode == 200, statusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar parámetro");
                return (false, 500, $"Error procesando respuesta: {ex.Message}");
            }
        }
    }
}