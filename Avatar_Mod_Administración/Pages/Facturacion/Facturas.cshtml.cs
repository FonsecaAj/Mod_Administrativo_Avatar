using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.Facturacion
{
    public class FacturasModel : BasePageModel
    {
        private readonly IFacturaApiClient _api;

        public FacturasModel(
            IFacturaApiClient api,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<FacturasModel> logger)
            : base(authService, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty] public string Identificacion { get; set; } = string.Empty;
        [BindProperty] public int ID_Factura { get; set; }
        [BindProperty] public string? Motivo { get; set; }

        public FacturaDto? Factura { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (string.IsNullOrWhiteSpace(Identificacion))
            {
                ErrorMessage = "Debe ingresar la identificación del estudiante.";
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg, idFactura) = await _api.CrearFacturaAsync(Identificacion, token);

            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;
            return Page();
        }

        public async Task<IActionResult> OnPostReversarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID válido.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                ErrorMessage = "Debe indicar un motivo de reversión.";
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg) = await _api.ReversarFacturaAsync(ID_Factura, Motivo!, token);

            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;
            return Page();
        }

        public async Task<IActionResult> OnPostConsultarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID válido.";
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg, factura) = await _api.ObtenerFacturaAsync(ID_Factura, token);

            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;
            Factura = factura;
            return Page();
        }
    }
}
