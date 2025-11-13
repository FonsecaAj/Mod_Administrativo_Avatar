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

        // Propiedades para el filtro de listado
        [BindProperty(SupportsGet = true)] public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
        [BindProperty(SupportsGet = true)] public DateTime FechaFin { get; set; } = DateTime.Today;
        [BindProperty(SupportsGet = true)] public string? EstadoFiltro { get; set; } // Recibe "" o el estado

        public List<FacturaDto>? FacturasListado { get; set; }

        public FacturaDto? Factura { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            // Ejecutar listado inicial
            return await OnPostListarAsync();
        }

        public async Task<IActionResult> OnPostListarAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (FechaInicio > FechaFin)
            {
                ErrorMessage = "La fecha de inicio no puede ser posterior a la fecha fin.";
                return Page();
            }

            var token = ObtenerToken();

            // ⭐ APLICAR LÓGICA DE FILTRADO DE ESTADO:
            // Si el valor es nulo o vacío (lo que ocurre cuando se selecciona "TODOS"),
            // forzamos el valor a null para que el API Client pueda omitir el parámetro de consulta.
            var estadoParaApi = string.IsNullOrWhiteSpace(EstadoFiltro) ? null : EstadoFiltro;

            var (ok, status, msg, facturas) = await _api.ListarFacturasAsync(
                FechaInicio,
                FechaFin,
                estadoParaApi, // Usamos el valor ajustado
                token);

            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;
            FacturasListado = facturas;

            // Mantener el detalle de la factura consultada anteriormente (si existe)
            // Si Factura es null, se anula la variable Factura, lo cual está bien.

            return Page();
        }

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

            // ⭐ Volvemos a listar después de la operación para actualizar la tabla
            await OnPostListarAsync();

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

            // ⭐ Volvemos a listar después de la operación para actualizar la tabla
            await OnPostListarAsync();

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

            // ⭐ Volvemos a listar después de la operación para mantener la tabla de listado cargada
            await OnPostListarAsync();

            // Aseguramos que la Factura individual quede cargada al final
            Factura = factura;

            return Page();
        }
    }
}