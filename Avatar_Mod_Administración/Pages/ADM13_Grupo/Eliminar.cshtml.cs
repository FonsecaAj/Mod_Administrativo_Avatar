using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM13_Grupo
{
    public class EliminarModel : PageModel
    {
        private readonly IGrupoApiClient _api;

        public EliminarModel(IGrupoApiClient api)
        {
            _api = api;
        }

        public GrupoDto? Grupo { get; set; }

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Grupo = await _api.ObtenerPorIdAsync(id);
            if (Grupo == null)
            {
                MensajeError = "Grupo no encontrado.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ok = await _api.EliminarAsync(id);

            if (ok)
            {
                Mensaje = "Grupo eliminado correctamente.";
            }
            else
            {
                MensajeError = "No se puede eliminar el grupo. Puede tener matrículas asociadas o hubo un error en la API.";
            }

            return RedirectToPage("Index");
        }
    }
}
