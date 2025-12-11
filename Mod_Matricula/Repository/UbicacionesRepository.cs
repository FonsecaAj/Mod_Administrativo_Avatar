using Dapper;
using Mod_Matricula.Entities;

namespace Mod_Matricula.Repository
{
    public class UbicacionesRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;


        public UbicacionesRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Provincia>> Obtener_Provincias()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Provincia>("SELECT * FROM Provincia;");
            }
        }

        public async Task<IEnumerable<Canton>> Obtener_Cantones(int idProvincia)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Canton WHERE ID_Provincia = @ID_Provincia";
                return await connection.QueryAsync<Canton>(sql, new { ID_Provincia = idProvincia });
            }
        }

        public async Task<IEnumerable<Distrito>> Obtener_Distritos(int idProvincia, int idCanton)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT * FROM Distrito 
                    WHERE ID_Provincia = @ID_Provincia 
                    AND ID_Canton = @ID_Canton";

                return await connection.QueryAsync<Distrito>(sql, new
                {
                    ID_Provincia = idProvincia,
                    ID_Canton = idCanton
                });
            }
        }
    }
}
