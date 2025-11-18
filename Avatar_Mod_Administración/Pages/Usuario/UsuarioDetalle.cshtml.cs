using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using UsuarioEntity = Avatar_Mod_Administración.Entities.Usuario;

namespace Avatar_Mod_Administración.Pages.Usuario
{
    public class UsuarioDetalleModel : BasePageModel
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioDetalleModel(
            IUsuarioService usuarioService,
            IAuthService authService,
            ILogger<UsuarioDetalleModel> logger)
            : base(authService, usuarioService, logger)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; } = string.Empty;

        public UsuarioEntity? Usuario { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            Usuario = await _usuarioService.ObtenerPorEmailAsync(Email, ObtenerToken()!);

            if (Usuario == null)
                return NotFound();

            return Page();
        }

        // Usar mensajes dinámicos de la API
        public async Task<IActionResult> OnPostEliminarAsync(string email)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            // Recibir (ok, status, message) igual que en Factura
            var (ok, status, message) = await _usuarioService.EliminarAsync(email, token);

            // Usar el mensaje que viene de la API
            if (ok)
                TempData["Mensaje"] = message;  // "Usuario eliminado exitosamente"
            else
                TempData["Error"] = message;    // "Usuario no encontrado" o mensaje de error

            return RedirectToPage("/Usuario/Usuarios");
        }
    }
}