using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace Avatar_Mod_Administración.Pages.ADM15_Matricula
{
    public class IndexModel : BasePageModel
    {
        private readonly IMatriculaApiClient _matriculaClient;

        public IndexModel(
            IMatriculaApiClient matriculaClient,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _matriculaClient = matriculaClient;
        }



        [BindProperty]
        public MatriculaRequestDto NuevaMatricula { get; set; } = new();

        // ===== Filtros del listado =====

        [BindProperty(SupportsGet = true)]
        public int? CursoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoFiltro { get; set; }

        // ===== Combos básicos =====

        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();



        public List<MatriculaGrupoLookupDto> GruposCrear { get; set; } = new();
        public List<MatriculaGrupoLookupDto> GruposFiltro { get; set; } = new();



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

       
            if (string.IsNullOrWhiteSpace(NuevaMatricula.Identificacion))
                ModelState.AddModelError("NuevaMatricula.Identificacion", "La identificación es obligatoria.");

            if (NuevaMatricula.ID_Periodo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Periodo", "Debe seleccionar un periodo.");

            if (NuevaMatricula.ID_Curso <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Curso", "Debe seleccionar un curso.");

            if (NuevaMatricula.ID_Grupo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Grupo", "Debe seleccionar un grupo.");


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

            return RedirectToPage(new
            {
                CursoFiltro = NuevaMatricula.ID_Curso,
                GrupoFiltro = NuevaMatricula.ID_Grupo
            });
        }


        private async Task CargarCombosAsync()
        {
            var lookups = await _matriculaClient.ObtenerLookupsAsync();

            if (lookups == null)
            {
                Periodos = new();
                Cursos = new();
                GruposCrear = new();
                GruposFiltro = new();
                return;
            }

            // Periodos
            Periodos = lookups.Periodos
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Periodo.ToString(),
                    Text = $"{p.Año} - Periodo {p.Numero_Periodo}"
                })
                .ToList();

            // Cursos
            Cursos = lookups.Cursos
                .Select(c => new SelectListItem
                {
                    Value = c.ID_Curso.ToString(),
                    Text = c.Nombre_Curso
                })
                .ToList();

            // ===== Grupos directamente desde MAT2 (mapeando al tipo Lookup) =====
            var gruposLista = lookups.Grupos
                .Select(g => new MatriculaGrupoLookupDto
                {
                    ID_Grupo = g.ID_Grupo,
                    ID_Curso = g.ID_Curso,
                    Nombre_Curso = g.Nombre_Curso,
                    Nombre_Grupo = g.Nombre_Grupo,
                    Cupo_Disponible = g.Cupo_Disponible,
                    EstadoGrupo = g.EstadoGrupo
                })
                .ToList();

            GruposCrear = gruposLista;
            GruposFiltro = gruposLista;

        }

        // ================== Exportar CSV ==================

        public async Task<IActionResult> OnGetExportarCsvAsync(int? cursoFiltro, int? grupoFiltro)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            if (!cursoFiltro.HasValue || !grupoFiltro.HasValue ||
                cursoFiltro.Value <= 0 || grupoFiltro.Value <= 0)
            {
                return RedirectToPage(new { CursoFiltro = cursoFiltro, GrupoFiltro = grupoFiltro });
            }

            var lista = (await _matriculaClient
                .ObtenerPorCursoYGrupoAsync(cursoFiltro.Value, grupoFiltro.Value)).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Identificación;NombreCompleto;Curso;Grupo;FechaMatrícula");

            foreach (var m in lista)
            {
                sb.AppendLine(
                    $"{m.Identificacion};\"{m.NombreCompleto}\";\"{m.NombreCurso}\";\"{m.NombreGrupo}\";{m.Fecha_Matricula:yyyy-MM-dd}");
            }

            var csv = sb.ToString();
            var bom = Encoding.UTF8.GetPreamble();
            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

            return File(bytes, "text/csv", "Matriculados.csv");
        }
    }
}
