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
            return await OnPostListarAsync(esListadoSecundario: true);
        }

        public async Task<IActionResult> OnPostListarAsync(bool esListadoSecundario = false)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (FechaInicio > FechaFin)
            {
                ErrorMessage = "La fecha de inicio no puede ser posterior a la fecha fin.";
                return Page();
            }

            var token = ObtenerToken();

            var estadoParaApi = string.IsNullOrWhiteSpace(EstadoFiltro) ? null : EstadoFiltro;

            var (ok, status, msg, facturas) = await _api.ListarFacturasAsync(
                FechaInicio,
                FechaFin,
                estadoParaApi,
                token);

            // Siempre asignamos 'ErrorMessage' si no es exitoso.
            if (ok && !esListadoSecundario)
            {
                Message = msg;
            }
            else if (!ok)
            {
                ErrorMessage = msg;
            }

            FacturasListado = facturas;

            // Mantenemos el detalle de la factura consultada anteriormente (si existe)

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

            // Mensaje de éxito
            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;

            await OnPostListarAsync(esListadoSecundario: true);

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

            // Mensaje de éxito
            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;

            // el mensaje de la reversión.
            await OnPostListarAsync(esListadoSecundario: true);

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

            // Se asigna el mensaje de éxito de la CONSULTA
            Message = ok ? msg : null;
            ErrorMessage = !ok ? msg : null;
            Factura = factura;

            // el mensaje de la consulta.
            // Usamos 'esListadoSecundario: true'
            await OnPostListarAsync(esListadoSecundario: true);

            // Aseguramos que la Factura individual quede cargada al final
            Factura = factura;

            return Page();
        }
    }
}
