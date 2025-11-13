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

        // --- Propiedades para Registrar, Reversar y Consultar por ID ---
        [BindProperty] public int ID_Factura { get; set; }
        [BindProperty] public int ID_Pago { get; set; }
        [BindProperty] public string MetodoPago { get; set; } = "Tarjeta";
        [BindProperty] public string? Motivo { get; set; }
        public PagoDto? Pago { get; set; }
        public IEnumerable<PagoDetalleDto>? DetallesPago { get; set; } = Enumerable.Empty<PagoDetalleDto>();
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        // --- Listar por Período ---
        [BindProperty] public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
        [BindProperty] public DateTime FechaFin { get; set; } = DateTime.Today;
        public IEnumerable<PagoDto>? PagosListado { get; set; } = Enumerable.Empty<PagoDto>();


        

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

            // Validar que el motivo esté presente (capturado desde el modal)
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                ErrorMessage = "Debe indicar un motivo de reversión.";
                return Page();
            }

            var token = ObtenerToken();

            // Llamada a la API de reversión
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

                // Asegurarse de que el detalle esté vacío
                Pago = null;
                DetallesPago = Enumerable.Empty<PagoDetalleDto>();
                return Page();
            }

            var token = ObtenerToken();
            
            var (ok, status, msg, pago, detalles) = await _api.ObtenerPagoAsync(ID_Pago, token);

            if (!ok)
            {
                ErrorMessage = msg;
                Pago = null; // Limpiar el detalle si falla
                DetallesPago = Enumerable.Empty<PagoDetalleDto>(); // Limpiar detalles
                return Page();
            }

            Pago = pago;
           
            DetallesPago = detalles;
            Message = msg;
            return Page();
        }

        public async Task<IActionResult> OnPostListarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (FechaInicio > FechaFin)
            {
                ErrorMessage = "La fecha de inicio no puede ser posterior a la fecha de fin.";
                PagosListado = Enumerable.Empty<PagoDto>();
                return Page();
            }

            var token = ObtenerToken();
            var (ok, status, msg, pagos) = await _api.ListarPagosPorPeriodoAsync(FechaInicio, FechaFin, token);

            if (!ok)
            {
                ErrorMessage = msg;
                PagosListado = Enumerable.Empty<PagoDto>();
                return Page();
            }

            PagosListado = pagos;
            Message = msg;
            return Page();
        }
    }
}