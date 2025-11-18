using ApiACD4.Entities;

namespace ApiACD4.Services
{
    public interface IGrupoService
    {
        Task<BusinessLogicResponse> ObtenerTodos();
        Task<BusinessLogicResponse> ObtenerPorId(int id);
        Task<BusinessLogicResponse> ObtenerPorCurso(int idCurso);
        Task<BusinessLogicResponse> Crear(Grupo grupo);
        Task<BusinessLogicResponse> Actualizar(Grupo grupo);
        Task<BusinessLogicResponse> Eliminar(int id);
    }
}
