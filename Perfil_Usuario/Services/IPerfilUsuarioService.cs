using Perfil_Usuario.Entities;

namespace Perfil_Usuario.Services
{
    public interface IPerfilUsuarioService
    {
        Task<BusinessLogicResponse> ObtenerPerfilAsync(string email);
    }
}