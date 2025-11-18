
using ApiACD6.Entities;

namespace ApiACD6.Services
{
    public interface IProfesorService
    {
        Task<BusinessLogicResponse> ObtenerTodos();
        Task<BusinessLogicResponse> ObtenerPorId(int idProfesor);
        Task<BusinessLogicResponse> Crear(Profesor profesor);
        Task<BusinessLogicResponse> Actualizar(Profesor profesor);
        Task<BusinessLogicResponse> Eliminar(int idProfesor);
    }
}
