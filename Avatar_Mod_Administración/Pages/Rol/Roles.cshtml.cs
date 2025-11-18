using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Rol
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

            var token = ObtenerToken()!;
            Roles = await _rolService.ObtenerTodosAsync(token);
            return Page();
        }

        // Usar mensajes dinámicos de la API
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            // Recibir (ok, status, message)
            var (ok, status, message) = await _rolService.EliminarAsync(id, token);

            // Usar el mensaje que viene de la API
            if (ok)
                TempData["Mensaje"] = message;  // "Rol eliminado exitosamente"
            else
                TempData["Error"] = message;    // "Rol no encontrado" o mensaje de error

            return RedirectToPage();
        }
    }
}