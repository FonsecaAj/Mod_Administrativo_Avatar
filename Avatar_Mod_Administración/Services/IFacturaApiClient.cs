using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IFacturaApiClient
    {
        Task<(bool ok, int statusCode, string? message, int? idFactura)> CrearFacturaAsync(string identificacion, string token, CancellationToken ct = default);
        Task<(bool ok, int statusCode, string? message)>
            ReversarFacturaAsync(int idFactura, string motivo, string token, CancellationToken ct = default);
        Task<(bool ok, int statusCode, string? message, FacturaDto? factura)> ObtenerFacturaAsync(int idFactura, string token, CancellationToken ct = default);

        
        Task<(bool ok, int statusCode, string? message, List<FacturaDto>? facturas)>
            ListarFacturasAsync(DateTime inicio, DateTime fin, string? estado, string token, CancellationToken ct = default);

    }
}
