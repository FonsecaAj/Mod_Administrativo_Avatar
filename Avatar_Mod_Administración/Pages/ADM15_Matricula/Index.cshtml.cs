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

        // ===== Datos para registrar matrícula =====

        [BindProperty]
        public MatriculaRequestDto NuevaMatricula { get; set; } = new();

        // ===== Filtros del listado =====

        [BindProperty(SupportsGet = true)]
        public int? CursoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoFiltro { get; set; }

        // ===== Combos =====
        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> GruposCrear { get; set; } = new();
        public List<SelectListItem> GruposFiltro { get; set; } = new();

        // ===== Listado de estudiantes matriculados =====

        public List<MatriculaDto> Matriculados { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        // ================== GET ==================

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

        // ================== POST (crear matrícula) ==================

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            await CargarCombosAsync();

            // Validaciones básicas en el modelo
            if (string.IsNullOrWhiteSpace(NuevaMatricula.Identificacion))
                ModelState.AddModelError("NuevaMatricula.Identificacion", "La identificación es obligatoria.");

            if (NuevaMatricula.ID_Periodo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Periodo", "Debe seleccionar un periodo.");

            if (NuevaMatricula.ID_Curso <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Curso", "Debe seleccionar un curso.");

            if (NuevaMatricula.ID_Grupo <= 0)
                ModelState.AddModelError("NuevaMatricula.ID_Grupo", "Debe seleccionar un grupo.");

            // Si había filtros, recargar listado (para que no se vea vacío al fallar)
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

            // Después de crear, usamos el curso/grupo de la nueva matrícula como filtros
            return RedirectToPage(new
            {
                CursoFiltro = NuevaMatricula.ID_Curso,
                GrupoFiltro = NuevaMatricula.ID_Grupo
            });
        }

        // ================== Cargar combos desde MAT2 ==================

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

            var gruposBase = lookups.Grupos;

           
            GruposCrear = gruposBase
                .Where(g =>
                    NuevaMatricula != null &&
                    NuevaMatricula.ID_Curso > 0
                        ? g.ID_Curso == NuevaMatricula.ID_Curso 
                        : true
                )
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Grupo.ToString(),
                    Text = $"{g.Nombre_Curso} - {g.Nombre_Grupo} " +
                            $"({g.Cupo_Disponible} cupos, {g.EstadoGrupo})"
                })
                .ToList();

            GruposFiltro = gruposBase
                .Where(g =>
                    CursoFiltro.HasValue && CursoFiltro.Value > 0
                        ? g.ID_Curso == CursoFiltro.Value      
                        : true
                )
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Grupo.ToString(),
                    Text = $"{g.Nombre_Curso} - {g.Nombre_Grupo} " +
                            $"({g.Cupo_Disponible} cupos, {g.EstadoGrupo})"
                })
                .ToList();
        }




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

 
            sb.AppendLine("sep=;");


            sb.AppendLine("Identificación;Nombre completo;Curso;Grupo;Fecha matrícula");

            foreach (var m in lista)
            {
                sb.AppendLine(
                    $"{m.Identificacion};" +
                    $"\"{m.NombreCompleto}\";" +
                    $"\"{m.NombreCurso}\";" +
                    $"\"{m.NombreGrupo}\";" +
                    $"{m.Fecha_Matricula:yyyy-MM-dd}");
            }

            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var bytes = utf8Bom.GetBytes(sb.ToString());

            return File(bytes, "text/csv", "Matriculados.csv");
        }

    }
}
