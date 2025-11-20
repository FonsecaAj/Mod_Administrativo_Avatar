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

            var token = ObtenerToken()!;

            try
            {
                Modulos = await _moduloService.ObtenerTodosAsync(token, NombreBusqueda?.Trim());
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de módulos");
                TempData["Error"] = "Error al cargar el listado de módulos";
                Modulos = new List<Entities.Modulo>();
                return Page();
            }
        }

        // Usar el mensaje que devuelve la API
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            // Recibir (ok, status, message)
            var (ok, status, message) = await _moduloService.EliminarAsync(id, token);

            // Usar el mensaje que viene de la API
            if (ok)
                TempData["Mensaje"] = message;  // "Módulo eliminado exitosamente"
            else
                TempData["Error"] = message;    // "Módulo no encontrado" o mensaje de error

            return RedirectToPage();
        }
    }
}