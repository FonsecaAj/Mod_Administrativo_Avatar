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

            Factura = factura;
            Message = msg;
            return Page();
        }

    }
}
