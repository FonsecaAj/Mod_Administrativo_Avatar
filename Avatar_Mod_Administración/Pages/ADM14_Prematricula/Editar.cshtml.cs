using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class EditarModel : BasePageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;
        private readonly ICursoApiClient _cursoApi;
        private readonly IPeriodoApiClient _periodoApi;

        public EditarModel(
            IPrematriculaApiClient prematriculaApi,
            ICursoApiClient cursoApi,
            IPeriodoApiClient periodoApi,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<EditarModel> logger)
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var dto = await _prematriculaApi.ObtenerPorIdAsync(id);

            if (dto == null)
                return RedirectToPage("Index");

            Prematricula = dto;

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

            var ok = await _prematriculaApi.ActualizarAsync(
                Prematricula.ID_Prematricula,
                Prematricula
            );

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar la prematrícula.");
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
