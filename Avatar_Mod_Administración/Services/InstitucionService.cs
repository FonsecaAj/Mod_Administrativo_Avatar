using Avatar_Mod_Administración.Entities;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class InstitucionService : IInstitucionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<InstitucionService> _logger;

        public InstitucionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InstitucionService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ACD1ApiUrl"] ?? "http://localhost:5001";
            _logger = logger;
        }

        public async Task<List<Institucion>> ObtenerTodosAsync(string token, string? nombre = null)
        {
            try
            {
                var url = $"{_apiUrl}/institucion";

                // Construir query string solo si hay búsqueda
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrimmed = nombre.Trim();
                    url += $"?nombre={Uri.EscapeDataString(nombreTrimmed)}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener instituciones: {Status}, {Content}", response.StatusCode, errorContent);
                    return new List<Institucion>();
                }

                var content = await response.Content.ReadAsStringAsync();

                var instituciones = JsonSerializer.Deserialize<List<Institucion>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Institucion>();

                return instituciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener instituciones");
                return new List<Institucion>();
            }
        }

        public async Task<Institucion?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener institución: {Status}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Institucion>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener institución");
                return null;
            }
        }

        public async Task<Institucion?> CrearAsync(InstitucionCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/institucion");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { nombre = dto.Nombre });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear institución: {Status}, {Content}", response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Institucion>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear institución");
                return null;
            }
        }

        public async Task<Institucion?> ActualizarAsync(int id, InstitucionCrearDto dto, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new { nombre = dto.Nombre });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar institución: {Status}, {Content}", response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Institucion>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar institución");
                return null;
            }
        }

        public async Task<(bool exito, string? mensajeError)> EliminarAsync(int id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiUrl}/institucion/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                // Leer el contenido del error
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al eliminar institución: {Status}, {Content}", response.StatusCode, errorContent);

                // Extraer el mensaje de error del API
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (false, errorObj?.Error ?? "Error al eliminar la institución");
                }
                catch
                {
                    // Si no se puede parsear como JSON, intentar usar el contenido directo
                    if (!string.IsNullOrWhiteSpace(errorContent) && errorContent.Length < 200)
                    {
                        return (false, errorContent);
                    }
                    return (false, "Error al eliminar la institución");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar institución");
                return (false, "Error de conexión al eliminar la institución");
            }
        }

        private class ErrorResponse
        {
            public string Error { get; set; } = string.Empty;
        }
    }
}