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

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            try
            {
                var modulo = await _moduloService.ObtenerPorIdAsync(id, token);
                var nombreModulo = modulo?.Nombre ?? $"ID {id}";

                var (exito, mensajeError) = await _moduloService.EliminarAsync(id, token);

                if (exito)
                {
                    TempData["Mensaje"] = $"Módulo '{nombreModulo}' eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = mensajeError ?? $"No se pudo eliminar el módulo '{nombreModulo}'";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo {Id}", id);
                TempData["Error"] = "Ocurrió un error al eliminar el módulo. Intente nuevamente.";
            }

            return RedirectToPage();
        }
    }
}