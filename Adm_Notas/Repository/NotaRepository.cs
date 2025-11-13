using Adm_Notas.Entities;
using Dapper;

namespace Adm_Notas.Repository
{
    public class NotaRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public NotaRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // =========================================================
        // ASIGNAR O MODIFICAR NOTA DE UN RUBRO
        // =========================================================
        public async Task<int> AsignarNota(NotaRequest nota)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"
                    IF EXISTS (SELECT 1 FROM Nota WHERE ID_Estudiante = @ID_Estudiante AND ID_Rubro = @ID_Rubro)
                        UPDATE Nota 
                        SET Valor_Nota = @Valor_Nota
                        WHERE ID_Estudiante = @ID_Estudiante AND ID_Rubro = @ID_Rubro;
                    ELSE
                        INSERT INTO Nota (ID_Estudiante, ID_Rubro, Valor_Nota)
                        VALUES (@ID_Estudiante, @ID_Rubro, @Valor_Nota);";

                return await connection.ExecuteAsync(sql, new
                {
                    nota.ID_Estudiante,
                    nota.ID_Rubro,
                    nota.Valor_Nota
                });
            }
        }

        // =========================================================
        // OBTENER NOTAS DE UN ESTUDIANTE EN UN CURSO
        // =========================================================
        public async Task<IEnumerable<Nota>> ObtenerNotas(int idEstudiante, int idCurso)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"
                SELECT n.ID_Nota, n.ID_Estudiante, n.ID_Rubro, n.Valor_Nota
                FROM Nota n
                INNER JOIN Rubro r ON n.ID_Rubro = r.ID_Rubro
                INNER JOIN Grupo g ON r.ID_Grupo = g.ID_Grupo
                INNER JOIN Matricula m ON m.ID_Grupo = g.ID_Grupo AND m.ID_Estudiante = n.ID_Estudiante
                WHERE n.ID_Estudiante = @ID_Estudiante AND g.ID_Curso = @ID_Curso;";

                return await connection.QueryAsync<Nota>(sql, new
                {
                    ID_Estudiante = idEstudiante,
                    ID_Curso = idCurso
                });
            }
        }




    }
}
