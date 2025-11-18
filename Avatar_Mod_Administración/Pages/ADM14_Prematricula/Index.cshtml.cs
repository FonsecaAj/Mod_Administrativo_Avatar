using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class IndexModel : BasePageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;
        private readonly IPeriodoApiClient _periodoApi;
        private readonly ICursoApiClient _cursoApi;

        public IndexModel(
            IPrematriculaApiClient prematriculaApi,
            IPeriodoApiClient periodoApi,
            ICursoApiClient cursoApi,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _prematriculaApi = prematriculaApi;
            _periodoApi = periodoApi;
            _cursoApi = cursoApi;
        }

        [BindProperty(SupportsGet = true)]
        public PrematriculaFiltro Filtro { get; set; } = new();

        public IEnumerable<PrematriculaDto> Prematriculas { get; set; } = new List<PrematriculaDto>();
        public IEnumerable<SelectListItem> Periodos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Carreras { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var periodos = await _periodoApi.ObtenerTodosAsync();
            Periodos = (periodos ?? Enumerable.Empty<PeriodoDto>())
                .Where(p => p.FechaInicio > DateTime.Today)
                .Select(p => new SelectListItem
                {
                    Value = p.IdPeriodo.ToString(),
                    Text = $"{p.Anno}-{p.NumeroPeriodo}"
                });

            var lookups = await _cursoApi.ObtenerLookupsAsync();
            Carreras = (lookups?.Carreras ?? Enumerable.Empty<LookupItem>())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                });

            Cursos = (await _cursoApi.ObtenerTodosAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.ID_Curso.ToString(),
                    Text = c.Nombre
                });

            Prematriculas = await _prematriculaApi.ObtenerTodosAsync(Filtro);

            return Page();
        }
    }
}
