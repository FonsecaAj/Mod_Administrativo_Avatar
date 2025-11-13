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
            _usr4ApiUrl = configuration.GetValue<string>("USR4ApiUrl") ?? "http://localhost:5290";
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
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener módulos: {StatusCode}", response.StatusCode);
                    return new List<Modulo>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var modulos = JsonSerializer.Deserialize<List<Modulo>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Modulo>();

                _logger.LogInformation("Obtenidos {Count} módulos", modulos.Count);
                return modulos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos");
                return new List<Modulo>();
            }
        }

        public async Task<Modulo?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Modulo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo {Id}", id);
                return null;
            }
        }

        public async Task<Modulo?> CrearAsync(ModuloCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_usr4ApiUrl}/modulo");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al crear módulo: {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Modulo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear módulo");
                return null;
            }
        }

        public async Task<Modulo?> ActualizarAsync(int id, ModuloCrearDto dto, string token)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al actualizar módulo: {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Modulo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar módulo {Id}", id);
                return null;
            }
        }

        public async Task<(bool exito, string? mensajeError)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_usr4ApiUrl}/modulo/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if (errorObj.TryGetProperty("error", out var errorMsg))
                        {
                            return (false, errorMsg.GetString());
                        }
                    }
                    catch { }
                    return (false, "No se puede eliminar el módulo porque está asignado a uno o más roles");
                }

                return (false, $"Error al eliminar: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar módulo {Id}", id);
                return (false, "Error de conexión al intentar eliminar el módulo");
            }
        }
    }
}