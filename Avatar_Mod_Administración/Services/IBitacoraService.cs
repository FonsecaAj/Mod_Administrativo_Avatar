using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IBitacoraService
    {
            Task<(IEnumerable<Bitacora> Data, int Total)> ConsultarAsync(BitacoraFiltroDto filtro); // sin token
     
    }
}