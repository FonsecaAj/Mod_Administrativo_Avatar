using System.Text;

namespace ADM_Pagos.Services
{
    public interface IAutenticacionService
    {
        Task<bool> ValidarTokenAsync(string? authorization);
        Task<string?> ObtenerUsuarioDelTokenAsync(string? authorization);
    }

    public class AutenticacionService : IAutenticacionService
    {

        private readonly HttpClient _http;
        private readonly string _authUrl;

        public AutenticacionService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _authUrl = cfg["AutenticacionApiUrl"] ?? "http://localhost:5233";
        }

        public async Task<bool> ValidarTokenAsync(string? authorization)
        {
            try
            {
                var token = ExtraerBearer(authorization);
                if (string.IsNullOrWhiteSpace(token)) return false;

                var resp = await _http.PostAsJsonAsync($"{_authUrl}/validate", new { Token = token });
                if (!resp.IsSuccessStatusCode) return false;

                var ok = await resp.Content.ReadFromJsonAsync<bool>();
                return ok;
            }
            catch { return false; }
        }

        public async Task<string?> ObtenerUsuarioDelTokenAsync(string? authorization)
        {
            try
            {
                var token = ExtraerBearer(authorization);
                if (string.IsNullOrWhiteSpace(token)) return null;

                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1];
                var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var payloadBytes = Convert.FromBase64String(padded);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);

                using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("sub", out var sub)) return sub.GetString();
                if (doc.RootElement.TryGetProperty("email", out var email)) return email.GetString();
                if (doc.RootElement.TryGetProperty("unique_name", out var un)) return un.GetString();
                return null;
            }
            catch { return null; }
        }

        private static string? ExtraerBearer(string? authorization)
        {
            if (string.IsNullOrWhiteSpace(authorization)) return null;
            return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization[7..]
                : authorization;
        }


    }
}
