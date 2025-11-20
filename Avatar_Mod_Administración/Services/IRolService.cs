using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IRolService
    {
        Task<List<RolApi>> ObtenerTodosAsync(string token);
        Task<RolApi?> ObtenerPorIdAsync(int id, string token);

        Task<(bool ok, int statusCode, string? message)> CrearAsync(RolCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, RolCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token);

        Task<List<RolModuloDetalleDto>> ObtenerModulosPorRolAsync(int idRol, string token);
        Task<bool> AsignarModulosAsync(int idRol, List<int> idsModulos, string token);
        Task<MenuDto?> ObtenerMenuPorRolAsync(int idRol, string token);
    }
}