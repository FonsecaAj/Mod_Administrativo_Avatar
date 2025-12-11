using Mod_Matricula.Entities;
using Mod_Matricula.Repository;
using Mod_Matricula.Services;

namespace Mod_Matricula.Services
{
    public class UbicacionesService : IUbicacionesServices
    {
        private readonly UbicacionesRepository _ubicacionesRepository;
        private readonly BitacoraConsumer _bitacoraConsumer;

        public UbicacionesService(UbicacionesRepository ubicacionesRepository, BitacoraConsumer bitacoraClient)
        {
            _ubicacionesRepository = ubicacionesRepository;
            _bitacoraConsumer = bitacoraClient;
        }

        public async Task<BusinessLogicResponse> Obtener_Provincias()
        {
            string usuario = "sistema";

            try
            {
                var provincias = await _ubicacionesRepository.Obtener_Provincias();

                if (!provincias.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { accion = "SELECT", detalle = "Consulta de provincias sin resultados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen provincias registradas en el sistema."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { accion = "SELECT", resultado = $"{provincias.Count()} provincias encontradas" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Provincias obtenidas correctamente.",
                    ResponseObject = provincias
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
        public async Task<BusinessLogicResponse> Obtener_Cantones(int idProvincia)
        {
            string usuario = "sistema";

            try
            {
                if (idProvincia <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'provincia' es requerido y debe ser válido."
                    };
                }

                var cantones = await _ubicacionesRepository.Obtener_Cantones(idProvincia);

                if (!cantones.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { accion = "SELECT", provincia = idProvincia, detalle = "Sin cantones asociados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen cantones registrados para la provincia especificada."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { accion = "SELECT", provincia = idProvincia, resultado = $"{cantones.Count()} cantones encontrados" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Cantones obtenidos correctamente.",
                    ResponseObject = cantones
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

        public async Task<BusinessLogicResponse> Obtener_Distritos(int idProvincia, int idCanton)
        {
            string usuario = "sistema";

            try
            {
                if (idProvincia <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'provincia' es requerido y debe ser válido."
                    };
                }

                if (idCanton <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'canton' es requerido y debe ser válido."
                    };
                }

                var cantones = await _ubicacionesRepository.Obtener_Cantones(idProvincia);
                bool pertenece = cantones.Any(c => c.ID_Canton == idCanton);

                if (!pertenece)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El cantón no pertenece a la provincia indicada."
                    };
                }

                var distritos = await _ubicacionesRepository.Obtener_Distritos(idProvincia, idCanton);

                if (!distritos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { accion = "SELECT", provincia = idProvincia, canton = idCanton, detalle = "Sin distritos asociados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen distritos asociados a la provincia y cantón seleccionados."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { accion = "SELECT", provincia = idProvincia, canton = idCanton, resultado = $"{distritos.Count()} distritos encontrados" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Distritos obtenidos correctamente.",
                    ResponseObject = distritos
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
