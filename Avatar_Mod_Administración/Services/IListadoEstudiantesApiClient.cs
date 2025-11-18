using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IListadoEstudiantesApiClient
    {
            Task<(bool ok, int statusCode, string? message, List<EstudiantesListadoDto>? data)> ObtenerListadoAsync(int periodo, CancellationToken ct = default);
        
    }
}
