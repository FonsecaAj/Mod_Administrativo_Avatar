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

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                var institucion = await _institucionService.ObtenerPorIdAsync(id, token);
                var nombreInstitucion = institucion?.Nombre ?? $"ID {id}";

                var (exito, mensajeError) = await _institucionService.EliminarAsync(id, token);

                if (exito)
                {
                    TempData["Mensaje"] = $"Institución '{nombreInstitucion}' eliminada exitosamente";
                }
                else
                {
                    if (mensajeError?.Contains("carreras") == true || mensajeError?.Contains("relacionadas") == true)
                    {
                        TempData["Error"] = $"No se puede eliminar la institución '{nombreInstitucion}' porque tiene carreras asociadas.";
                    }
                    else if (mensajeError?.Contains("foreign key") == true)
                    {
                        TempData["Error"] = $"No se puede eliminar la institución '{nombreInstitucion}' porque tiene registros relacionados.";
                    }
                    else
                    {
                        TempData["Error"] = mensajeError ?? $"No se pudo eliminar la institución '{nombreInstitucion}'";
                    }
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al eliminar la institución.";
            }

            return RedirectToPage();
        }
    }
}