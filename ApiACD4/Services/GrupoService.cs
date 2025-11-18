using ApiACD4.Entities;
using ApiACD4.Repository;

namespace ApiACD4.Services
{
    public class GrupoService : IGrupoService
    {
        private readonly GrupoRepository _repository;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public GrupoService(
            GrupoRepository repository,
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
                var grupos = await _repository.ObtenerTodos();

                if (!grupos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = "Consulta de grupos sin resultados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen grupos registrados en el sistema."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { resultado = $"{grupos.Count()} grupos encontrados" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Grupos obtenidos correctamente.",
                    ResponseObject = grupos
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
                        Message = "El identificador del grupo debe ser válido."
                    };
                }

                var grupo = await _repository.ObtenerPorId(id);

                if (grupo == null)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = $"Grupo con ID {id} no encontrado" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El grupo especificado no existe."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { detalle = $"Consulta del grupo con ID {id}" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Grupo obtenido correctamente.",
                    ResponseObject = grupo
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

     
        public async Task<BusinessLogicResponse> ObtenerPorCurso(int idCurso)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (idCurso <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'idCurso' debe ser válido."
                    };
                }

                var grupos = await _repository.ObtenerPorCurso(idCurso);

                if (!grupos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = $"No hay grupos asociados al curso {idCurso}" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen grupos asociados al curso seleccionado."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { resultado = $"{grupos.Count()} grupos encontrados para el curso {idCurso}" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Grupos obtenidos correctamente.",
                    ResponseObject = grupos
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


        public async Task<BusinessLogicResponse> Crear(Grupo grupo)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var validacion = ValidarDatos(grupo);
                if (!validacion.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = validacion.Mensaje
                    };
                }

              
                var existentes = await _repository.ObtenerTodos();
                var yaExiste = existentes.Any(g =>
                    g.ID_Curso == grupo.ID_Curso &&
                    g.ID_Periodo == grupo.ID_Periodo &&
                    g.Numero_Grupo == grupo.Numero_Grupo);

                if (yaExiste)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT",
                        new
                        {
                            accion = "CREAR",
                            detalle = $"Intento de crear grupo duplicado: Curso={grupo.ID_Curso}, Periodo={grupo.ID_Periodo}, NumeroGrupo={grupo.Numero_Grupo}"
                        });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Ya existe un grupo con el mismo número para ese curso y periodo."
                    };
                }

                var nuevoId = await _repository.Crear(grupo);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT",
                    new { accion = "CREAR", resultado = grupo });

                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = "Grupo creado correctamente.",
                    ResponseObject = new
                    {
                        ID_Generado = nuevoId,
                        grupo.Numero_Grupo,
                        grupo.Horario
                    }
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

   
        public async Task<BusinessLogicResponse> Actualizar(Grupo grupo)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (grupo.ID_Grupo <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El ID del grupo no es válido."
                    };
                }

                var grupoAnterior = await _repository.ObtenerPorId(grupo.ID_Grupo);
                if (grupoAnterior == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontró el grupo a actualizar."
                    };
                }

                var validacion = ValidarDatos(grupo);
                if (!validacion.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = validacion.Mensaje
                    };
                }

                var existentes = await _repository.ObtenerTodos();
                var yaExiste = existentes.Any(g =>
                    g.ID_Grupo != grupo.ID_Grupo &&
                    g.ID_Curso == grupo.ID_Curso &&
                    g.ID_Periodo == grupo.ID_Periodo &&
                    g.Numero_Grupo == grupo.Numero_Grupo);

                if (yaExiste)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE",
                        new
                        {
                            accion = "MODIFICAR",
                            detalle = $"Intento de modificar grupo a duplicado: Curso={grupo.ID_Curso}, Periodo={grupo.ID_Periodo}, NumeroGrupo={grupo.Numero_Grupo}"
                        });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Ya existe otro grupo con el mismo número para ese curso y periodo."
                    };
                }

                var filas = await _repository.Modificar(grupo);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE",
                    new { anterior = grupoAnterior, nuevo = grupo });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Grupo actualizado correctamente." : "Error al actualizar el grupo.",
                    ResponseObject = grupo
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
                        Message = "El identificador del grupo debe ser válido."
                    };
                }

                var grupoEliminado = await _repository.ObtenerPorId(id);
                if (grupoEliminado == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El grupo no existe."
                    };
                }

             

                var filas = await _repository.Eliminar(id);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "DELETE",
                    new { accion = "ELIMINAR", resultado = grupoEliminado });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Grupo eliminado correctamente." : "Error al eliminar el grupo.",
                    ResponseObject = grupoEliminado
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

  
 
        private (bool EsValido, string Mensaje) ValidarDatos(Grupo grupo)
        {
            if (grupo.Numero_Grupo <= 0)
                return (false, "El número del grupo no puede estar vacío.");

            if (grupo.ID_Curso <= 0)
                return (false, "Debe especificar un curso válido.");

            if (grupo.ID_Profesor <= 0)
                return (false, "Debe especificar un profesor válido.");

            if (string.IsNullOrWhiteSpace(grupo.Horario))
                return (false, "El horario no puede estar vacío.");

            if (grupo.ID_Periodo <= 0)
                return (false, "El periodo no puede estar vacío.");

            return (true, string.Empty);
        }
    }
}
