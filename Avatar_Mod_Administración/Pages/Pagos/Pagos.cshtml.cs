using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Pagos
{
    public class PagosModel : PageModel
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

        // --- Propiedades para Registrar, Reversar y Consultar por ID ---
        [BindProperty] public int ID_Factura { get; set; }
        [BindProperty] public int ID_Pago { get; set; }
        [BindProperty] public string MetodoPago { get; set; } = "Tarjeta";

        public PagoDto? Pago { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        // --- Listar por Período ---
        [BindProperty] public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
        [BindProperty] public DateTime FechaFin { get; set; } = DateTime.Today;
        public IEnumerable<PagoDto>? PagosListado { get; set; } = Enumerable.Empty<PagoDto>();


        public async Task<IActionResult> OnGetAsync()
        {
            // Llama a la inicialización de sesión que verifica/renueva el token 
            // y carga los datos de usuario (nombre, rol) en ViewData.
            var result = await InicializarSesionAsync();

            // Si la sesión expiró o es inválida, redirige a Login.
            if (result != null) return result;

            // Retorna la página.
            return Page();
        }
        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            if (ID_Factura <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de factura válido.";
                return Page();
            }

            var (ok, status, msg, pago) = await _api.CrearPagoAsync(ID_Factura, MetodoPago);
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
            if (ID_Pago <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de pago válido.";
                return Page();
            }

            var (ok, status, msg) = await _api.ReversarPagoAsync(ID_Pago);
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
            if (ID_Pago <= 0)
            {
                ErrorMessage = "Debe ingresar un ID de pago válido.";
                return Page();
            }

            var (ok, status, msg, pago) = await _api.ObtenerPagoAsync(ID_Pago);
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
