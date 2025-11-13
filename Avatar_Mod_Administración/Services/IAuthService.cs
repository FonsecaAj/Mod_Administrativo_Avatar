using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string contrasenna);
        Task<RefreshResponse?> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        void GuardarSesion(LoginResponse response, bool recordar);
        LoginResponse? ObtenerSesionActual();
        Task CerrarSesionAsync();
        Task<bool> RenovarSesionAutomaticaAsync();
        Task<bool> RestaurarSesionDesdeCookiesAsync();
    }
}