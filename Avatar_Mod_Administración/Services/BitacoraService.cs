using Avatar_Mod_Administración.Entities;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BitacoraService> logger)
        {
            _httpClient = httpClient;
            // URL del servicio GEN1
            _apiUrl = configuration["GEN1ApiUrl"] ?? "http://localhost:5000";
            _logger = logger;
        }

        public async Task<bool> RegistrarAsync(BitacoraCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Registrando bitácora para usuario: {Usuario}", dto.Usuario);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/bitacora");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new
                {
                    usuario = dto.Usuario,
                    descripcion = dto.Descripcion
                });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al registrar bitácora: {Status}, {Content}",
                        response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("Bitácora registrada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar bitácora");
                return false;
            }
        }

        public async Task<List<Bitacora>> ObtenerTodosAsync(string token, BitacoraFiltroDto? filtro = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo bitácoras");

                var queryParams = new List<string>();
                if (filtro != null)
                {
                    if (filtro.FechaInicio.HasValue)
                        queryParams.Add($"fechaInicio={filtro.FechaInicio.Value:yyyy-MM-dd}");
                    if (filtro.FechaFin.HasValue)
                        queryParams.Add($"fechaFin={filtro.FechaFin.Value:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(filtro.Usuario))
                        queryParams.Add($"usuario={Uri.EscapeDataString(filtro.Usuario)}");
                    if (!string.IsNullOrWhiteSpace(filtro.Accion))
                        queryParams.Add($"accion={Uri.EscapeDataString(filtro.Accion)}");
                    if (!string.IsNullOrWhiteSpace(filtro.Modulo))
                        queryParams.Add($"modulo={Uri.EscapeDataString(filtro.Modulo)}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_apiUrl}/bitacora{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener bitácoras: {Status}, {Content}",
                        response.StatusCode, errorContent);
                    return new List<Bitacora>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var bitacoras = JsonSerializer.Deserialize<List<Bitacora>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Bitacora>();

                _logger.LogInformation("Bitácoras obtenidas: {Count}", bitacoras.Count);
                return bitacoras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bitácoras");
                return new List<Bitacora>();
            }
        }

        public async Task<Bitacora?> ObtenerPorIdAsync(int id, string token)
        {
            try
            {
                _logger.LogInformation("Obteniendo bitácora {Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/bitacora/{id}");
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al obtener bitácora: {Status}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Bitacora>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bitácora");
                return null;
            }
        }
    }
}