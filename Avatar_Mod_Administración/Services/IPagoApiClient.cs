using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IPagoApiClient
    {
        Task<(bool ok, int statusCode, string? message, PagoDto? pago)> CrearPagoAsync(int idFactura, string metodo);
        Task<(bool ok, int statusCode, string? message)> ReversarPagoAsync(int idPago);
        Task<(bool ok, int statusCode, string? message, PagoDto? pago)> ObtenerPagoAsync(int idPago);
    }
}
