using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.Pagos
{
    public class PagosModel : BasePageModel
    {
        private readonly IPagoApiClient _api;

        public PagosModel(
            IPagoApiClient api,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<PagosModel> logger)
            : base(authService, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty] public int ID_Factura { get; set; }
        [BindProperty] public int ID_Pago { get; set; }
        [BindProperty] public string MetodoPago { get; set; } = "Tarjeta";
        [BindProperty] public string? Motivo { get; set; }
        public PagoDto? Pago { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de factura válido.";
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg, pago) = await _api.CrearPagoAsync(ID_Factura, MetodoPago, token);

            if (!ok)
            {
                ErrorMessage = msg;
                return Page();
            }

            Pago = pago;
            Message = msg;
            return Page();
        }

        public async Task<IActionResult> OnPostReversarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (ID_Pago <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de pago válido.";
                return Page();
            }

            
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                ErrorMessage = "Debe indicar un motivo de reversión.";
                return Page();
            }

            var token = ObtenerToken();

            
            var (ok, status, msg) = await _api.ReversarPagoAsync(ID_Pago, Motivo!, token);

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
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (ID_Pago <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de pago válido.";
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg, pago) = await _api.ObtenerPagoAsync(ID_Pago, token);

            if (!ok)
            {
                ErrorMessage = msg;
                return Page();
            }

            Pago = pago;
            Message = msg;
            return Page();
        }
    }
}
