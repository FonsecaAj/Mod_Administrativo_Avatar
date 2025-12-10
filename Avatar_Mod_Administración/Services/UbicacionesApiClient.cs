using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public class UbicacionesApiClient : IUbicacionesApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        // Solo para debug (para ver qué hay en appsettings)
        private readonly string _baseUrlFromConfig;

        public UbicacionesApiClient(HttpClient http, IConfiguration config, IAuthService authService)
        {
            _http = http;
            _config = config;
            _authService = authService;

            _baseUrlFromConfig = _config["Adm_Direcciones:BaseUrl"] ?? "(NO CONFIG)";

            // OJO: el BaseAddress ya lo estás seteando en Program.cs con AddHttpClient
            // así que aquí NO volvemos a tocarlo.
        }

        // ================= TOKEN =================

        private string ObtenerTokenLimpio()
        {
            var sesion = _authService.ObtenerSesionActual();

            if (sesion == null || string.IsNullOrWhiteSpace(sesion.AccessToken))
                return string.Empty;

            var token = sesion.AccessToken;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7).Trim();

            return token;
        }

        private void AplicarToken()
        {
            var token = ObtenerTokenLimpio();

            _http.DefaultRequestHeaders.Remove("Authorization");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ================= PROVINCIAS =================

        public async Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync()
        {
            AplicarToken();

            var relativeUrl = "api/provincias";

            try
            {
                var response = await _http.GetAsync(relativeUrl);
                var raw = await response.Content.ReadAsStringAsync();

                Console.WriteLine("=== DEBUG PROVINCIAS ===");
                Console.WriteLine($"BaseAddress        : {_http.BaseAddress}");
                Console.WriteLine($"Config BaseUrl     : {_baseUrlFromConfig}");
                Console.WriteLine($"Relative URL       : {relativeUrl}");
                Console.WriteLine($"Status             : {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"Raw Content        : {raw}");
                Console.WriteLine("========================");

                if (!response.IsSuccessStatusCode)
                    return Enumerable.Empty<ProvinciaDto>();

                var wrapper = JsonSerializer.Deserialize<BusinessLogicResponse<IEnumerable<ProvinciaDto>>>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.ResponseObject ?? Enumerable.Empty<ProvinciaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR PROVINCIAS ===");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================");
                return Enumerable.Empty<ProvinciaDto>();
            }
        }

        // ================= CANTONES =================

        public async Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia)
        {
            AplicarToken();

            var relativeUrl = $"api/cantones?provincia={idProvincia}";

            try
            {
                var response = await _http.GetAsync(relativeUrl);
                var raw = await response.Content.ReadAsStringAsync();

                Console.WriteLine("=== DEBUG CANTONES ===");
                Console.WriteLine($"BaseAddress        : {_http.BaseAddress}");
                Console.WriteLine($"Config BaseUrl     : {_baseUrlFromConfig}");
                Console.WriteLine($"Relative URL       : {relativeUrl}");
                Console.WriteLine($"Status             : {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"Raw Content        : {raw}");
                Console.WriteLine("=======================");

                if (!response.IsSuccessStatusCode)
                    return Enumerable.Empty<CantonDto>();

                var wrapper = JsonSerializer.Deserialize<BusinessLogicResponse<IEnumerable<CantonDto>>>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.ResponseObject ?? Enumerable.Empty<CantonDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR CANTONES ===");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=======================");
                return Enumerable.Empty<CantonDto>();
            }
        }

        // ================= DISTRITOS =================

        public async Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton)
        {
            AplicarToken();

            var relativeUrl = $"api/distritos?provincia={idProvincia}&canton={idCanton}";

            try
            {
                var response = await _http.GetAsync(relativeUrl);
                var raw = await response.Content.ReadAsStringAsync();

                Console.WriteLine("=== DEBUG DISTRITOS ===");
                Console.WriteLine($"BaseAddress        : {_http.BaseAddress}");
                Console.WriteLine($"Config BaseUrl     : {_baseUrlFromConfig}");
                Console.WriteLine($"Relative URL       : {relativeUrl}");
                Console.WriteLine($"Status             : {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"Raw Content        : {raw}");
                Console.WriteLine("========================");

                if (!response.IsSuccessStatusCode)
                    return Enumerable.Empty<DistritoDto>();

                var wrapper = JsonSerializer.Deserialize<BusinessLogicResponse<IEnumerable<DistritoDto>>>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper?.ResponseObject ?? Enumerable.Empty<DistritoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR DISTRITOS ===");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================");
                return Enumerable.Empty<DistritoDto>();
            }
        }
    }
}
