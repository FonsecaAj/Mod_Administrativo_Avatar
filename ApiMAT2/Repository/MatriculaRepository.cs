using ApiMAT2.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiMAT2.Repository
{
    public class MatriculaRepository : IMatriculaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MatriculaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Crear(Matricula matricula)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO Matricula (ID_Estudiante, ID_Grupo, Fecha_Matricula)
                        VALUES (@ID_Estudiante, @ID_Grupo, @Fecha_Matricula)";
            connection.Execute(sql, matricula);
        }

        public void Actualizar(Matricula matricula)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE Matricula
                        SET ID_Grupo = @ID_Grupo, Fecha_Matricula = @Fecha_Matricula
                        WHERE ID_Estudiante = @ID_Estudiante";
            connection.Execute(sql, matricula);
        }

        public void Eliminar(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "DELETE FROM Matricula WHERE ID_Matricula = @id";
            connection.Execute(sql, new { id });
        }

        public int? ObtenerIdEstudiantePorIdentificacion(string identificacion)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT ID_Estudiante FROM Estudiante WHERE Identificacion = @identificacion";
            return connection.QueryFirstOrDefault<int?>(sql, new { identificacion });
        }

        public bool PeriodoEsActivo(int idPeriodo)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT CASE 
                        WHEN Fecha_Inicio <= GETDATE() AND Fecha_Fin >= GETDATE() THEN 1 
                        ELSE 0 END 
                        FROM Periodo WHERE ID_Periodo = @idPeriodo";
            return connection.ExecuteScalar<int>(sql, new { idPeriodo }) == 1;
        }

        public bool GrupoPerteneceACurso(int idGrupo, int idCurso)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(*) 
                        FROM Grupo 
                        WHERE ID_Grupo = @idGrupo AND ID_Curso = @idCurso";
            return connection.ExecuteScalar<int>(sql, new { idGrupo, idCurso }) > 0;
        }

        public IEnumerable<object> ObtenerPorCursoYGrupo(int idCurso, int idGrupo)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    E.Identificacion,
                    CONCAT(E.Nombre, ' ', E.Apellido1, ' ', E.Apellido2) AS NombreCompleto,
                    C.Nombre_Curso,
                    G.Nombre_Grupo,
                    M.Fecha_Matricula
                FROM Matricula M
                INNER JOIN Estudiante E ON M.ID_Estudiante = E.ID_Estudiante
                INNER JOIN Grupo G ON M.ID_Grupo = G.ID_Grupo
                INNER JOIN Curso C ON G.ID_Curso = C.ID_Curso
                WHERE G.ID_Curso = @idCurso AND M.ID_Grupo = @idGrupo";

            return connection.Query<object>(sql, new { idCurso, idGrupo });
        }
    }
}
