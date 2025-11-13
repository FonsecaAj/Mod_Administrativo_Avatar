using ADM_Pagos.Entities;

namespace ADM_Pagos.Services
{
    public interface IPagoService
    {
        Task<BusinessLogicResponse> CrearPagoAsync(PagoRequest request, string? token);
        Task<BusinessLogicResponse> ReversarPagoAsync(int idPago, string? token);
        Task<BusinessLogicResponse> ObtenerPagoAsync(int idPago, string? token);
        Task<BusinessLogicResponse> ListadoPorPeriodoAsync(DateTime inicio, DateTime fin, string? token);

    }
}
