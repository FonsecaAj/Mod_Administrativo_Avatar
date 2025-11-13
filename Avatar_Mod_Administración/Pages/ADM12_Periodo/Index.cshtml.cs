using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.ADM12_Periodo
{
    public class IndexModel : PageModel
    {
        private readonly IPeriodoApiClient _api;

        public IndexModel(IPeriodoApiClient api)
        {
            _api = api;
        }

        public List<PeriodoDto> Periodos { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            var lista = (await _api.ObtenerTodosAsync()).ToList();

            Periodos = lista
                .OrderBy(p => p.Anno)
                .ThenBy(p => p.NumeroPeriodo)
                .ToList();
        }
    }
}
