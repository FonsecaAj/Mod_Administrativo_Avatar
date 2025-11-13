using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IFacturaApiClient
    {
            Task<(bool ok, int statusCode, string? message, int? idFactura)> CrearFacturaAsync(string identificacion, CancellationToken ct = default);
            Task<(bool ok, int statusCode, string? message)> ReversarFacturaAsync(int idFactura, CancellationToken ct = default);
            Task<(bool ok, int statusCode, string? message, FacturaDto? factura)> ObtenerFacturaAsync(int idFactura, CancellationToken ct = default);
        

    }
}
