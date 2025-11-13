using Adm_Notas.Entities;
using Adm_Notas.Repository;

namespace Adm_Notas.Services
{
    public class NotaService : INotasServices
    {
        private readonly NotaRepository _notaRepository;
        private readonly BitacoraConsumer _bitacoraConsumer;

        public NotaService (NotaRepository rubroRepository, BitacoraConsumer bitacoraConsumer)
        {
            _notaRepository = rubroRepository;
            _bitacoraConsumer = bitacoraConsumer;
        }


        public async Task<BusinessLogicResponse> AsignarNota(NotaRequest request)
        {
            string usuario = "sistema";

            try
            {
                if (request.Valor_Nota < 1 || request.Valor_Nota > 100)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "La nota debe tener un valor entre 1 y 100." };

                var filas = await _notaRepository.AsignarNota(request);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT", new
                {
                    accion = "AsignarNota",
                    estudiante = request.ID_Estudiante,
                    rubro = request.ID_Rubro,
                    nota = request.Valor_Nota
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = filas > 0 ? "Nota asignada correctamente." : "No se pudo asignar la nota."
                };
            }
            catch (Exception ex)
            {
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "ERROR", new { accion = "AsignarNota", mensaje = ex.Message });
                return new BusinessLogicResponse { StatusCode = 500, Message = $"Error interno: {ex.Message}" };
            }
        }

        // =========================================================
        //  OBTENER NOTAS DE UN ESTUDIANTE EN UN CURSO
        // =========================================================
        public async Task<BusinessLogicResponse> ObtenerNotas(int idEstudiante, int idCurso)
        {
            string usuario = "sistema";

            try
            {
                if (idEstudiante <= 0 || idCurso <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Los parámetros 'idEstudiante' y 'idCurso' son requeridos y válidos." };

                var notas = await _notaRepository.ObtenerNotas(idEstudiante, idCurso);

                if (!notas.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT", new { accion = "ObtenerNotas", estudiante = idEstudiante, curso = idCurso, detalle = "Sin resultados" });
                    return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontraron notas para el estudiante en el curso indicado." };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT", new { accion = "ObtenerNotas", estudiante = idEstudiante, curso = idCurso, cantidad = notas.Count() });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Notas obtenidas correctamente.",
                    ResponseObject = notas
                };
            }
            catch (Exception ex)
            {
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "ERROR", new { accion = "ObtenerNotas", mensaje = ex.Message });
                return new BusinessLogicResponse { StatusCode = 500, Message = $"Error interno: {ex.Message}" };
            }
        }
    }
}
