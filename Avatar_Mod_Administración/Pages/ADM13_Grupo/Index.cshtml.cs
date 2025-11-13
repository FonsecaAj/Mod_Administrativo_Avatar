using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.ADM13_Grupo
{
    public class IndexModel : BasePageModel
    {
        private readonly IGrupoApiClient _gruposApi;
        private readonly ICursoApiClient _cursosApi;
        private readonly IProfesorApiClient _profesoresApi;
        private readonly IPeriodoApiClient _periodosApi;

        public IndexModel(
            IGrupoApiClient gruposApi,
            ICursoApiClient cursosApi,
            IProfesorApiClient profesoresApi,
            IPeriodoApiClient periodosApi,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _gruposApi = gruposApi;
            _cursosApi = cursosApi;
            _profesoresApi = profesoresApi;
            _periodosApi = periodosApi;
        }

        public List<GrupoDto> Grupos { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var grupos = (await _gruposApi.ObtenerTodosAsync()).ToList();
            var cursos = (await _cursosApi.ObtenerTodosAsync()).ToList();
            var profesores = (await _profesoresApi.ObtenerTodosAsync()).ToList();
            var periodos = (await _periodosApi.ObtenerTodosAsync()).ToList();

            var dictCursos = cursos.ToDictionary(c => c.ID_Curso, c => c.Nombre);
            var dictProfes = profesores.ToDictionary(p => p.IdProfesor, p => p.NombreCompleto);
            var dictPeriodos = periodos.ToDictionary(p => p.IdPeriodo,
                                                    p => $"{p.Anno}-{p.NumeroPeriodo}");

            foreach (var g in grupos)
            {
                if (dictCursos.TryGetValue(g.IdCurso, out var cn))
                    g.CursoNombre = cn;

                if (dictProfes.TryGetValue(g.IdProfesor, out var pn))
                    g.ProfesorNombre = pn;

                if (dictPeriodos.TryGetValue(g.IdPeriodo, out var perNombre))
                    g.PeriodoNombre = perNombre;
            }

            Grupos = grupos
                .OrderBy(g => g.CursoNombre ?? string.Empty)
                .ThenBy(g => g.PeriodoNombre ?? string.Empty)
                .ThenBy(g => g.NumeroGrupo)
                .ToList();

            return Page();
        }
    }
}
