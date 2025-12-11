using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerTodosAsync(string token);
        Task<Usuario?> ObtenerPorEmailAsync(string email, string token);

        // TODAS las operaciones devuelven (ok, statusCode, message) igual que Factura
        Task<(bool ok, int statusCode, string? message)> CrearAsync(UsuarioCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> ActualizarAsync(string email, UsuarioCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> EliminarAsync(string email, string token);

        Task<List<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipo, string token);
        Task<List<TipoIdentificacion>> ObtenerTiposIdentificacionAsync(string token);
        Task<List<Rol>> ObtenerRolesAsync(string token);
    }
}