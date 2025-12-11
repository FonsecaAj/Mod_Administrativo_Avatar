using Avatar_Mod_Administración.Entities;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers; // Se agregó este using

namespace Avatar_Mod_Administración.Services
{
    public class NotificacionesApiClient : INotificacionesApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<NotificacionesApiClient> _logger;

        public NotificacionesApiClient(HttpClient http, IConfiguration config, ILogger<NotificacionesApiClient> logger)
        {
            _http = http;
            _logger = logger;

            _baseUrl = config["ApiUrls:ServiciosApi:Notificaciones"]
              ?? throw new InvalidOperationException("Base URL de Notificaciones no configurada");
        }

        public async Task<BusinessLogicResponseNotificacion?> EnviarCorreoAsync(NotificacionRequestDto request, string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return new BusinessLogicResponseNotificacion
                    {
                        StatusCode = 401,
                        Message = "Token no proporcionado"
                    };

                // INICIO: Lógica copiada del FacturaApiClient para asegurar el formato correcto del token
                var tokenLimpio = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
          ? token.Substring(7).Trim()
          : token.Trim();

                _http.DefaultRequestHeaders.Remove("Authorization"); // Se remueve cualquier encabezado previo
                _http.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", tokenLimpio); // Se establece el encabezado con el token limpio
                // FIN: Lógica copiada del FacturaApiClient

                var response = await _http.PostAsJsonAsync($"{_baseUrl}api/notificar/", request);

                var raw = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(raw))
                {
                    return new BusinessLogicResponseNotificacion
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = response.IsSuccessStatusCode
                        ? "Correo enviado (sin contenido JSON en la respuesta)"
                        : "La API devolvió una respuesta vacía",
                        ResponseObject = null
                    };
                }

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Esto hace que "message" coincida con "Message"
                    };

                    // Pasa las 'options' como segundo parámetro
                    var json = JsonSerializer.Deserialize<BusinessLogicResponseNotificacion>(raw, options);

                    if (json != null)
                        return json;
                }
                catch
                {
                    return new BusinessLogicResponseNotificacion
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = raw,
                        ResponseObject = null
                    };
                }

                return new BusinessLogicResponseNotificacion
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "No se pudo interpretar la respuesta",
                    ResponseObject = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación");
                return new BusinessLogicResponseNotificacion
                {
                    StatusCode = 500,
                    Message = $"Error al consumir Notificaciones: {ex.Message}"
                };
            }
        }
    }
}