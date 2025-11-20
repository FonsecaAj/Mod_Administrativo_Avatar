using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Pages;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administracion.Pages.ADM10_Cursos
{
    public class EliminarModel : BasePageModel
    {
        private readonly ICursoApiClient _cursoClient;

        public EliminarModel(
            ICursoApiClient cursoClient,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<EliminarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _cursoClient = cursoClient;
        }

        [BindProperty]
        public CursoDto CursoEliminar { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            var curso = await _cursoClient.ObtenerPorIdAsync(id);
            if (curso == null)
                return RedirectToPage("Index");

            CursoEliminar = curso;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var resultado = await InicializarSesionAsync();
            if (resultado != null)
                return resultado;

            var (ok, code, msg) = await _cursoClient.EliminarAsync(id);

            if (ok)
            {
                TempData["Mensaje"] = msg;
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, msg);
            return Page();
        }

    }
}
