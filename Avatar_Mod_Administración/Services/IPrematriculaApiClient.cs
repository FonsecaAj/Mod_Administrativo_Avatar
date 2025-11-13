using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IPrematriculaApiClient
    {
        Task<IEnumerable<PrematriculaDto>> ObtenerTodosAsync(PrematriculaFiltro filtro);
        Task<PrematriculaDto?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(PrematriculaDto dto);
        Task<bool> ActualizarAsync(int id, PrematriculaDto dto);
        Task<bool> EliminarAsync(int id);
    }
}
