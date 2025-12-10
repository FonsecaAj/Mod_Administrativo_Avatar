using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IUbicacionesApiClient
    {
        Task<IEnumerable<ProvinciaDto>> ObtenerProvinciasAsync();
        Task<IEnumerable<CantonDto>> ObtenerCantonesAsync(int idProvincia);
        Task<IEnumerable<DistritoDto>> ObtenerDistritosAsync(int idProvincia, int idCanton);
    }
}
