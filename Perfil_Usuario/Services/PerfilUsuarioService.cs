using Perfil_Usuario.Entities;
using Perfil_Usuario.Repository;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<BusinessLogicResponse> ActualizarContrasenaAsync(string email, string nuevaContrasena)
        {
            string usuario = "sistema";

            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nuevaContrasena))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe especificar un email y una contraseña válida."
                    };

                var contrasennaHash = EncriptarContrasenna(nuevaContrasena);

                var filas = await _repository.ActualizarContrasenaAsync(email, contrasennaHash);


                if (filas == 0)
                {
                    await _bitacora.RegistrarAccionAsync(usuario, "UPDATE", new
                    {
                        accion = "ActualizarContrasena",
                        email,
                        detalle = "Usuario no encontrado"
                    });

                    return new BusinessLogicResponse
                    {
                        StatusCode = 404,
                        Message = "No se encontró el usuario."
                    };
                }

                // bitácora OK
                await _bitacora.RegistrarAccionAsync(usuario, "UPDATE", new
                {
                    accion = "ActualizarContrasena",
                    email,
                    actualizado = true
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Contraseña actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                await _bitacora.RegistrarAccionAsync(usuario, "ERROR", new
                {
                    accion = "ActualizarContrasena",
                    mensaje = ex.Message
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno: {ex.Message}"
                };
            }
        }

        private string EncriptarContrasenna(string contrasenna)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasenna));
            return Convert.ToBase64String(bytes);
        }



    }
}