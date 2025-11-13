using Estudiantes_Matriculados.Entities;
using Estudiantes_Matriculados.Repository;

namespace Estudiantes_Matriculados.Services
{
    public class ListadoService : IEstudiantesService
    {


        private readonly ListadoRepository _listadoRepository;
        private readonly BitacoraConsumer _bitacora;
        //private readonly IAutenticacionService _auth;

        public ListadoService(ListadoRepository repo, BitacoraConsumer bitacora)
        {
            _listadoRepository = repo;
            _bitacora = bitacora;
            //_auth = auth;
        }

        public async Task<BusinessLogicResponse> ObtenerListadoPorPeriodo(int idPeriodo)
        {
            string usuario = "sistema";

            try
            {
                if (idPeriodo <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe indicar un ID de periodo válido."
                    };
                }

                var listado = await _listadoRepository.ObtenerListadoEstudiantesPorPeriodo(idPeriodo);

                if (!listado.Any())
                {
                    //await _bitacora.RegistrarAccionAsync(usuario, "SELECT",
                    //    new { accion = "ListadoEstudiantes", periodo = idPeriodo, resultado = "Sin resultados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontraron estudiantes matriculados en el periodo indicado."
                    };
                }

                //await _bitacora.RegistrarAccionAsync(usuario, "SELECT",
                //    new { accion = "ListadoEstudiantes", periodo = idPeriodo, cantidad = listado.Count() });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Listado de estudiantes obtenido correctamente.",
                    ResponseObject = listado
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno: {ex.Message}"
                };
            }
        }


    }
}
