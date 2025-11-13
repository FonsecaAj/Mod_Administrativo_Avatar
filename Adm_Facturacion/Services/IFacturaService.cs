using Adm_Facturacion.Entities;

namespace Adm_Facturacion.Services
{
    public interface IFacturaService
    {
        Task<BusinessLogicResponse> CrearFacturaAsync(string identificacion, string? token);
        Task<BusinessLogicResponse> ReversarFacturaAsync(int idFactura, string? token);
        Task<BusinessLogicResponse> ObtenerFacturaAsync(int idFactura, string? token);
        Task<BusinessLogicResponse> ObtenerFacturasPorPeriodoAsync(DateTime inicio, DateTime fin, string? token);
    }
}
