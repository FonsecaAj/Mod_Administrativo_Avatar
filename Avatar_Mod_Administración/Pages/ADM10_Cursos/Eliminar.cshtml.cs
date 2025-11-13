
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administracion.Pages.ADM10_Cursos
{
    public class EliminarModel : PageModel
    {
        private readonly ICursoApiClient _cursoClient;

        public EliminarModel(ICursoApiClient cursoClient)
        {
            _cursoClient = cursoClient;
        }

        [BindProperty]
        public CursoDto CursoEliminar { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var curso = await _cursoClient.ObtenerPorIdAsync(id);
            if (curso == null)
                return RedirectToPage("Index");

            CursoEliminar = curso;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var exito = await _cursoClient.EliminarAsync(id);
            if (exito)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "No se pudo eliminar el curso.");
            return Page();
        }
    }
}
