namespace Adm_Notificaciones.Service
{
    public class AutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _autenticacionApiUrl;

        public AutenticacionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _autenticacionApiUrl = configuration["AutenticacionApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";
        }

        public async Task<bool> ValidarTokenAsync(string? authorization)
        {
            if (string.IsNullOrWhiteSpace(authorization)) return false;

            var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization.Substring(7)
                : authorization;

            var response = await _httpClient.PostAsJsonAsync($"{_autenticacionApiUrl}/validate", new { Token = token });
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<string?> ObtenerUsuarioDelTokenAsync(string? authorization)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorization)) return null;
                var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorization.Substring(7)
                    : authorization;

                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1];
                var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var bytes = Convert.FromBase64String(padded);
                var json = System.Text.Encoding.UTF8.GetString(bytes);

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("email", out var email))
                    return email.GetString();

                return "sistema";
            }
            catch { return null; }
        }
    }
}
