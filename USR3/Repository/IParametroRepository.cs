using USR3.Entities;

namespace USR3.Repository
{
    public interface IParametroRepository
    {
        Task CrearAsync(Parametro parametro);
        Task ActualizarAsync(Parametro parametro);
        Task EliminarAsync(string id);
        Task<IEnumerable<Parametro>> ObtenerTodosAsync();
        Task<IEnumerable<Parametro>> ObtenerPaginadosAsync(int pagina, int porPagina);
        Task<int> ObtenerTotalAsync();
        Task<Parametro?> ObtenerPorIdAsync(string id);
    }
}