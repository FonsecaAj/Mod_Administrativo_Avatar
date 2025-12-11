using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IGrupoApiClient
    {
        Task<IEnumerable<GrupoDto>> ObtenerTodosAsync();
        Task<GrupoDto?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(GrupoDto grupo);
        Task<bool> ActualizarAsync(GrupoDto grupo);
        Task<bool> EliminarAsync(int id);
    }
}
