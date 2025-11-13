namespace ACD2.Services
{
    public interface IInstitucionService
    {
        Task<bool> ValidarInstitucionAsync(int idInstitucion, string? authToken);
    }

    public class InstitucionService : IInstitucionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _acd1ApiUrl;

        public InstitucionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _acd1ApiUrl = configuration["ACD1ApiUrl"] ?? "http://localhost:5001";
        }

        public async Task<bool> ValidarInstitucionAsync(int idInstitucion, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_acd1ApiUrl}/institucion/{idInstitucion}");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}