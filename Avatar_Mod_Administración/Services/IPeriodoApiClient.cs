using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IPeriodoApiClient
    {
        Task<IEnumerable<PeriodoDto>> ObtenerTodosAsync();
        Task<PeriodoDto?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(PeriodoDto periodo);
        Task<bool> ActualizarAsync(PeriodoDto periodo);
        Task<bool> EliminarAsync(int id);
    }
}
