using ApiMAT1.Entities;

namespace ApiMAT1.Services
{
    public interface IPrematriculaService
    {
      
        Task<BusinessLogicResponse> ObtenerTodas();

       
        Task<BusinessLogicResponse> ObtenerTodas(
            int? idEstudiante,
            int? idPeriodo,
            int? idCarrera,
            int? idCurso
        );

        Task<BusinessLogicResponse> ObtenerPorId(int id);
        Task<BusinessLogicResponse> Crear(Prematricula entidad);
        Task<BusinessLogicResponse> Actualizar(Prematricula entidad);
        Task<BusinessLogicResponse> Eliminar(int id);
    }
}
