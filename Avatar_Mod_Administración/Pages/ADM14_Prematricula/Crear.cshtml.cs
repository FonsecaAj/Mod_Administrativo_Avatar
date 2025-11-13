using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class CrearModel : PageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;
        private readonly ICursoApiClient _cursoApi;
        private readonly IPeriodoApiClient _periodoApi;

        public CrearModel(
            IPrematriculaApiClient prematriculaApi,
            ICursoApiClient cursoApi,
            IPeriodoApiClient periodoApi)
        {
            _prematriculaApi = prematriculaApi;
            _cursoApi = cursoApi;
            _periodoApi = periodoApi;
        }

        [BindProperty]
        public PrematriculaDto Prematricula { get; set; } = new();

        public IEnumerable<SelectListItem> Carreras { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Periodos { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            await CargarLookupsAsync();
        }

        private async Task CargarLookupsAsync()
        {
            var lookups = await _cursoApi.ObtenerLookupsAsync();

            Carreras = (lookups?.Carreras ?? Enumerable.Empty<LookupItem>())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                });

            var cursos = await _cursoApi.ObtenerTodosAsync();

            Cursos = (cursos ?? Enumerable.Empty<CursoDto>())
                .Select(c => new SelectListItem
                {
                    Value = c.ID_Curso.ToString(),
                    Text = c.Nombre
                });

            var periodos = await _periodoApi.ObtenerTodosAsync();

            Periodos = (periodos ?? Enumerable.Empty<PeriodoDto>())
                .Where(p => p.FechaInicio > DateTime.Today)
                .Select(p => new SelectListItem
                {
                    Value = p.IdPeriodo.ToString(),
                    Text = $"{p.Anno}-{p.NumeroPeriodo}"
                });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarLookupsAsync();

            if (!ModelState.IsValid)
                return Page();

            var ok = await _prematriculaApi.CrearAsync(Prematricula);

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Error al crear la prematrícula.");
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}
