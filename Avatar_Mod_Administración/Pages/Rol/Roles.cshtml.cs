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

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;
            var resultado = await _rolService.EliminarAsync(id, token);

            if (resultado)
                TempData["Mensaje"] = "Rol eliminado exitosamente";
            else
                TempData["Error"] = "No se pudo eliminar el rol. Verifique dependencias.";

            return RedirectToPage();
        }
    }
}