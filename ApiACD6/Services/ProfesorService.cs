using ApiACD6.Entities;
using ApiACD6.Repository;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ApiACD6.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly IProfesorRepository _repository;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;
        private readonly string _dominioCorreo;

        public ProfesorService(
            IProfesorRepository repository,
            IConfiguration configuration,
            BitacoraConsumer bitacoraConsumer,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _bitacoraConsumer = bitacoraConsumer;
            _auth = auth;
            _http = httpContextAccessor;
            _dominioCorreo = configuration["ConfiguracionesGenerales:DominioCorreo"] ?? "cuc.ac.cr";
        }

        private async Task<string> ObtenerUsuarioActualAsync()
        {
            var authorization = _http.HttpContext?.Request?.Headers["Authorization"].ToString();
            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(authorization);
            return string.IsNullOrWhiteSpace(usuario) ? "Anónimo" : usuario;
        }


        public async Task<BusinessLogicResponse> ObtenerTodos()
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var profesores = _repository.ObtenerTodos();

                if (!profesores.Any())
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "CONSULTA",
                        new { detalle = "Consulta sin resultados de profesores" });

                    response.StatusCode = 404;
                    response.Message = "No existen profesores registrados.";
                    return response;
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "CONSULTA",
                    new { resultado = $"{profesores.Count()} profesores encontrados" });

                response.StatusCode = 200;
                response.Message = "Profesores obtenidos correctamente.";
                response.ResponseObject = profesores;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }


        public async Task<BusinessLogicResponse> ObtenerPorId(int idProfesor)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (idProfesor <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "El ID del profesor debe ser válido.";
                    return response;
                }

                var profesor = _repository.ObtenerPorId(idProfesor);
                if (profesor == null)
                {
                    await _bitacoraConsumer.RegistrarAccionAsync(usuario, "CONSULTA",
                        new { detalle = $"Profesor con ID {idProfesor} no encontrado" });

                    response.StatusCode = 404;
                    response.Message = "Profesor no encontrado.";
                    return response;
                }

                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "CONSULTA",
                    new { detalle = $"Consulta del profesor con ID {idProfesor}" });

                response.StatusCode = 200;
                response.Message = "Profesor obtenido correctamente.";
                response.ResponseObject = profesor;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

       
        public async Task<BusinessLogicResponse> Crear(Profesor profesor)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                var validacion = ValidarProfesor(profesor);
                if (!validacion.EsValido)
                {
                    response.StatusCode = 400;
                    response.Message = validacion.Mensaje;
                    return response;
                }

                _repository.Crear(profesor);
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "CREAR", new { accion = "INSERT", resultado = profesor });

                response.StatusCode = 201;
                response.Message = "Profesor creado correctamente.";
                response.ResponseObject = profesor;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }


        public async Task<BusinessLogicResponse> Actualizar(Profesor profesor)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (profesor.ID_Profesor <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "El ID del profesor no es válido.";
                    return response;
                }

                var anterior = _repository.ObtenerPorId(profesor.ID_Profesor);
                if (anterior == null)
                {
                    response.StatusCode = 404;
                    response.Message = "No se encontró el profesor a actualizar.";
                    return response;
                }

                var validacion = ValidarProfesor(profesor);
                if (!validacion.EsValido)
                {
                    response.StatusCode = 400;
                    response.Message = validacion.Mensaje;
                    return response;
                }

                _repository.Actualizar(profesor);
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "ACTUALIZAR", new { anterior, nuevo = profesor });

                response.StatusCode = 200;
                response.Message = "Profesor actualizado correctamente.";
                response.ResponseObject = profesor;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

      
        public async Task<BusinessLogicResponse> Eliminar(int idProfesor)
        {
            var response = new BusinessLogicResponse();
            var usuario = await ObtenerUsuarioActualAsync();

            try
            {
                if (idProfesor <= 0)
                {
                    response.StatusCode = 400;
                    response.Message = "El ID debe ser válido.";
                    return response;
                }

                var profesor = _repository.ObtenerPorId(idProfesor);
                if (profesor == null)
                {
                    response.StatusCode = 404;
                    response.Message = "El profesor no existe.";
                    return response;
                }

                _repository.Eliminar(idProfesor);
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, "ELIMINAR", new { accion = "DELETE", resultado = profesor });

                response.StatusCode = 200;
                response.Message = "Profesor eliminado correctamente.";
                response.ResponseObject = profesor;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error interno: {ex.Message}";
            }

            return response;
        }

        private (bool EsValido, string Mensaje) ValidarProfesor(Profesor profesor)
        {
            if (string.IsNullOrWhiteSpace(profesor.Numero_Identificacion) ||
                string.IsNullOrWhiteSpace(profesor.Email) ||
                string.IsNullOrWhiteSpace(profesor.Nombre_Completo) ||
                string.IsNullOrWhiteSpace(profesor.Telefono))
                return (false, "Todos los campos son obligatorios.");

            if (!Regex.IsMatch(profesor.Nombre_Completo, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$"))
                return (false, "El nombre solo puede contener letras y espacios.");

            int edad = DateTime.Today.Year - profesor.Fecha_Nacimiento.Year;
            if (profesor.Fecha_Nacimiento.Date > DateTime.Today.AddYears(-edad))
                edad--;
            if (edad < 18)
                return (false, "El profesor debe ser mayor de edad.");

            string patron = $@"^[\w\.-]+@{Regex.Escape(_dominioCorreo)}$";
            if (!Regex.IsMatch(profesor.Email, patron))
                return (false, $"El correo debe pertenecer al dominio @{_dominioCorreo}.");

            return (true, string.Empty);
        }
    }
}
