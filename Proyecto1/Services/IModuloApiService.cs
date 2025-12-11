using USR2.Entities;

namespace USR2.Services
{
    public interface IModuloApiService
    {
        Task<ModuloDto?> ObtenerPorIdAsync(int idModulo, string? token = null);
    }
}