using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IModuloService
    {
        Task<List<Modulo>> ObtenerTodosAsync(string token, string? nombre = null);
        Task<Modulo?> ObtenerPorIdAsync(int id, string token);

        // Todas las operaciones CUD devuelven (ok, statusCode, message)
        Task<(bool ok, int statusCode, string? message)> CrearAsync(ModuloCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, ModuloCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token);
    }
}