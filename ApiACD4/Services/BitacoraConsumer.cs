using System.Net.Http.Json;

namespace ApiACD4.Services
{
    public class BitacoraConsumer
    {
        private readonly HttpClient _http;

        public BitacoraConsumer(HttpClient http)
        {
            _http = http;
        }

        public async Task RegistrarAccionAsync(string usuario, string tipoAccion, object descripcion)
        {
            try
            {
                
                var body = new
                {
                    Usuario = usuario,
                    Tipo_Accion = tipoAccion,    
                    Descripcion = descripcion,    
                    Fecha = DateTime.UtcNow
                };

          
                var response = await _http.PostAsJsonAsync("/api/bitacora", body);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Bitácora registrada correctamente ({tipoAccion}) por {usuario}");
                }
                else
                {
                    Console.WriteLine($" No se pudo registrar la bitácora. StatusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
             
                Console.WriteLine($" Error al conectar con bitácora: {ex.Message}");
            }
        }
    }
}
