using Adm_Notas.Entities;
using Dapper;

namespace Adm_Notas.Repository
{
    public class RubroRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RubroRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }


        // =========================================================
        // OBTENER DESGLOSE
        // =========================================================
        public async Task<IEnumerable<Rubro>> ObtenerDesglose(int idGrupo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Rubro WHERE ID_Grupo = @ID_Grupo;";
                return await connection.QueryAsync<Rubro>(sql, new { ID_Grupo = idGrupo });
            }
        }

        // =========================================================
        // VALIDAR SI EXISTEN NOTAS ASOCIADAS A UN GRUPO
        // =========================================================
        public async Task<bool> ExistenNotasAsociadas(int idGrupo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Nota n
                            INNER JOIN Rubro r ON n.ID_Rubro = r.ID_Rubro
                            WHERE r.ID_Grupo = @ID_Grupo;";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { ID_Grupo = idGrupo });
                return count > 0;
            }
        }

        // =========================================================
        // CARGAR DESGLOSE (elimina el anterior y crea el nuevo)
        // =========================================================
        public async Task<int> CargarDesglose(DesgloseRequest request)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                // Eliminar rubros anteriores
                var sqlDelete = "DELETE FROM Rubro WHERE ID_Grupo = @ID_Grupo;";
                await connection.ExecuteAsync(sqlDelete, new { ID_Grupo = request.ID_Grupo });

                // Insertar nuevos rubros
                var sqlInsert = @"INSERT INTO Rubro (ID_Grupo, Nombre_Rubro, Porcentaje)
                                  VALUES (@ID_Grupo, @Nombre_Rubro, @Porcentaje);";

                int total = 0;
                foreach (var rubro in request.Rubros)
                {
                    await connection.ExecuteAsync(sqlInsert, new
                    {
                        request.ID_Grupo,
                        rubro.Nombre_Rubro,
                        rubro.Porcentaje
                    });
                    total++;
                }

                return total;
            }
        }


    }
}
