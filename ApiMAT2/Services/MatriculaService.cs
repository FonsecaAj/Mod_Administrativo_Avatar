using ApiMAT2.Entities;
using ApiMAT2.Repository;
using System.Text.Json;

namespace ApiMAT2.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly IMatriculaRepository _repository;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public MatriculaService(
            IMatriculaRepository repository,
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

       
        private async Task RegistrarBitacoraAsync(string tipoAccion, object detalle)
        {
            try
            {
                var usuario = await ObtenerUsuarioActualAsync();
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, tipoAccion, detalle);
            }
            catch
            {
                
            }
        }

        private void ValidarCampos(MatriculaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identificacion))
                throw new Exception("La identificación del estudiante es requerida.");

            if (request.ID_Curso <= 0)
                throw new Exception("Debe indicar un curso válido.");

            if (request.ID_Grupo <= 0)
                throw new Exception("Debe indicar un grupo válido.");

            if (request.ID_Periodo <= 0)
                throw new Exception("Debe indicar un periodo válido.");
        }


        public void Crear(MatriculaRequest request)
        {
            // 1. Validaciones básicas del request
            ValidarCampos(request);

            // 2. Buscar estudiante por identificación
            var idEstudiante = _repository.ObtenerIdEstudiantePorIdentificacion(request.Identificacion);
            if (idEstudiante == null)
                throw new Exception("No se encontró un estudiante con esa identificación.");

            // 3. Validar que el periodo esté ACTIVO
            if (!_repository.PeriodoEsActivo(request.ID_Periodo))
                throw new Exception("Solo se permite matricular en períodos activos.");

            // 4. Validar que el grupo pertenezca al curso
            if (!_repository.GrupoPerteneceACurso(request.ID_Grupo, request.ID_Curso))
                throw new Exception("El grupo seleccionado no pertenece al curso indicado.");

            // 5. Validar DUPLICIDAD (misma combinación estudiante–curso–grupo)
            if (_repository.ExisteMatricula(idEstudiante.Value, request.ID_Curso, request.ID_Grupo))
                throw new Exception("Ya existe una matrícula para este estudiante en ese curso y grupo.");

            // 6. Validar CUPO disponible en el grupo (si el backend lo provee)
            var cupoDisponible = _repository.ObtenerCupoDisponible(request.ID_Grupo);
            if (cupoDisponible <= 0)
                throw new Exception("El grupo seleccionado ya no tiene cupos disponibles.");

            // 7. Crear la matrícula
            var matricula = new Matricula
            {
                ID_Estudiante = idEstudiante.Value,
                ID_Grupo = request.ID_Grupo,
                Fecha_Matricula = DateTime.Now
            };

            _repository.Crear(matricula);

            // 8. Registrar en bitácora
            _ = RegistrarBitacoraAsync("CREAR", new
            {
                accion = "INSERT",
                identificacion = request.Identificacion,
                idEstudiante = idEstudiante.Value,
                idCurso = request.ID_Curso,
                idGrupo = request.ID_Grupo,
                fecha = matricula.Fecha_Matricula
            });
        }


        public void Actualizar(MatriculaRequest request)
        {
            ValidarCampos(request);

            var idEstudiante = _repository.ObtenerIdEstudiantePorIdentificacion(request.Identificacion);
            if (idEstudiante == null)
                throw new Exception("No se encontró el estudiante.");

            var matriculaAnterior = _repository.ObtenerPorCursoYGrupo(request.ID_Curso, request.ID_Grupo);

            var matriculaNueva = new Matricula
            {
                ID_Estudiante = idEstudiante.Value,
                ID_Grupo = request.ID_Grupo,
                Fecha_Matricula = DateTime.Now
            };

            _repository.Actualizar(matriculaNueva);

            _ = RegistrarBitacoraAsync("ACTUALIZAR", new
            {
                accion = "UPDATE",
                resultado = new
                {
                    anterior = matriculaAnterior,
                    nuevo = matriculaNueva
                }
            });
        }

       
        public void Eliminar(int id)
        {
            if (id <= 0)
                throw new Exception("El ID de matrícula no es válido.");

            _repository.Eliminar(id);

            _ = RegistrarBitacoraAsync("ELIMINAR", new
            {
                accion = "DELETE",
                resultado = $"El usuario eliminó la matrícula con ID {id}"
            });
        }

        public IEnumerable<object> ObtenerPorCursoYGrupo(int idCurso, int idGrupo)
        {
            var resultado = _repository.ObtenerPorCursoYGrupo(idCurso, idGrupo);

            _ = RegistrarBitacoraAsync("CONSULTA", new
            {
                accion = "SELECT",
                resultado = $"El usuario consulta las matrículas del curso {idCurso} y grupo {idGrupo}"
            });

            return resultado;
        }

        public void Eliminar(MatriculaRequest request)
        {
            throw new NotImplementedException();
        }


        public MatriculaLookupsDto ObtenerLookups()
        {
            var periodos = _repository.ObtenerPeriodos();
            var cursos = _repository.ObtenerCursos();
            var grupos = _repository.ObtenerGrupos();

            return new MatriculaLookupsDto
            {
                Periodos = periodos,
                Cursos = cursos,
                Grupos = grupos
            };
        }

    }
}
