using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IInstitucionService
    {
        Task<List<Institucion>> ObtenerTodosAsync(string token, string? nombre = null);
        Task<Institucion?> ObtenerPorIdAsync(int id, string token);
        Task<Institucion?> CrearAsync(InstitucionCrearDto dto, string token);
        Task<Institucion?> ActualizarAsync(int id, InstitucionCrearDto dto, string token);
        Task<(bool exito, string? mensajeError)> EliminarAsync(int id, string token);
    }
}