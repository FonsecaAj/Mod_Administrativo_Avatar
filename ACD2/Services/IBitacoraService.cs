namespace ACD2.Services
{
    public interface IBitacoraService
    {
        Task RegistrarAsync(string usuario, string descripcion);
    }

    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _gen1ApiUrl;

        public BitacoraService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _gen1ApiUrl = configuration["GEN1ApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/";
        }

        public async Task RegistrarAsync(string usuario, string descripcion)
        {
            try
            {
                var request = new { usuario, descripcion };
                await _httpClient.PostAsJsonAsync($"{_gen1ApiUrl}/bitacora", request);
            }
            catch { }
        }
    }
}