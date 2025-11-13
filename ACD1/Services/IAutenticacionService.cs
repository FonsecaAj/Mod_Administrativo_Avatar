namespace ACD1.Services
{
    public interface IAutenticacionService
    {
        Task<bool> ValidarTokenAsync(string? authorization);
        Task<string?> ObtenerUsuarioDelTokenAsync(string? authorization);
    }

    public class AutenticacionService : IAutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _autenticacionApiUrl;

        public AutenticacionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _autenticacionApiUrl = configuration["AutenticacionApiUrl"] ?? "http://localhost:5233";
        }

        public async Task<bool> ValidarTokenAsync(string? authorization)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorization))
                    return false;

                var token = authorization.Trim();
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = token.Substring(7).Trim();

                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var validateRequest = new { Token = token };
                var response = await _httpClient.PostAsJsonAsync($"{_autenticacionApiUrl}/validate", validateRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        return await response.Content.ReadFromJsonAsync<bool>();
                    }
                    catch
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return content.Trim().ToLower() == "true";
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> ObtenerUsuarioDelTokenAsync(string? authorization)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorization))
                    return null;

                var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorization.Substring(7)
                    : authorization;

                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var parts = token.Split('.');
                if (parts.Length != 3)
                    return null;

                var payload = parts[1];
                var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var payloadBytes = Convert.FromBase64String(paddedPayload);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

                using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);

                if (doc.RootElement.TryGetProperty("sub", out var sub))
                    return sub.GetString();

                if (doc.RootElement.TryGetProperty("email", out var email))
                    return email.GetString();

                if (doc.RootElement.TryGetProperty("unique_name", out var uniqueName))
                    return uniqueName.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}