using Mod_Matricula.Entities;

namespace Mod_Matricula.Services
{
    public interface IUbicacionesServices
    {

        Task<BusinessLogicResponse> Obtener_Provincias();
        Task<BusinessLogicResponse> Obtener_Cantones(int idProvincia);
        Task<BusinessLogicResponse> Obtener_Distritos(int idProvincia, int idCanton);


    }
}
