using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Institucion
{
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
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                Instituciones = await _institucionService.ObtenerTodosAsync(token, NombreBusqueda?.Trim());
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                Instituciones = new List<Entities.Institucion>();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var (ok, status, message) = await _institucionService.EliminarAsync(id, token);

            if (ok)
                TempData["Mensaje"] = message;
            else
                TempData["Error"] = message;

            return RedirectToPage();
        }
    }
}