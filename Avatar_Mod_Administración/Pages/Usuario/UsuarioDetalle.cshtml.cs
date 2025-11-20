using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administraci�n.Services;
using UsuarioEntity = Avatar_Mod_Administraci�n.Entities.Usuario;

namespace Avatar_Mod_Administraci�n.Pages.Usuario
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

            try
            {
                Usuario = await _usuarioService.ObtenerPorEmailAsync(Email, ObtenerToken()!);

                if (Usuario == null)
                    return NotFound();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("/Usuario/Usuarios");
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(string email)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var (ok, status, message) = await _usuarioService.EliminarAsync(email, token);

            if (ok)
                TempData["Mensaje"] = message;
            else
                TempData["Error"] = message;

            return RedirectToPage("/Usuario/Usuarios");
        }
    }
}