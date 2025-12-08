namespace ApiMAT2.Services
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public BitacoraConsumer(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["BitacoraService:BaseUrl"] ?? "http://localhost:5293";
        }

        public async Task RegistrarAccionAsync(string usuario, string tipoAccion, object descripcion)
        {
            var body = new
            {
                Usuario = usuario,
                Tipo_Accion = tipoAccion,
                Descripcion = descripcion,
                Fecha = DateTime.UtcNow
            };

            try
            {
                var response = await _http.PostAsJsonAsync($"{_baseUrl}/api/bitacora", body);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Bitácora no registrada. StatusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar con bitácora: {ex.Message}");
            }
        }
    }
}
