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

        public IEnumerable<MatriculaListadoDto> ObtenerPorCursoYGrupo(int idCurso, int idGrupo)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
        SELECT 
            E.Identificacion,
            CONCAT(E.Nombre, ' ', E.Apellido1, ' ', E.Apellido2) AS NombreCompleto,
            C.Nombre_Curso AS NombreCurso,
            G.Nombre_Grupo AS NombreGrupo,
            M.Fecha_Matricula
        FROM Matricula M
        INNER JOIN Estudiante E ON M.ID_Estudiante = E.ID_Estudiante
        INNER JOIN Grupo G ON M.ID_Grupo = G.ID_Grupo
        INNER JOIN Curso C ON G.ID_Curso = C.ID_Curso
        WHERE G.ID_Curso = @idCurso AND M.ID_Grupo = @idGrupo";

            return connection.Query<MatriculaListadoDto>(sql, new { idCurso, idGrupo });
        }

        public IEnumerable<PeriodoMatriculaDto> ObtenerPeriodos()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT ID_Periodo, Año, Numero_Periodo, Fecha_Inicio, Fecha_Fin
                FROM Periodo";
            return connection.Query<PeriodoMatriculaDto>(sql);
        }

        public IEnumerable<CursoMatriculaDto> ObtenerCursos()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT ID_Curso, Nombre_Curso, Codigo_Curso
                FROM Curso";
            return connection.Query<CursoMatriculaDto>(sql);
        }

        public IEnumerable<GrupoMatriculaDto> ObtenerGrupos()
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        SELECT 
            g.ID_Grupo,
            g.ID_Curso,
            g.Nombre_Grupo,
            c.Nombre_Curso,
            g.Cupo_Maximo,
            g.Cupo_Maximo - COUNT(m.ID_Matricula) AS Cupo_Disponible
        FROM Grupo g
        LEFT JOIN Matricula m ON g.ID_Grupo = m.ID_Grupo
        INNER JOIN Curso c ON g.ID_Curso = c.ID_Curso
        GROUP BY g.ID_Grupo, g.ID_Curso, g.Nombre_Grupo, c.Nombre_Curso, g.Cupo_Maximo";

            return connection.Query<GrupoMatriculaDto>(sql);
        }

        public bool ExisteMatricula(int idEstudiante, int idCurso, int idGrupo)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        SELECT COUNT(*)
        FROM Matricula M
        INNER JOIN Grupo G ON M.ID_Grupo = G.ID_Grupo
        WHERE M.ID_Estudiante = @idEstudiante
          AND M.ID_Grupo      = @idGrupo
          AND G.ID_Curso      = @idCurso";

            var count = connection.ExecuteScalar<int>(sql, new
            {
                idEstudiante,
                idCurso,
                idGrupo
            });

            return count > 0;
        }

        public int ObtenerCupoDisponible(int idGrupo)
{
    using var connection = _connectionFactory.CreateConnection();

    var sql = @"
        SELECT g.Cupo_Maximo - COUNT(m.ID_Matricula) AS CupoDisponible
        FROM Grupo g
        LEFT JOIN Matricula m ON g.ID_Grupo = m.ID_Grupo
        WHERE g.ID_Grupo = @idGrupo
        GROUP BY g.Cupo_Maximo";

    var result = connection.ExecuteScalar<int?>(sql, new { idGrupo });
    return result ?? 0;
}


        public async Task<IEnumerable<MatriculaEstudianteDto>> Obtener_Matricula_Estudiante(string identificacion)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        SELECT 
            -- Información del estudiante
            e.Carrera AS Carrera_Estudiante,
            
            -- Información de matrícula
            m.Fecha_Matricula,
            
            -- Información del grupo y curso (Verificación de existencia)
            g.Nombre_Grupo,
            c.Nombre_Curso,
            c.Codigo_Curso,
            
            -- Información del período académico actual (Subconsulta escalar)
            (SELECT TOP 1 CONCAT('Año ', p.Año, ' - Periodo ', p.Numero_Periodo)
             FROM Periodo p 
             WHERE GETDATE() BETWEEN p.Fecha_Inicio AND p.Fecha_Fin
             ORDER BY p.Fecha_Inicio DESC) AS Periodo_Actual
            
        FROM Estudiante e
        JOIN Matricula m ON e.ID_Estudiante = m.ID_Estudiante
        JOIN Grupo g ON m.ID_Grupo = g.ID_Grupo
        JOIN Curso c ON g.ID_Curso = c.ID_Curso
        WHERE e.Identificacion = @Identificacion 
        ORDER BY m.Fecha_Matricula DESC, c.Nombre_Curso;";

            return await connection.QueryAsync<MatriculaEstudianteDto>(sql, new
            {
                Identificacion = identificacion
            });
        }



    }






}
