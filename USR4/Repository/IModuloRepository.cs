using USR4.Entities;

namespace USR4.Repository
{
    public interface IModuloRepository
    {
        Task<int> CrearAsync(Modulo modulo);
        Task ActualizarAsync(Modulo modulo);
        Task EliminarAsync(int id);
        Task<IEnumerable<Modulo>> ObtenerTodosAsync(string? nombre = null);
        Task<Modulo?> ObtenerPorIdAsync(int id);
        Task<Modulo?> ObtenerPorNombreAsync(string nombre);
    }
}