using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface INotaApiClient
    {
        Task<(bool ok, int statusCode, string? message, List<NotaDto>? data)> ObtenerNotas(int idEstudiante, int idCurso);
        Task<(bool ok, int statusCode, string? message)> AsignarNota(NotaRequest request);
    }
}
