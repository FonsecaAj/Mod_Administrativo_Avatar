using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM13_Grupo
{
    public class EditarModel : PageModel
    {
        private readonly IGrupoApiClient _grupoApi;
        private readonly ICursoApiClient _cursosApi;
        private readonly IProfesorApiClient _profesoresApi;
        private readonly IPeriodoApiClient _periodosApi;

        public EditarModel(
            IGrupoApiClient grupoApi,
            ICursoApiClient cursosApi,
            IProfesorApiClient profesoresApi,
            IPeriodoApiClient periodosApi)
        {
            _grupoApi = grupoApi;
            _cursosApi = cursosApi;
            _profesoresApi = profesoresApi;
            _periodosApi = periodosApi;
        }

        [BindProperty]
        public GrupoDto Grupo { get; set; } = new();

        public List<CursoDto> Cursos { get; set; } = new();
        public List<ProfesorDto> Profesores { get; set; } = new();
        public List<PeriodoDto> Periodos { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var grupo = await _grupoApi.ObtenerPorIdAsync(id);
            if (grupo == null)
            {
                MensajeError = "Grupo no encontrado.";
                return RedirectToPage("Index");
            }

            Grupo = grupo;
            await CargarListasAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarListasAsync();

            if (!ModelState.IsValid)
            {
                MensajeError = "Hay errores de validación en el formulario.";
                return Page();
            }

            var ok = await _grupoApi.ActualizarAsync(Grupo);

            if (ok)
            {
               
                return RedirectToPage("Index");
            }

            MensajeError = "No se pudo actualizar el grupo. Revise número único por curso/periodo u otros errores.";
            return Page();
        }

        private async Task CargarListasAsync()
        {
            Cursos = (await _cursosApi.ObtenerTodosAsync()).ToList();
            Profesores = (await _profesoresApi.ObtenerTodosAsync()).ToList();
            Periodos = (await _periodosApi.ObtenerTodosAsync()).ToList();
        }
    }
}
