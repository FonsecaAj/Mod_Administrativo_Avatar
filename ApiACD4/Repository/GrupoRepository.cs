using ApiACD4.Entities;
using Dapper;
using System.Data;

namespace ApiACD4.Repository
{
    public class GrupoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public GrupoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

 
        public async Task<IEnumerable<Grupo>> ObtenerTodos()
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Grupo";
            var result = await db.QueryAsync<Grupo>(sql);
            return result.ToList();
        }

       
        public async Task<Grupo?> ObtenerPorId(int id)
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Grupo WHERE ID_Grupo = @ID_Grupo";
            return await db.QueryFirstOrDefaultAsync<Grupo>(sql, new { ID_Grupo = id });
        }

     
        public async Task<IEnumerable<Grupo>> ObtenerPorCurso(int idCurso)
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Grupo WHERE ID_Curso = @ID_Curso";
            var result = await db.QueryAsync<Grupo>(sql, new { ID_Curso = idCurso });
            return result.ToList();
        }

        public async Task<int> Crear(Grupo grupo)
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Grupo (Numero_Grupo, ID_Curso, ID_Profesor, Horario, ID_Periodo)
                VALUES (@Numero_Grupo, @ID_Curso, @ID_Profesor, @Horario, @ID_Periodo);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await db.ExecuteScalarAsync<int>(sql, grupo);
            return id;
        }

      
        public async Task<int> Modificar(Grupo grupo)
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Grupo
                SET Numero_Grupo = @Numero_Grupo,
                    ID_Curso = @ID_Curso,
                    ID_Profesor = @ID_Profesor,
                    Horario = @Horario,
                    ID_Periodo = @ID_Periodo
                WHERE ID_Grupo = @ID_Grupo";

            var filas = await db.ExecuteAsync(sql, grupo);
            return filas;
        }


        public async Task<int> Eliminar(int id)
        {
            using var db = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Grupo WHERE ID_Grupo = @ID_Grupo";
            var filas = await db.ExecuteAsync(sql, new { ID_Grupo = id });
            return filas;
        }
    }
}
