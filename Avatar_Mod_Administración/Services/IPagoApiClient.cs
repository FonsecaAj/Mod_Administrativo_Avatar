using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IPagoApiClient
    {
        Task<(bool ok, int statusCode, string? message, PagoDto? pago)> CrearPagoAsync(int idFactura, string metodo, string token);
        Task<(bool ok, int statusCode, string? message)> ReversarPagoAsync(int idPago, string motivo, string token);
        Task<(bool ok, int statusCode, string? message, PagoDto? pago, IEnumerable<PagoDetalleDto>? detalles)> ObtenerPagoAsync(int idPago, string token);

        Task<(bool ok, int statusCode, string? message, IEnumerable<PagoDto>? pagos)> ListarPagosPorPeriodoAsync(DateTime inicio, DateTime fin, string token);
    
    }
}
