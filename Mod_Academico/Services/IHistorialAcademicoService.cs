using Mod_Academico.Entities;

namespace Mod_Academico.Services
{
    public interface IHistorialAcademicoService
    {
        Task<BusinessLogicResponse> ObtenerHistorial(string tipo, string identificacion);
    }
}
