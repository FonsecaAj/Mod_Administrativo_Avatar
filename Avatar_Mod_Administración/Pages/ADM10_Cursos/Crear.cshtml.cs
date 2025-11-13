using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Pages;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administracion.Pages.ADM10_Cursos
{
    public class CrearModel : BasePageModel
    {
        private readonly ICursoApiClient _cursoClient;

        public CrearModel(
            ICursoApiClient cursoClient,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<CrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _cursoClient = cursoClient;
        }

        [BindProperty]
        public CursoDto NuevoCurso { get; set; } = new();

        public List<SelectListItem> Carreras { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            var lookups = await _cursoClient.ObtenerLookupsAsync();
            if (lookups != null)
            {
                Carreras = lookups.Carreras
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
                    .ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            if (string.IsNullOrWhiteSpace(NuevoCurso.Nombre) ||
                !NuevoCurso.Nombre.All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch)))
                ModelState.AddModelError("NuevoCurso.Nombre", "El nombre solo puede contener letras y espacios.");

            if (NuevoCurso.Nivel < 1 || NuevoCurso.Nivel > 12)
                ModelState.AddModelError("NuevoCurso.Nivel", "El nivel debe estar entre 1 y 12.");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var exito = await _cursoClient.CrearAsync(NuevoCurso);

            if (exito)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Error al crear el curso. Intente de nuevo.");
            await OnGetAsync();
            return Page();
        }
    }
}
