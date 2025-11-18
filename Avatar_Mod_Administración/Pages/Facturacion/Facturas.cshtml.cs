using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Facturacion
{
    public class FacturasModel : PageModel
    {
        private readonly IFacturaApiClient _api;
        public FacturasModel(IFacturaApiClient api) => _api = api;

        [BindProperty] public string Identificacion { get; set; } = string.Empty;
        [BindProperty] public int ID_Factura { get; set; }

        public FacturaDto? Factura { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (string.IsNullOrWhiteSpace(Identificacion))
            {
                ErrorMessage = "Debe ingresar la identificación del estudiante.";
                return Page();
            }

            var (ok, status, msg, idFactura) = await _api.CrearFacturaAsync(Identificacion);
            if (!ok)
            {
                ErrorMessage = msg;
                return Page();
            }

            Message = msg;
            return Page();
        }

        public async Task<IActionResult> OnPostReversarAsync()
        {
            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID válido.";
                return Page();
            }

            var (ok, status, msg) = await _api.ReversarFacturaAsync(ID_Factura);
            if (!ok)
            {
                ErrorMessage = msg;
                return Page();
            }

            Message = msg;
            return Page();
        }

        public async Task<IActionResult> OnPostConsultarAsync()
        {
            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID válido.";
                return Page();
            }

            var (ok, status, msg, factura) = await _api.ObtenerFacturaAsync(ID_Factura);
            if (!ok)
            {
                ErrorMessage = msg;
                return Page();
            }

            Factura = factura;
            Message = msg;
            return Page();
        }

    }
}
