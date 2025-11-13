using USR1.Entities;

namespace USR1.Repository
{
    public interface IUsuarioRepository
    {
        Task<string> CrearAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task EliminarAsync(string email);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipoIdentificacion);
    }
}