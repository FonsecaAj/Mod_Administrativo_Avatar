using Avatar_Mod_Administración.Entities;
using System.Net;

namespace Avatar_Mod_Administración.Services
{
    public interface IRubroApiClient
    {

        Task<(bool ok, int statusCode, string? message)> CargarDesglose(DesgloseRequest request, CancellationToken ct = default);

        Task<(bool ok, int statusCode, string? message, List<RubroDto>? data)> ObtenerDesglose(int idGrupo, CancellationToken ct = default);


    }
}
