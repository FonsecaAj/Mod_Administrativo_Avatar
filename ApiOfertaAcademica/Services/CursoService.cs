
using ApiACD3.Entities;
using ApiACD3.Repository;

namespace ApiACD3.Services
{
    public class CursoService : ICursoService
    {
        private readonly CursoRepository _repo;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public CursoService(
            CursoRepository repo,
            BitacoraConsumer bitacoraConsumer,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
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
                var cursos = await _repo.ObtenerTodos();

                if (!cursos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = "Consulta de cursos sin resultados" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen cursos registrados en el sistema."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { resultado = $"{cursos.Count()} cursos encontrados" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Cursos obtenidos correctamente.",
                    ResponseObject = cursos
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
                        Message = "El parámetro 'id' debe ser válido."
                    };
                }

                var curso = await _repo.ObtenerPorId(id);

                if (curso == null)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = $"Curso con ID {id} no encontrado" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El curso especificado no existe."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { detalle = $"Consulta del curso con ID {id}" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Curso obtenido correctamente.",
                    ResponseObject = curso
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


        public async Task<BusinessLogicResponse> ObtenerPorCarrera(int idCarrera)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (idCarrera <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'idCarrera' debe ser válido."
                    };
                }

                var cursos = await _repo.ObtenerPorCarrera(idCarrera);

                if (!cursos.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                        new { detalle = $"No hay cursos asociados a la carrera {idCarrera}" });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen cursos asociados a la carrera seleccionada."
                    };
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "SELECT",
                    new { resultado = $"{cursos.Count()} cursos encontrados para la carrera {idCarrera}" });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Cursos obtenidos correctamente.",
                    ResponseObject = cursos
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

      
      
        public async Task<BusinessLogicResponse> Crear(Curso curso)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var resultado = ValidacionesService.ValidarCurso(curso);
                if (!resultado.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = resultado.Mensaje
                    };
                }

                var filas = await _repo.Crear(curso);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT",
                    new { accion = "CREAR", resultado = curso });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 201 : 500,
                    Message = filas > 0 ? "Curso creado correctamente." : "Error al crear el curso.",
                    ResponseObject = curso
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


        public async Task<BusinessLogicResponse> Actualizar(Curso curso)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
           
                var resultado = ValidacionesService.ValidarCurso(curso);
                if (!resultado.EsValido)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = resultado.Mensaje
                    };
                }

                if (curso.ID_Curso <= 0)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El identificador del curso no es válido."
                    };
                }

            
                var cursoAnterior = await _repo.ObtenerPorId(curso.ID_Curso);
                if (cursoAnterior == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = $"No se encontró el curso con ID {curso.ID_Curso}."
                    };
                }

               
                var filas = await _repo.Actualizar(curso);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE",
                    new { anterior = cursoAnterior, nuevo = curso });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Curso actualizado correctamente." : "Error al actualizar el curso.",
                    ResponseObject = curso
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
                        Message = "El parámetro 'id' debe ser válido."
                    };
                }

                var cursoEliminado = await _repo.ObtenerPorId(id);
                if (cursoEliminado == null)
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "El curso no existe."
                    };
                }

                var filas = await _repo.Eliminar(id);

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "DELETE",
                    new { accion = "ELIMINAR", resultado = cursoEliminado });

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Curso eliminado correctamente." : "Error al eliminar el curso.",
                    ResponseObject = cursoEliminado
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

        public async Task<BusinessLogicResponse> ObtenerLookups()
        {
            try
            {
                var carreras = await _repo.ObtenerCarrerasLookup();

                var niveles = Enumerable.Range(1, 12)
                    .Select(n => new { Id = n, Nombre = $"Nivel {n}" });

                var result = new
                {
                    Carreras = carreras,
                    Niveles = niveles
                };

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Datos obtenidos correctamente.",
                    ResponseObject = result
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
