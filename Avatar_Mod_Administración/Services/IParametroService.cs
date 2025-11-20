using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IParametroService
    {
        Task<List<ParametroApi>> ObtenerTodosAsync(string token, int pagina = 1, int porPagina = 30);
        Task<int> ObtenerTotalAsync(string token);
        Task<ParametroApi?> ObtenerPorIdAsync(string id, string token);
        Task<bool> CrearAsync(ParametroCrearDto dto, string token);
        Task<bool> ActualizarAsync(string id, ParametroCrearDto dto, string token);
        Task<bool> EliminarAsync(string id, string token);
    }
}