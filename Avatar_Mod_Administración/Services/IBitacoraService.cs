using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IBitacoraService
    {
        Task<bool> RegistrarAsync(BitacoraCrearDto dto, string token);
        Task<List<Bitacora>> ObtenerTodosAsync(string token, BitacoraFiltroDto? filtro = null);
        Task<Bitacora?> ObtenerPorIdAsync(int id, string token);
    }
}