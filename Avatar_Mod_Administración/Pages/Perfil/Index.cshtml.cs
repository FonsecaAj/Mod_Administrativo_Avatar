using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using Microsoft.Extensions.Logging;

namespace Avatar_Mod_Administración.Pages.Perfil
{
    public class IndexModel : BasePageModel
    {
        private readonly IPerfilUsuarioApiClient _perfilApi;

        [BindProperty]
        public string NuevaContrasena { get; set; } = "";

        [BindProperty]
        public string ConfirmarContrasena { get; set; } = "";

        public PerfilUsuarioDto? Perfil { get; set; }

        // Mensajes para la vista
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public IndexModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            IPerfilUsuarioApiClient perfilApiClient,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _perfilApi = perfilApiClient;
        }

        private async Task CargarPerfilAsync()
        {
            try
            {
                var token = ObtenerToken();
                var email = UsuarioEmail;

                var (ok, statusCode, message, data) = await _perfilApi.ObtenerPerfilAsync(email, token);

                Perfil = ok ? data : null;

                if (!ok && string.IsNullOrEmpty(MensajeError))
                {
 
                    MensajeError = $"Error al cargar el perfil ({statusCode}): {message ?? "No se recibió un mensaje de error."}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando perfil");
                if (string.IsNullOrEmpty(MensajeError))
                {
                    MensajeError = "Error inesperado al cargar el perfil.";
                }
            }
        }


        public async Task<IActionResult> OnGet()
        {
            var validar = await InicializarSesionAsync();
            if (validar != null)
                return validar;

            await CargarPerfilAsync();

            return Page();
        }

        // Cambiar contraseña
        public async Task<IActionResult> OnPostCambiarContrasena()
        {
            var validar = await InicializarSesionAsync();
            if (validar != null)
                return validar;


            await CargarPerfilAsync();

            if (Perfil == null)
            {
                // Si el perfil es null, ya MensajeError fue establecido en CargarPerfilAsync
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NuevaContrasena) ||
                string.IsNullOrWhiteSpace(ConfirmarContrasena))
            {
                MensajeError = "Debe completar ambos campos de contraseña.";
                // Ya se llamó a CargarPerfilAsync arriba
                return Page();
            }

            if (NuevaContrasena.Length < 8)
            {
                MensajeError = "La nueva contraseña debe tener al menos 8 caracteres.";
                return Page();
            }

            if (NuevaContrasena != ConfirmarContrasena)
            {
                MensajeError = "Las contraseñas no coinciden. Por favor, revíselas.";
                return Page();
            }

            try
            {
                var token = ObtenerToken();
                var email = UsuarioEmail;

                var (ok, statusCode, message) =
                    await _perfilApi.CambiarContrasenaAsync(email, NuevaContrasena, token);

                if (!ok)
                {
               
                    MensajeError = $"Error al actualizar la contraseña ({statusCode}): {message ?? "No se recibió un mensaje de error."}";
                }
                else
                {
                    // MEJORADO: Mensaje de éxito más claro
                    MensajeExito = "¡Contraseña actualizada correctamente!";
                    // Limpiar los campos después del éxito
                    NuevaContrasena = "";
                    ConfirmarContrasena = "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                MensajeError = " Ocurrió un error inesperado al cambiar la contraseña. Intente de nuevo más tarde.";
            }

            return Page();
        }
    }
}