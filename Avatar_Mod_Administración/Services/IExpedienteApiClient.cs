using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IExpedienteApiClient
    {
        // CRUD expedientes
        Task<IEnumerable<ExpedienteDto>> ObtenerTodosAsync();
        Task<ExpedienteDto?> ObtenerPorIdAsync(string numeroIdentificacion);
        Task<(bool ok, int statusCode, string message)> CrearAsync(ExpedienteDto expediente);
        Task<(bool ok, int statusCode, string message)> ActualizarAsync(ExpedienteDto expediente);
        Task<(bool ok, int statusCode, string message)> EliminarAsync(string numeroIdentificacion);

        // Catálogos de dirección
        Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync();
        Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia);
        Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton);
    }
}
