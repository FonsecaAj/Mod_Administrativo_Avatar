using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Modulo
{
    public class ModulosModel : BasePageModel
    {
        private readonly IModuloService _moduloService;

        public ModulosModel(
            IModuloService moduloService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ModulosModel> logger)
            : base(authService, usuarioService, logger)
        {
            _moduloService = moduloService;
        }

        public List<Entities.Modulo> Modulos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? NombreBusqueda { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                Modulos = await _moduloService.ObtenerTodosAsync(token, NombreBusqueda?.Trim());
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

            var (ok, status, message) = await _moduloService.EliminarAsync(id, token);

            if (ok)
                TempData["Mensaje"] = message;
            else
                TempData["Error"] = message;

            return RedirectToPage();
        }
    }
}