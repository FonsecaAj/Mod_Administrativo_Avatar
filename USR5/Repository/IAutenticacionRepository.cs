using USR5.Entities;

namespace USR5.Repository
{
    public interface IAutenticacionRepository
    {
        Task<string> CrearRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> ObtenerRefreshTokenAsync(string token);
        Task DesactivarRefreshTokenAsync(string token);
        Task<string> CrearJwtTokenAsync(JwtToken token);
        Task<JwtToken?> ObtenerJwtTokenAsync(string token);
        Task DesactivarJwtTokenAsync(string token);
        Task DesactivarJwtTokensUsuarioAsync(string email);
        Task DesactivarTokensUsuarioAsync(string email);
    }
}