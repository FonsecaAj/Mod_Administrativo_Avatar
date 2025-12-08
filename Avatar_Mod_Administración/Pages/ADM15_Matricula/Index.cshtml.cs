using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM15_Matricula
{
    public class IndexModel : BasePageModel
    {
        private readonly IMatriculaApiClient _matriculaClient;
        private readonly ICursoApiClient _cursoClient;
        private readonly IGrupoApiClient _grupoClient;
        private readonly IPeriodoApiClient _periodoClient;

        public IndexModel(
            IMatriculaApiClient matriculaClient,
            ICursoApiClient cursoClient,
            IGrupoApiClient grupoClient,
            IPeriodoApiClient periodoClient,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _matriculaClient = matriculaClient;
            _cursoClient = cursoClient;
            _grupoClient = grupoClient;
            _periodoClient = periodoClient;
        }

        // ==== Datos para crear matrícula ====

        [BindProperty]
        public MatriculaRequestDto NuevaMatricula { get; set; } = new();

        // ==== Filtros del listado ====

        [BindProperty(SupportsGet = true)]
        public int? CursoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoFiltro { get; set; }

        // ==== Combos y listado ====

        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();

        public List<MatriculaDto> Matriculados { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            await CargarCombosAsync();

            if (CursoFiltro.HasValue && GrupoFiltro.HasValue &&
                CursoFiltro.Value > 0 && GrupoFiltro.Value > 0)
            {
                Matriculados = (await _matriculaClient
                    .ObtenerPorCursoYGrupoAsync(CursoFiltro.Value, GrupoFiltro.Value)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            await CargarCombosAsync();

            // Validaciones
            if (string.IsNullOrWhiteSpace(NuevaMatricula.Identificacion))
                ModelState.AddModelError("NuevaMatricula.Identificacion", "La identificación es obligatoria.");

            if (NuevaMatricula.ID_Periodo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Periodo", "Debe seleccionar un periodo.");

            if (NuevaMatricula.ID_Curso <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Curso", "Debe seleccionar un curso.");

            if (NuevaMatricula.ID_Grupo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Grupo", "Debe seleccionar un grupo.");

            // Recargar listado si había filtros
            if (CursoFiltro.HasValue && GrupoFiltro.HasValue &&
                CursoFiltro.Value > 0 && GrupoFiltro.Value > 0)
            {
                Matriculados = (await _matriculaClient
                    .ObtenerPorCursoYGrupoAsync(CursoFiltro.Value, GrupoFiltro.Value)).ToList();
            }

            if (!ModelState.IsValid)
                return Page();

            var (ok, status, message) = await _matriculaClient.CrearAsync(NuevaMatricula);

            if (!ok)
            {
                MensajeError = message;
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            Mensaje = message;

            // Después de matricular, redirige usando ese curso/grupo como filtros
            return RedirectToPage(new
            {
                CursoFiltro = NuevaMatricula.ID_Curso,
                GrupoFiltro = NuevaMatricula.ID_Grupo
            });
        }

        private async Task CargarCombosAsync()
        {
            // ===== Periodos (solo activos) =====
            var periodos = await _periodoClient.ObtenerTodosAsync();
            periodos = periodos
                .Where(p => p.EstadoCalculado == "Activo")
                .ToList();

            Periodos = periodos
                .Select(p => new SelectListItem
                {
                    Value = p.IdPeriodo.ToString(),
                    Text = $"{p.Anno} - Periodo {p.NumeroPeriodo}"
                })
                .ToList();

            // ===== Cursos =====
            var cursos = await _cursoClient.ObtenerTodosAsync();

            Cursos = cursos
                .Select(c => new SelectListItem
                {
                    Value = c.ID_Curso.ToString(),
                    Text = $"{c.Identificador} - {c.Nombre}"
                })
                .ToList();

            // ===== Grupos =====
            var grupos = await _grupoClient.ObtenerTodosAsync();

            Grupos = grupos
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = $"{g.PeriodoNombre} - {g.CursoNombre} - Grupo {g.NumeroGrupo}"
                })
                .ToList();
        }
    }
}
