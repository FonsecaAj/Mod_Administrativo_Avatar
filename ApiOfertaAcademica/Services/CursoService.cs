using ApiACD3.Entities;
using ApiACD3.Repository;

namespace ApiACD3.Services
{
    public class CursoService : ICursoService
    {
        private readonly CursoRepository _repo;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public CursoService(
            CursoRepository repo,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _auth = auth;
            _http = httpContextAccessor;
        }

        private async Task<string> ObtenerUsuarioActualAsync()
        {
            var authorization = _http.HttpContext?.Request?.Headers["Authorization"].ToString();
            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(authorization);
            return string.IsNullOrWhiteSpace(usuario) ? "Anonimo" : usuario;
        }

        // ============================================
        // OBTENER TODOS
        // ============================================
        public async Task<BusinessLogicResponse> ObtenerTodos()

        {
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var cursos = await _repo.ObtenerTodos();

                if (!cursos.Any())
                {
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen cursos registrados."
                    };
                }

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

        // ============================================
        // OBTENER POR ID
        // ============================================
        public async Task<BusinessLogicResponse> ObtenerPorId(int id)
        {
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
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "Curso no encontrado."
                    };
                }

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

        // ============================================
        // OBTENER POR CARRERA
        // ============================================
        public async Task<BusinessLogicResponse> ObtenerPorCarrera(int idCarrera)
        {
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
                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No existen cursos para esa carrera."
                    };
                }

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

        // ============================================
        // CREAR
        // ============================================
        public async Task<BusinessLogicResponse> Crear(Curso curso)
        {
            try
            {
                int filas = await _repo.Crear(curso); // filas = int

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 201 : 500,
                    Message = filas > 0 ? "Curso creado correctamente." : "Error al crear curso.",
                    ResponseObject = filas > 0 ? curso : null
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
            try
            {
                int filas = await _repo.Actualizar(curso);

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Curso actualizado correctamente." : "Error al actualizar curso.",
                    ResponseObject = filas > 0 ? curso : null
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
            try
            {
                int filas = await _repo.Eliminar(id);

                return new BusinessLogicResponse
                {
                    StatusCode = filas > 0 ? 200 : 500,
                    Message = filas > 0 ? "Curso eliminado correctamente." : "Error al eliminar curso."
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
                    Message = "Lookups obtenidos correctamente.",
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
