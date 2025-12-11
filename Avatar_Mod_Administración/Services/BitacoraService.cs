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
            PropertyNameCaseInsensitive = true
        };

        public BitacoraService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BitacoraService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["GEN1ApiUrl"]
                      ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral";
            _logger = logger;
        }

        public async Task<(IEnumerable<Bitacora> Data, int Total)> ConsultarAsync(BitacoraFiltroDto filtro)
        {
            try
            {
                _logger.LogInformation("Consultando Bitácora…");

                var query = BuildQueryString(new Dictionary<string, string?>
                {
                    ["nombreModulo"] = filtro.Modulo,
                    ["usuario"] = filtro.Usuario,
                    ["tipoAccion"] = filtro.Accion,
                    ["fechaDesde"] = filtro.FechaInicio?.ToString("yyyy-MM-dd"),
                    ["fechaHasta"] = filtro.FechaFin?.ToString("yyyy-MM-dd"),
                    ["pagina"] = "1",
                    ["porPagina"] = "100"
                });

                var url = $"{_apiUrl}/api/bitacora/consulta{query}";
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
