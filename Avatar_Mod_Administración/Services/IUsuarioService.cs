using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerTodosAsync(string token);
        Task<Usuario?> ObtenerPorEmailAsync(string email, string token);
        Task<bool> CrearAsync(UsuarioCrearDto dto, string token);
        Task<bool> ActualizarAsync(string email, UsuarioCrearDto dto, string token);
        Task<bool> EliminarAsync(string email, string token);
        Task<List<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipo, string token);
        Task<List<TipoIdentificacion>> ObtenerTiposIdentificacionAsync(string token);
        Task<List<Rol>> ObtenerRolesAsync(string token);
    }
}