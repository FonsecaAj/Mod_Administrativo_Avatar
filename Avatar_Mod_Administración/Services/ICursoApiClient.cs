using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface ICursoApiClient
    {
        Task<IEnumerable<CursoDto>> ObtenerTodosAsync();
        Task<IEnumerable<CursoDto>> ObtenerPorCarreraAsync(int idCarrera);
        Task<CursoDto?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(CursoDto curso);
        Task<bool> ActualizarAsync(CursoDto curso);
        Task<bool> EliminarAsync(int id);
        Task<LookupsResponse?> ObtenerLookupsAsync();

    }
}
