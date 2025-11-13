using USR2.Entities;

namespace USR2.Repository
{
    public interface IRolModuloRepository
    {
        Task<int> CrearAsync(RolModulo rolModulo);
        Task EliminarAsync(int id);
        Task EliminarPorRolYModuloAsync(int idRol, int idModulo);
        Task<IEnumerable<RolModulo>> ObtenerPorRolAsync(int idRol);
        Task<RolModulo?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsignacionAsync(int idRol, int idModulo);
        Task EliminarTodosPorRolAsync(int idRol);
    }
}
