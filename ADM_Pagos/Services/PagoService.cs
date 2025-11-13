using ADM_Pagos.Entities;
using ADM_Pagos.Repository;

namespace ADM_Pagos.Services
{
    public class PagoService : IPagoService
    {

        private readonly PagoRepository _pagoRepository;
        private readonly IAutenticacionService _auth;
        private readonly BitacoraConsumer _bitacora;
        private readonly FacturaCliente _facturas;


        public PagoService(PagoRepository repo, IAutenticacionService auth, BitacoraConsumer bitacora, FacturaCliente facturas)
        {
            _pagoRepository = repo;
            _auth = auth;
            _bitacora = bitacora;
            _facturas = facturas;
        }

        public async Task<BusinessLogicResponse> CrearPagoAsync(PagoRequest request, string? token)
        {
            if (!await _auth.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

            if (request.ID_Factura <= 0 || string.IsNullOrWhiteSpace(request.Metodo_Pago))
                return new BusinessLogicResponse { StatusCode = 400, Message = "Datos de pago inválidos." };

            // 1) Validar factura en microservicio de facturación
            var factura = await _facturas.ObtenerFacturaAsync(request.ID_Factura, token);
            if (factura == null)
                return new BusinessLogicResponse { StatusCode = 404, Message = "Factura no encontrada." };

            if (!string.Equals(factura.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
                return new BusinessLogicResponse { StatusCode = 400, Message = "La factura no está pendiente." };

            // 2) Crear pago por el total de la factura
            var idPago = await _pagoRepository.CrearPagoAsync(factura.ID_Factura, factura.Total, request.Metodo_Pago);

            // 3) Marcar factura como Pagada
            await _facturas.MarcarFacturaPagadaAsync(factura.ID_Factura, token);

            await _bitacora.RegistrarAsync(usuario, "INSERT", new
            {
                accion = "CrearPago",
                factura = factura.ID_Factura,
                pago = idPago,
                monto = factura.Total,
                metodo = request.Metodo_Pago
            });

            return new BusinessLogicResponse
            {
                StatusCode = 201,
                Message = "Pago registrado correctamente.",
                ResponseObject = new { ID_Pago = idPago, factura.ID_Factura, factura.Total, request.Metodo_Pago }
            };
        }


        public async Task<BusinessLogicResponse> ReversarPagoAsync(int idPago, string? token)
        {
            if (!await _auth.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

            var pago = await _pagoRepository.ObtenerPagoAsync(idPago);
            if (pago == null)
                return new BusinessLogicResponse { StatusCode = 404, Message = "Pago no encontrado." };

            if (string.Equals(pago.Estado, "Reversado", StringComparison.OrdinalIgnoreCase))
                return new BusinessLogicResponse { StatusCode = 400, Message = "El pago ya se encuentra reversado." };

            var filas = await _pagoRepository.ReversarPagoAsync(idPago);

            // (Opcional) reabrir la factura como "Pendiente"
            await _facturas.MarcarFacturaPendienteAsync(pago.ID_Factura, token);

            await _bitacora.RegistrarAsync(usuario, "UPDATE", new
            {
                accion = "ReversarPago",
                pago = idPago,
                factura = pago.ID_Factura
            });

            return new BusinessLogicResponse
            {
                StatusCode = filas > 0 ? 200 : 404,
                Message = filas > 0 ? "Pago reversado correctamente." : "No se pudo reversar el pago."
            };
        }

        public async Task<BusinessLogicResponse> ObtenerPagoAsync(int idPago, string? token)
        {
            if (!await _auth.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            var pago = await _pagoRepository.ObtenerPagoAsync(idPago);
            if (pago == null)
                return new BusinessLogicResponse { StatusCode = 404, Message = "Pago no encontrado." };

            var detalles = await _pagoRepository.ObtenerDetallesAsync(idPago);

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Pago obtenido correctamente.",
                ResponseObject = new PagoConDetalles { Pago = pago, Detalles = detalles }
            };
        }

        public async Task<BusinessLogicResponse> ListadoPorPeriodoAsync(DateTime inicio, DateTime fin, string? token)
        {
            if (!await _auth.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            if (inicio > fin)
                return new BusinessLogicResponse { StatusCode = 400, Message = "Rango de fechas inválido." };

            var pagos = await _pagoRepository.ListadoPorPeriodoAsync(inicio, fin);

            return new BusinessLogicResponse
            {
                StatusCode = 200,
                Message = "Listado de pagos obtenido correctamente.",
                ResponseObject = pagos
            };
        }

    }
}
