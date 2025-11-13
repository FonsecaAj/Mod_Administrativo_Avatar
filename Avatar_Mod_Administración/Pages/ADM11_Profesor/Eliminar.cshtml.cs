using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM11_Profesor
{
    public class EliminarModel : PageModel
    {
        private readonly IProfesorApiClient _api;

        public EliminarModel(IProfesorApiClient api)
        {
            _api = api;
        }

        public ProfesorDto? Profesor { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Profesor = await _api.ObtenerPorIdAsync(id);
            if (Profesor == null)
            {
                MensajeError = "Profesor no encontrado.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ok = await _api.EliminarAsync(id);

            if (ok)
            {
                Mensaje = "Profesor eliminado correctamente.";
            }
            else
            {
                MensajeError = "No se puede eliminar el profesor porque tiene grupos activos.";
            }

            return RedirectToPage("Index");
        }
    }
}
