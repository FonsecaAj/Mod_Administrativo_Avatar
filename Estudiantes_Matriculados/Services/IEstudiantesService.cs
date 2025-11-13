using Estudiantes_Matriculados.Entities;

namespace Estudiantes_Matriculados.Services
{
    public interface IEstudiantesService
    {
        Task<BusinessLogicResponse> ObtenerListadoPorPeriodo(int idPeriodo);
    }
}
