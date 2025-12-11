using Adm_Facturacion.Entities;
using Adm_Facturacion.Repository;

namespace Adm_Facturacion.Services
{
    public class FacturaService : IFacturaService
    {
        private readonly FacturaRepository _facturaRepository;
        private readonly IAutenticacionService _authService;
        private readonly BitacoraConsumer _bitacoraConsumer;

        public FacturaService(FacturaRepository facturaRepository, IAutenticacionService authService, BitacoraConsumer bitacoraConsumer)
        {
            _facturaRepository = facturaRepository;
            _authService = authService;
            _bitacoraConsumer = bitacoraConsumer;
        }

        public async Task<BusinessLogicResponse> CrearFacturaAsync(string identificacion, string? token)
        {
            if (!await _authService.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var usuario = await _authService.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

            int idFactura = await _facturaRepository.CrearFacturaAsync(identificacion);

            await _bitacoraConsumer.RegistrarAccionAsync(usuario, "INSERT", new { factura = idFactura, estudiante = identificacion });

            return new BusinessLogicResponse
            {
                StatusCode = 201,
                Message = $"Factura creada correctamente con ID {idFactura}",
                ResponseObject = idFactura
            };
        }

        public async Task<BusinessLogicResponse> ReversarFacturaAsync(int idFactura, string detalle, string? token)
        {
            if (!await _authService.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var usuario = await _authService.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

            var filas = await _facturaRepository.ReversarFacturaAsync(idFactura, detalle);

            await _bitacoraConsumer.RegistrarAccionAsync(usuario, "UPDATE", new
            {
                factura = idFactura,
                accion = "Factura reversada",
                motivo = detalle
            });

            return new BusinessLogicResponse
            {
                StatusCode = filas > 0 ? 200 : 404,
                Message = filas > 0
                    ? "Factura anulada correctamente."
                    : "Factura no encontrada."
            };
        }

        public async Task<BusinessLogicResponse> ObtenerFacturaAsync(int idFactura, string? token)
        {
            if (!await _authService.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var factura = await _facturaRepository.ObtenerFacturaAsync(idFactura);
            if (factura == null)
                return new BusinessLogicResponse { StatusCode = 404, Message = "Factura no encontrada." };

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Factura obtenida correctamente.",
                ResponseObject = factura
            };
        }

        public async Task<BusinessLogicResponse> ObtenerFacturasPorPeriodoAsync(DateTime inicio, DateTime fin, string? token)
        {
            if (!await _authService.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var facturas = await _facturaRepository.ObtenerFacturasPorPeriodoAsync(inicio, fin);
            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Facturación del periodo obtenida correctamente.",
                ResponseObject = facturas
            };
        }

        public async Task<BusinessLogicResponse> Facturas_UsuarioAsync(string identificacion, string? token)
        {

            if (!await _authService.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            // 2. Obtener datos del Repositorio
            var facturas = await _facturaRepository.Facturas_UsuarioAsync(identificacion);

            // 3. Devolver Respuesta
            if (facturas == null || !facturas.Any())
            {
                return new BusinessLogicResponse
                {
                    StatusCode = 404,
                    Message = $"No se encontraron facturas para la identificación {identificacion}."
                };
            }

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = $"Facturas obtenidas correctamente para la identificación {identificacion}.",
                ResponseObject = facturas
            };
        }

    }
}
