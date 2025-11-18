using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Institucion
{
    // Heredar de BasePageModel en lugar de PageModel
    public class InstitucionesModel : BasePageModel
    {
        private readonly IInstitucionService _institucionService;

        public InstitucionesModel(
            IInstitucionService institucionService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<InstitucionesModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
        }

        public List<Entities.Institucion> Instituciones { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? NombreBusqueda { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // BasePageModel
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                Instituciones = await _institucionService.ObtenerTodosAsync(token, NombreBusqueda?.Trim());
                return Page();
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al cargar el listado de instituciones";
                Instituciones = new List<Entities.Institucion>();
                return Page();
            }
        }

        // Usar el mensaje que devuelve la API
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            // Recibir (ok, status, message) igual que en Usuario
            var (ok, status, message) = await _institucionService.EliminarAsync(id, token);

            // Usar el mensaje que viene de la API
            if (ok)
                TempData["Mensaje"] = message;  // "Institución 'X' eliminada exitosamente"
            else
                TempData["Error"] = message;    // "No se puede eliminar..." o "Institución no encontrada"

            return RedirectToPage();
        }
    }
}