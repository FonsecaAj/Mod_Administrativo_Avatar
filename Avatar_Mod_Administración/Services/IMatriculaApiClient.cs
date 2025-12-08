using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{
    public interface IMatriculaApiClient
    {
        Task<IEnumerable<MatriculaDto>> ObtenerPorCursoYGrupoAsync(int idCurso, int idGrupo);

        Task<(bool ok, int statusCode, string message)> CrearAsync(MatriculaRequestDto request);
        Task<(bool ok, int statusCode, string message)> ActualizarAsync(MatriculaRequestDto request);
        Task<(bool ok, int statusCode, string message)> EliminarAsync(int id);

    }
}
