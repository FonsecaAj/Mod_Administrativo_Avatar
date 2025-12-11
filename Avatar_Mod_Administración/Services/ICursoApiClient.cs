using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Services
{ 
    public interface ICursoApiClient
        {
            Task<IEnumerable<CursoDto>> ObtenerTodosAsync();
            Task<IEnumerable<CursoDto>> ObtenerPorCarreraAsync(int idCarrera);
            Task<CursoDto?> ObtenerPorIdAsync(int id);

            // → Devuelven ok, statusCode, message
            Task<(bool ok, int statusCode, string message)> CrearAsync(CursoDto curso);
            Task<(bool ok, int statusCode, string message)> ActualizarAsync(CursoDto curso);
            Task<(bool ok, int statusCode, string message)> EliminarAsync(int id);

            Task<LookupsResponse?> ObtenerLookupsAsync();
        }

    }
