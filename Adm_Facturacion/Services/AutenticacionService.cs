namespace Adm_Facturacion.Services
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
            _autenticacionApiUrl = configuration["AutenticacionApiUrl"] ?? "http://localhost:5234"; // URL del microservicio USR5
        }

        public async Task<bool> ValidarTokenAsync(string? authorization)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorization))
                    return false;

                var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorization.Substring(7)
                    : authorization;

                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_autenticacionApiUrl}/validate",
                    new { Token = token });

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>();
                    return result;
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
