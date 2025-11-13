using Mod_Academico.Entities;
using Mod_Academico.Repository;

namespace Mod_Academico.Services
{
    public class HistorialAcademicoService : IHistorialAcademicoService
    {
        private readonly HistorialAcademicoRepository _historialAcademicoRepository;
        private readonly BitacoraConsumer _bitacora;

        public HistorialAcademicoService(HistorialAcademicoRepository historialAcademicoRepository, BitacoraConsumer bitacora)
        {
            _historialAcademicoRepository = historialAcademicoRepository;
            _bitacora = bitacora;
        }

        public async Task<BusinessLogicResponse> ObtenerHistorial(string tipo, string identificacion)
        {
            string usuario = "sistema";

            try
            {
                if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(identificacion))
                    return new BusinessLogicResponse { StatusCode = 400, Message = "Debe especificar un tipo e identificación válidos." };

                var historial = await _historialAcademicoRepository.ObtenerHistorialAsync(tipo, identificacion);

                if (!historial.Any())
                {
                    await _bitacora.RegistrarAccionAsync(usuario, "SELECT", new
                    {
                        accion = "ObtenerHistorial",
                        tipo,
                        identificacion,
                        detalle = "Sin registros de notas"
                    });

                    return new BusinessLogicResponse { StatusCode = 404, Message = "No se encontraron registros académicos para el estudiante indicado." };
                }

                await _bitacora.RegistrarAccionAsync(usuario, "SELECT", new
                {
                    accion = "ObtenerHistorial",
                    tipo,
                    identificacion,
                    cursos = historial.Count()
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Historial académico obtenido correctamente.",
                    ResponseObject = historial
                };
            }
            catch (Exception ex)
            {
                await _bitacora.RegistrarAccionAsync(usuario, "ERROR", new { accion = "ObtenerHistorial", mensaje = ex.Message });
                return new BusinessLogicResponse { StatusCode = 500, Message = $"Error interno: {ex.Message}" };
            }
        }

    }
}
