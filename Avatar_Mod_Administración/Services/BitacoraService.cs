using Avatar_Mod_Administración.Entities;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Avatar_Mod_Administración.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<BitacoraService> _logger;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public BitacoraService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BitacoraService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ApiUrls:ServiciosApi:Bitacora"]
                      ?? configuration["ApiUrls:GEN1"]
                      ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/";
            _logger = logger;
        }

        public async Task<bool> RegistrarAsync(BitacoraCrearDto dto, string token)
        {
            try
            {
                _logger.LogInformation("Registrando bitácora para usuario: {Usuario}", dto.Usuario);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}api/bitacora");
                request.Headers.Add("Authorization", token);
                request.Content = JsonContent.Create(new
                {
                    usuario = dto.Usuario,
                    descripcion = dto.Descripcion,
                    tipo_Accion = dto.Tipo_Accion
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

        public async Task<(IEnumerable<Bitacora> Data, int Total)> ConsultarAsync(BitacoraFiltroDto filtro)
        {
            try
            {
                _logger.LogInformation("Consultando Bitácora...");

                var query = BuildQueryString(new Dictionary<string, string?>
                {
                    ["nombreModulo"] = filtro.Modulo,
                    ["usuario"] = filtro.Usuario,
                    ["tipoAccion"] = filtro.Accion,
                    ["fechaDesde"] = filtro.FechaInicio?.ToString("yyyy-MM-dd"),
                    ["fechaHasta"] = filtro.FechaFin?.ToString("yyyy-MM-dd"),
                    ["pagina"] = filtro.Pagina.ToString(),
                    ["porPagina"] = filtro.PorPagina.ToString()
                });

                var url = $"{_apiUrl}api/bitacora/consulta{query}";
                _logger.LogInformation("URL generada: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error HTTP {Status}: {Content}",
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync());

                    return (Enumerable.Empty<Bitacora>(), 0);
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(stream);

                if (!jsonDoc.RootElement.TryGetProperty("responseObject", out var responseObj))
                    return (Enumerable.Empty<Bitacora>(), 0);

                int total = responseObj.GetProperty("totalRegistros").GetInt32();

                var dataElem = responseObj.GetProperty("data");

                var lista = JsonSerializer.Deserialize<IEnumerable<Bitacora>>(
                    dataElem.GetRawText(),
                    _jsonOptions
                ) ?? Enumerable.Empty<Bitacora>();

                return (lista, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar bitácora");
                return (Enumerable.Empty<Bitacora>(), 0);
            }
        }

        public async Task<List<Bitacora>> ObtenerTodosAsync(string token, BitacoraFiltroDto? filtro = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo bitácoras");

                var queryParams = new List<string>();

                // Solo aplicar filtros si se proporcionan
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
                var url = $"{_apiUrl}api/bitacora{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", token);

                // Timeout de 5 segundos para evitar esperas largas
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener bitácoras: {StatusCode}", response.StatusCode);
                    return new List<Bitacora>();
                }

                var content = await response.Content.ReadAsStringAsync();

                var bitacoras = JsonSerializer.Deserialize<List<Bitacora>>(content, _jsonOptions)
                    ?? new List<Bitacora>();

                _logger.LogInformation("Bitácoras obtenidas: {Count}", bitacoras.Count);

                return bitacoras;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Timeout al obtener bitácoras");
                return new List<Bitacora>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bitácoras");
                return new List<Bitacora>();
            }
        }

        private static string BuildQueryString(Dictionary<string, string?> parameters)
        {
            var sb = new StringBuilder();
            bool first = true;

            foreach (var p in parameters)
            {
                if (string.IsNullOrWhiteSpace(p.Value)) continue;

                sb.Append(first ? "?" : "&");
                sb.Append($"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}");

                first = false;
            }

            return sb.ToString();
        }
    }
}