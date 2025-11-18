using ApiACD5.Entities;
using ApiACD5.Repository;
using System.Linq;

namespace ApiACD5.Services
{
    public class PeriodoService : IPeriodoService
    {
        private readonly IPeriodoRepository _repository;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public PeriodoService(
            IPeriodoRepository repository,
            BitacoraConsumer bitacoraConsumer,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _bitacoraConsumer = bitacoraConsumer;
            _auth = auth;
            _http = httpContextAccessor;
        }

        private async Task<string> ObtenerUsuarioActualAsync()
        {
            var authorization = _http.HttpContext?.Request?.Headers["Authorization"].ToString();
            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(authorization);
            return string.IsNullOrWhiteSpace(usuario) ? "Anónimo" : usuario;
        }

        public async Task<BusinessLogicResponse> ObtenerTodos()
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var periodos = _repository.ObtenerTodos();

                if (!periodos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = "Consulta de periodos sin resultados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen periodos registrados en el sistema."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { resultado = $"{periodos.Count()} periodos encontrados" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Periodos obtenidos correctamente.",
                    ResponseObject = periodos
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> ObtenerPorId(int id)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (id <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El identificador del periodo debe ser válido."
                    };
                }

                var periodo = _repository.ObtenerPorId(id);

                if (periodo == null)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = $"Periodo con ID {id} no encontrado" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El periodo especificado no existe."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { detalle = $"Consulta del periodo con ID {id}" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Periodo obtenido correctamente.",
                    ResponseObject = periodo
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> Crear(Periodo periodo)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var validacion = ValidarDatos(periodo);
                if (!validacion.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = validacion.Mensaje
                    };
                }

             
                var existentes = _repository.ObtenerTodos();
                var yaExiste = existentes.Any(p =>
                    p.Año == periodo.Año &&
                    p.Numero_Periodo == periodo.Numero_Periodo);

                if (yaExiste)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT",
                        new
                        {
                            accion = "CREAR",
                            detalle = $"Intento de crear periodo duplicado Año={periodo.Año}, Numero_Periodo={periodo.Numero_Periodo}"
                        });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Ya existe un periodo registrado con el mismo año y número."
                    };
                }

                var filas = _repository.Crear(periodo);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT",
                    new { accion = "CREAR", resultado = periodo });

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = filas > 0 ? "Periodo creado correctamente." : "Error al crear el periodo.",
                    ResponseObject = periodo
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> Modificar(Periodo periodo)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (periodo.ID_Periodo <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El ID del periodo no es válido."
                    };
                }

                var anterior = _repository.ObtenerPorId(periodo.ID_Periodo);
                if (anterior == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontró el periodo a modificar."
                    };
                }

                var validacion = ValidarDatos(periodo);
                if (!validacion.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = validacion.Mensaje
                    };
                }

                // Impedir duplicados Año + Numero_Periodo en OTROS registrossssssss
                var existentes = _repository.ObtenerTodos();
                var yaExiste = existentes.Any(p =>
                    p.ID_Periodo != periodo.ID_Periodo &&
                    p.Año == periodo.Año &&
                    p.Numero_Periodo == periodo.Numero_Periodo);

                if (yaExiste)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE",
                        new
                        {
                            accion = "MODIFICAR",
                            detalle = $"Intento de modificar a periodo duplicado Año={periodo.Año}, Numero_Periodo={periodo.Numero_Periodo}"
                        });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Ya existe otro periodo registrado con el mismo año y número."
                    };
                }

                var filas = _repository.Modificar(periodo);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE",
                    new { anterior, nuevo = periodo });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Periodo actualizado correctamente." : "Error al actualizar el periodo.",
                    ResponseObject = periodo
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}"
                };
            }
        }

        public async Task<BusinessLogicResponse> Eliminar(int id)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (id <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El identificador del periodo debe ser válido."
                    };
                }

                var eliminado = _repository.ObtenerPorId(id);
                if (eliminado == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El periodo no existe."
                    };
                }

                var filas = _repository.Eliminar(id);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "DELETE",
                    new { accion = "ELIMINAR", resultado = eliminado });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Periodo eliminado correctamente." : "Error al eliminar el periodo.",
                    ResponseObject = eliminado
                };
            }
            catch (Exception ex)
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}"
                };
            }
        }

        private (bool EsValido, string Mensaje) ValidarDatos(Periodo periodo)
        {
            if (periodo.Año <= 0)
                return (false, "El año debe ser mayor que cero.");
            if (periodo.Numero_Periodo <= 0)
                return (false, "El número de periodo es obligatorio y debe ser mayor que cero.");
            if (periodo.Fecha_Inicio == default)
                return (false, "La fecha de inicio es obligatoria.");
            if (periodo.Fecha_Fin == default)
                return (false, "La fecha de fin es obligatoria.");
            if (periodo.Fecha_Fin < periodo.Fecha_Inicio)
                return (false, "La fecha de fin no puede ser anterior a la fecha de inicio.");

            return (true, string.Empty);
        }
    }
}
