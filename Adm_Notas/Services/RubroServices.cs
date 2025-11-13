using Adm_Notas.Entities;
using Adm_Notas.Repository;

namespace Adm_Notas.Services
{
    public class RubroServices : IRubroServices
    {

        private readonly RubroRepository _rubroRepository;
        private readonly BitacoraConsumer _bitacoraConsumer;

        public RubroServices(RubroRepository rubroRepository, BitacoraConsumer bitacoraClient)
        {
            _rubroRepository = rubroRepository;
            _bitacoraConsumer = bitacoraClient;
        }

        // =========================================================
        // CARGAR DESGLOSE DE RUBROS
        // =========================================================
        public async Task<BusinessLogicResponse> CargarDesglose(DesgloseRequest request)
        {
            string usuario = "sistema";

            try
            {
                if (request == null || request.Rubros == null || !request.Rubros.Any())
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe enviar al menos un rubro para cargar el desglose."
                    };
                }

                if (request.Rubros.Sum(r => r.Porcentaje) != 100)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "La sumatoria de los rubros debe ser igual a 100."
                    };
                }

                // Verificar si existen notas asociadas (no permitir modificación)
                var existenNotas = await _rubroRepository.ExistenNotasAsociadas(request.ID_Grupo);
                if (existenNotas)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "No se pueden modificar los rubros porque ya existen notas asignadas al grupo."
                    };
                }

                var total = await _rubroRepository.CargarDesglose(request);

                // Registrar en bitácora
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT", new
                {
                    accion = "CargarDesglose",
                    grupo = request.ID_Grupo,
                    totalRubros = total
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = $"Desglose cargado correctamente ({total} rubros)."
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno al cargar el desglose: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> ObtenerDesglose(int idGrupo)
        {
            string usuario = "sistema";

            try
            {
                if (idGrupo <= 0)
                    return new BusinessLogicResponse { StatusCode = 400, Message = "El parámetro 'idGrupo' es requerido y debe ser válido." };

                var rubros = await _rubroRepository.ObtenerDesglose(idGrupo);

                if (!rubros.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT", new { accion = "ObtenerDesglose", grupo = idGrupo, detalle = "Sin resultados" });
                    return new BusinessLogicResponse { StatusCode = 404, Message = "No existen rubros registrados para el grupo indicado." };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT", new { accion = "ObtenerDesglose", grupo = idGrupo, cantidad = rubros.Count() });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Rubros obtenidos correctamente.",
                    ResponseObject = rubros
                };
            }
            catch (Exception ex)
            {
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "ERROR", new { accion = "ObtenerDesglose", mensaje = ex.Message });
                return new BusinessLogicResponse { StatusCode = 500, Message = $"Error interno: {ex.Message}" };
            }
        }




    }
}
