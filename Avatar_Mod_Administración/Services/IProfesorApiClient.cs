using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IProfesorApiClient
    {
        Task<IEnumerable<ProfesorDto>> ObtenerTodosAsync();
        Task<ProfesorDto?> ObtenerPorIdAsync(int idProfesor);
        Task<bool> CrearAsync(ProfesorDto profesor);
        Task<bool> ActualizarAsync(ProfesorDto profesor);
        Task<bool> EliminarAsync(int idProfesor);
    }
}
