using ACD2.Entities;

namespace ACD2.Repository
{
    public interface ICarreraRepository
    {
        Task<int> CrearAsync(Carrera carrera);
        Task ActualizarAsync(Carrera carrera);
        Task EliminarAsync(int idCarrera);
        Task<IEnumerable<Carrera>> ObtenerTodosAsync();
        Task<Carrera?> ObtenerPorIdAsync(int idCarrera);
        Task<IEnumerable<Carrera>> ObtenerPorInstitucionAsync(int idInstitucion);
    }
}