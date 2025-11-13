using USR5.Entities;

namespace USR5.Repository
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ValidarCredencialesAsync(string email, string contrasenna);
    }
}