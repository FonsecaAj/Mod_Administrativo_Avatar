using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IPerfilUsuarioApiClient
    {
        Task<(bool ok, int statusCode, string? message, PerfilUsuarioDto? data)>
            ObtenerPerfilAsync(string email, string token, CancellationToken ct = default);
    }

}
