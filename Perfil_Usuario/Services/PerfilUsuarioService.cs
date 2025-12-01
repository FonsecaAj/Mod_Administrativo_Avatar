using Perfil_Usuario.Entities;
using Perfil_Usuario.Repository;

namespace Perfil_Usuario.Services
{
    public class PerfilUsuarioService : IPerfilUsuarioService
    {
        private readonly PerfilUsuarioRepository _repository;
        private readonly BitacoraConsumer _bitacora;

        public PerfilUsuarioService(PerfilUsuarioRepository repository, BitacoraConsumer bitacora)
        {
            _repository = repository;
            _bitacora = bitacora;
        }

        public async Task<BusinessLogicResponse> ObtenerPerfilAsync(string email)
        {
            string usuario = "sistema";

            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(email))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe especificar un email válido."
                    };

                // Consulta
                var perfil = await _repository.ObtenerPerfilAsync(email);

                // No encontrado
                if (perfil == null)
                {
                    await _bitacora.RegistrarAccionAsync(usuario, "SELECT", new
                    {
                        accion = "ObtenerPerfilUsuario",
                        email,
                        detalle = "No existe el perfil"
                    });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontró el perfil del usuario."
                    };
                }

                // Bitácora exitosa
                await _bitacora.RegistrarAccionAsync(usuario, "SELECT", new
                {
                    accion = "ObtenerPerfilUsuario",
                    email,
                    encontrado = true
                });

                // Respuesta correcta
                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Perfil obtenido correctamente.",
                    ResponseObject = perfil
                };
            }
            catch (Exception ex)
            {
                await _bitacora.RegistrarAccionAsync(usuario, "ERROR", new
                {
                    accion = "ObtenerPerfilUsuario",
                    mensaje = ex.Message
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno: {ex.Message}"
                };
            }
        }
    }
}