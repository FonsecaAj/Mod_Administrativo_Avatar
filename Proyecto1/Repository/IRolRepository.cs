using USR2.Entities;

namespace USR2.Repository
{
    public interface IRolRepository
    {
        Task<int> CrearAsync(Rol rol);
        Task ActualizarAsync(Rol rol);
        Task EliminarAsync(int id);
        Task<IEnumerable<Rol>> ObtenerTodosAsync();
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<Rol?> ObtenerPorNombreAsync(string nombre);
    }
}