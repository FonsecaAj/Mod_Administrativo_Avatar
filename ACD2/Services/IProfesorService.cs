namespace ACD2.Services
{
    public interface IProfesorService
    {
        Task<bool> ValidarProfesorAsync(int idProfesor, string? authToken);
    }

    public class ProfesorService : IProfesorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _acd6ApiUrl;

        public ProfesorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _acd6ApiUrl = configuration["ACD6ApiUrl"] ?? "http://localhost:5057";
        }

        public async Task<bool> ValidarProfesorAsync(int idProfesor, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_acd6ApiUrl}/profesor/{idProfesor}");

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