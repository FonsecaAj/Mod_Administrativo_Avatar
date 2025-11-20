using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IModuloService
    {
        Task<List<Modulo>> ObtenerTodosAsync(string token, string? nombre = null);
        Task<Modulo?> ObtenerPorIdAsync(int id, string token);
        Task<Modulo?> CrearAsync(ModuloCrearDto dto, string token);
        Task<Modulo?> ActualizarAsync(int id, ModuloCrearDto dto, string token);
        Task<(bool exito, string? mensajeError)> EliminarAsync(int id, string token);
    }
}