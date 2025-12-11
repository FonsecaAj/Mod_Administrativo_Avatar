using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class CrearModel : BasePageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;
        private readonly ICursoApiClient _cursoApi;
        private readonly IPeriodoApiClient _periodoApi;

        public CrearModel(
            IPrematriculaApiClient prematriculaApi,
            ICursoApiClient cursoApi,
            IPeriodoApiClient periodoApi,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<CrearModel> logger)
            : base(auth, usuarioService, logger)
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

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            await CargarLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

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

        private async Task CargarLookupsAsync()
        {
            var lookups = await _cursoApi.ObtenerLookupsAsync();

            Carreras = (lookups?.Carreras ?? Enumerable.Empty<LookupItem>())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre });

            Cursos = (await _cursoApi.ObtenerTodosAsync())
                .Select(c => new SelectListItem { Value = c.ID_Curso.ToString(), Text = c.Nombre });

            Periodos = (await _periodoApi.ObtenerTodosAsync())
                .Where(p => p.FechaInicio > DateTime.Today)
                .Select(p => new SelectListItem
                {
                    Value = p.IdPeriodo.ToString(),
                    Text = $"{p.Anno}-{p.NumeroPeriodo}"
                });
        }
    }
}
