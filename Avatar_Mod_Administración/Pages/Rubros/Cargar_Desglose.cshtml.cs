using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace Avatar_Mod_Administración.Pages.Rubros
{
    public class Cargar_DesgloseModel : PageModel
    {
        private readonly IRubroApiClient _api;

        public Cargar_DesgloseModel(IRubroApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public int ID_Grupo { get; set; }

        [BindProperty]
        public DesgloseRequest Desglose { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            if (ID_Grupo != 0)
                Desglose.ID_Grupo = ID_Grupo;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            foreach (var r in Desglose.Rubros)
                r.ID_Grupo = Desglose.ID_Grupo;

            var (ok, statusCode, message) = await _api.CargarDesglose(Desglose);

            if (!ok)
            {
                ErrorMessage = message ?? $"Error {statusCode}: No se pudo cargar el desglose.";
                return Page();
            }

            SuccessMessage = message ?? "Desglose cargado correctamente.";
            return Page();
        }



    }
}
