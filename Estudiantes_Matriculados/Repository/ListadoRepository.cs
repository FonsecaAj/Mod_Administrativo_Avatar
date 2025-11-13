using Dapper;
using Estudiantes_Matriculados.Entities;

namespace Estudiantes_Matriculados.Repository
{
    public class ListadoRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ListadoRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<EstudiantesListado>> ObtenerListadoEstudiantesPorPeriodo(int idPeriodo)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT 
                    e.ID_Estudiante,
                    e.Tipo_Identificacion,
                    e.Identificacion,
                    CONCAT(e.Nombre, ' ', e.Apellido1, ' ', e.Apellido2) AS Nombre_Completo,
                    e.Carrera,
                    c.Nombre_Curso AS Curso,
                    g.Nombre_Grupo AS Grupo
                FROM Matricula m
                INNER JOIN Estudiante e ON m.ID_Estudiante = e.ID_Estudiante
                INNER JOIN Grupo g ON m.ID_Grupo = g.ID_Grupo
                INNER JOIN Curso c ON g.ID_Curso = c.ID_Curso
                INNER JOIN Prematricula p ON p.ID_Estudiante = e.ID_Estudiante 
                                          AND p.ID_Curso = c.ID_Curso 
                                          AND p.ID_Periodo = @ID_Periodo
                ORDER BY e.Apellido1, e.Apellido2, e.Nombre;";

            return await connection.QueryAsync<EstudiantesListado>(sql, new { ID_Periodo = idPeriodo });
        }

    }
}
