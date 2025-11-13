using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IHistorialAcademicoApiClient
    {
        Task<(bool ok, int statusCode, string? message, List<HistorialAcademicoDto>? data)>
    ObtenerHistorialAsync(string tipo, string identificacion, string token, CancellationToken ct = default);

    }

}
