using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administraci�n.Services;
using Avatar_Mod_Administraci�n.Entities;

namespace Avatar_Mod_Administraci�n.Pages.Rol
{
    public class RolesModel : BasePageModel
    {
        private readonly IRolService _rolService;

        public RolesModel(
            IRolService rolService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<RolesModel> logger)
            : base(authService, usuarioService, logger)
        {
            _rolService = rolService;
        }

        public List<Entities.RolApi> Roles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                Roles = await _rolService.ObtenerTodosAsync(token);
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var (ok, status, message) = await _rolService.EliminarAsync(id, token);

            if (ok)
                TempData["Mensaje"] = message;
            else
                TempData["Error"] = message;

            return RedirectToPage();
        }
    }
}