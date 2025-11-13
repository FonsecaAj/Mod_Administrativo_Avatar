using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM12_Periodo
{
    public class EliminarModel : PageModel
    {
        private readonly IPeriodoApiClient _api;

        public EliminarModel(IPeriodoApiClient api)
        {
            _api = api;
        }

        public PeriodoDto? Periodo { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Periodo = await _api.ObtenerPorIdAsync(id);
            if (Periodo == null)
            {
                MensajeError = "Periodo no encontrado.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ok = await _api.EliminarAsync(id);

            if (ok)
            {
                Mensaje = "Periodo eliminado correctamente.";
            }
            else
            {
                MensajeError = "No se puede eliminar el periodo. Puede estar en uso o hubo un error en la API.";
            }

            return RedirectToPage("Index");
        }
    }
}
