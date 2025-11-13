using Adm_Notas.Entities;

namespace Adm_Notas.Services
{
    public interface IRubroServices
    {
        // =========================================================
        // RUBROS / DESGLOSE
        // =========================================================

        Task<BusinessLogicResponse> CargarDesglose(DesgloseRequest request);


        Task<BusinessLogicResponse> ObtenerDesglose(int idGrupo);


    }
}
