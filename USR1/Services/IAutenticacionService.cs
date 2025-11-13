namespace USR1.Services
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
                Console.WriteLine("=== INICIO VALIDACIÓN TOKEN EN USR1 ===");
                Console.WriteLine($"Authorization header recibido: [{authorization}]");

                // Validar que venga el header de autorización
                if (string.IsNullOrWhiteSpace(authorization))
                {
                    Console.WriteLine("ERROR: Authorization header vacío o null");
                    return false;
                }

                // Extraer el token (remover "Bearer " si existe)
                var token = authorization.Trim();

                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring(7).Trim();
                    Console.WriteLine("Token extraído después de remover 'Bearer '");
                }
                else
                {
                    Console.WriteLine("Token sin prefijo 'Bearer '");
                }

                Console.WriteLine($"Token limpio: [{token}]");
                Console.WriteLine($"Longitud del token: {token.Length}");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("ERROR: Token vacío después de extraer");
                    return false;
                }

                // Llamar al endpoint /validate de USR5
                var validateRequest = new { Token = token };
                var url = $"{_autenticacionApiUrl}/validate";
                Console.WriteLine($"Enviando validación a: {url}");

                var response = await _httpClient.PostAsJsonAsync(url, validateRequest);

                Console.WriteLine($"Respuesta HTTP: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Contenido de respuesta: {responseContent}");

                // Si la respuesta es 200, el token es válido
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        var result = await response.Content.ReadFromJsonAsync<bool>();
                        Console.WriteLine($"✅ Resultado validación: {result}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR al parsear respuesta: {ex.Message}");
                        // Si el contenido es "true" como string
                        if (responseContent.Trim().ToLower() == "true")
                        {
                            Console.WriteLine("Token válido (parseado como string)");
                            return true;
                        }
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"ERROR: Respuesta no OK - {response.StatusCode}");
                }

                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR DE RED en ValidarTokenAsync: {ex.Message}");
                Console.WriteLine($"¿USR5 está corriendo en {_autenticacionApiUrl}?");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓN en ValidarTokenAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

    public record ValidateRequestDto(string Token);
}