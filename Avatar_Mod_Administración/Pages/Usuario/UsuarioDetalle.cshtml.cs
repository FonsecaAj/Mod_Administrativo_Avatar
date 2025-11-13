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
    }
}