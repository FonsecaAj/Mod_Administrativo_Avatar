using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class EliminarModel : PageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;

        public EliminarModel(IPrematriculaApiClient prematriculaApi)
        {
            _prematriculaApi = prematriculaApi;
        }

        public PrematriculaDto? Prematricula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Prematricula = await _prematriculaApi.ObtenerPorIdAsync(id);
            if (Prematricula == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ok = await _prematriculaApi.EliminarAsync(id);

          
            return RedirectToPage("Index");
        }
    }
}
