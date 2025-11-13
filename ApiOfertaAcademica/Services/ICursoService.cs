using ApiACD3.Entities;

namespace ApiACD3.Services
{
    public interface ICursoService
    {
        Task<BusinessLogicResponse> ObtenerTodos();
        Task<BusinessLogicResponse> ObtenerPorId(int id);
        Task<BusinessLogicResponse> ObtenerPorCarrera(int idCarrera);
        Task<BusinessLogicResponse> Crear(Curso curso);
        Task<BusinessLogicResponse> Actualizar(Curso curso);
        Task<BusinessLogicResponse> Eliminar(int id);

        Task<BusinessLogicResponse> ObtenerLookups();
    }
}