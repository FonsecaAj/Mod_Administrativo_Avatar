using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IInstitucionService
    {
        Task<List<Institucion>> ObtenerTodosAsync(string token, string? nombre = null);
        Task<Institucion?> ObtenerPorIdAsync(int id, string token);

        // Se devuelven (ok, statusCode, message)
        Task<(bool ok, int statusCode, string? message)> CrearAsync(InstitucionCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> ActualizarAsync(int id, InstitucionCrearDto dto, string token);
        Task<(bool ok, int statusCode, string? message)> EliminarAsync(int id, string token);
    }
}