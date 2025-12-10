using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IExpedienteApiClient
    {
        Task<IEnumerable<ExpedienteDto>> ObtenerTodosAsync();
        Task<ExpedienteDto?> ObtenerPorIdAsync(string numeroIdentificacion);

        Task<(bool ok, int statusCode, string message)> CrearAsync(ExpedienteRequestDto expediente);
        Task<(bool ok, int statusCode, string message)> ActualizarAsync(ExpedienteRequestDto expediente);
        Task<(bool ok, int statusCode, string message)> EliminarAsync(string numeroIdentificacion);
    }
}
