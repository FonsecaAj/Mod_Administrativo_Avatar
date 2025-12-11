using ApiMAT1.Entities;
using ApiMAT1.Repository;
using Dapper;

namespace ApiMAT1.Services
{
    public class PrematriculaService : IPrematriculaService
    {
        private readonly IPrematriculaRepository _repository;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;

        public PrematriculaService(
            IPrematriculaRepository repository,
            IDbConnectionFactory connectionFactory,
            BitacoraConsumer bitacoraConsumer,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _connectionFactory = connectionFactory;
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

        public async Task<BusinessLogicResponse> ObtenerTodas()
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var prematriculas = _repository.ObtenerTodas();

                if (!prematriculas.Any())
                {
                    await RegistrarBitacoraAsync("CONSULTA", new { detalle = "Consulta sin resultados de prematrículas" });
                    response.StatusCode = 404;
                    response.Message = "No existen prematrículas registradas.";
                    return response;
                }

                await RegistrarBitacoraAsync("CONSULTA", new { resultado = $"{prematriculas.Count()} prematrículas encontradas" });
                response.StatusCode = 200;
                response.Message = "Prematrículas obtenidas correctamente.";
                response.ResponseObject = prematriculas;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }


        public async Task<BusinessLogicResponse> ObtenerPorId(int id)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (id <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "El ID debe ser válido.";
                    return response;
                }

                var prematricula = _repository.ObtenerPorId(id);
                if (prematricula == null)
                {
                    await RegistrarBitacoraAsync("CONSULTA", new { detalle = $"Prematrícula ID {id} no encontrada" });
                    response.StatusCode = 404;
                    response.Message = "La prematrícula especificada no existe.";
                    return response;
                }

                await RegistrarBitacoraAsync("CONSULTA", new { detalle = $"Consulta de prematrícula ID {id}" });
                response.StatusCode = 200;
                response.Message = "Prematrícula obtenida correctamente.";
                response.ResponseObject = prematricula;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

      
        public async Task<BusinessLogicResponse> Crear(Prematricula entidad)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (entidad.ID_Estudiante <= 0 || entidad.ID_Carrera <= 0 || entidad.ID_Curso <= 0 || entidad.ID_Periodo <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "Todos los campos requeridos deben tener un valor válido.";
                    return response;
                }

                using var connection = _connectionFactory.CreateConnection();
                const string sqlPeriodo = "SELECT Fecha_Inicio FROM Periodo WHERE ID_Periodo = @ID_Periodo";
                var fechaInicio = connection.QueryFirstOrDefault<DateTime?>(sqlPeriodo, new { entidad.ID_Periodo });

                if (fechaInicio == null)
                {
                    response.StatusCode = 400;
                    response.Message = "El periodo especificado no existe.";
                    return response;
                }

                if (fechaInicio <= DateTime.Now)
                {
                    response.StatusCode = 400;
                    response.Message = "Solo se pueden prematricular periodos futuros.";
                    return response;
                }

                _repository.Crear(entidad);
                await RegistrarBitacoraAsync("CREAR", new { accion = "INSERT", resultado = entidad });

                response.StatusCode = 201;
                response.Message = "Prematrícula creada correctamente.";
                response.ResponseObject = entidad;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

        
        public async Task<BusinessLogicResponse> Actualizar(Prematricula entidad)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (entidad.ID_Prematricula <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "Debe indicar un ID válido para actualizar.";
                    return response;
                }

                var anterior = _repository.ObtenerPorId(entidad.ID_Prematricula);
                if (anterior == null)
                {
                    response.StatusCode = 404;
                    response.Message = "No se encontró la prematrícula a modificar.";
                    return response;
                }

                _repository.Actualizar(entidad);
                await RegistrarBitacoraAsync("ACTUALIZAR", new { anterior, nuevo = entidad });

                response.StatusCode = 200;
                response.Message = "Prematrícula actualizada correctamente.";
                response.ResponseObject = entidad;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

   
        public async Task<BusinessLogicResponse> Eliminar(int id)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (id <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "Debe indicar un ID válido para eliminar.";
                    return response;
                }

                var eliminado = _repository.ObtenerPorId(id);
                if (eliminado == null)
                {
                    response.StatusCode = 404;
                    response.Message = "La prematrícula no existe.";
                    return response;
                }

                _repository.Eliminar(id);
                await RegistrarBitacoraAsync("ELIMINAR", new { accion = "DELETE", resultado = eliminado });

                response.StatusCode = 200;
                response.Message = "Prematrícula eliminada correctamente.";
                response.ResponseObject = eliminado;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }


        public async Task<BusinessLogicResponse> Obtener_Prematri_Estudiante(string identificacion)
        {
            var response = new BusinessLogicResponse();

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                response.StatusCode = 400;
                response.Message = "La identificación del estudiante es requerida.";
                return response;
            }

            try
            {
                // Llama al método del repositorio para obtener los datos detallados
                var prematriculas = await _repository.Obtener_Prematri_Estudiante(identificacion);

                if (prematriculas == null || !prematriculas.Any())
                {
                    await RegistrarBitacoraAsync("CONSULTA", new { detalle = $"Prematrículas no encontradas para ID {identificacion}" });
                    response.StatusCode = 404;
                    response.Message = $"No se encontraron prematrículas registradas para la identificación {identificacion}.";
                    return response;
                }

                await RegistrarBitacoraAsync("CONSULTA", new { detalle = $"Consulta de {prematriculas.Count()} prematrículas para ID {identificacion}" });
                response.StatusCode = 200;
                response.Message = $"Historial de prematrículas para {identificacion} obtenido correctamente.";
                response.ResponseObject = prematriculas;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }
    }
}
