using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Pages;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administracion.Pages.ADM10_Cursos
{
    public class EditarModel : BasePageModel
    {
        private readonly ICursoApiClient _cursoClient;

        public EditarModel(
            ICursoApiClient cursoClient,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<EditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _cursoClient = cursoClient;
        }

        [BindProperty]
        public CursoDto CursoEditado { get; set; } = new();

        public List<SelectListItem> Carreras { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            var curso = await _cursoClient.ObtenerPorIdAsync(id);
            if (curso == null)
                return RedirectToPage("Index");

            CursoEditado = curso;

            var lookups = await _cursoClient.ObtenerLookupsAsync();
            if (lookups != null)
            {
                Carreras = lookups.Carreras
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre,
                        Selected = c.Id == curso.ID_Carrera
                    })
                    .ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            if (CursoEditado.ID_Carrera == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar una carrera.");

                var lookups = await _cursoClient.ObtenerLookupsAsync();
                if (lookups != null)
                {
                    Carreras = lookups.Carreras
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Nombre,
                            Selected = c.Id == CursoEditado.ID_Carrera
                        })
                        .ToList();
                }

                return Page();
            }

            var (ok, code, msg) = await _cursoClient.ActualizarAsync(CursoEditado);

            if (ok)
            {
                TempData["Mensaje"] = msg;
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, msg);

            var lookupsFail = await _cursoClient.ObtenerLookupsAsync();
            if (lookupsFail != null)
            {
                Carreras = lookupsFail.Carreras
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre,
                        Selected = c.Id == CursoEditado.ID_Carrera
                    })
                    .ToList();
            }

            return Page();
        }

    }
}
