using Dapper;
using Mod_Academico.Entities;

namespace Mod_Academico.Repository
{
    public class HistorialAcademicoRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public HistorialAcademicoRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }


        // Consulta los promedios por curso para un estudiante con los parametros que solicita la HU
        public async Task<IEnumerable<HistorialAcademico>> ObtenerHistorialAsync(string tipo, string identificacion)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"
            SELECT 
                c.Codigo_Curso,
                c.Nombre_Curso,
                ISNULL(CAST(AVG(n.Valor_Nota) AS DECIMAL(5,2)), 0) AS Promedio
            FROM Estudiante e
            INNER JOIN Matricula m ON e.ID_Estudiante = m.ID_Estudiante
            INNER JOIN Grupo g ON m.ID_Grupo = g.ID_Grupo
            INNER JOIN Curso c ON g.ID_Curso = c.ID_Curso
            LEFT JOIN Rubro r ON g.ID_Grupo = r.ID_Grupo
            LEFT JOIN Nota n ON n.ID_Rubro = r.ID_Rubro AND n.ID_Estudiante = e.ID_Estudiante
            WHERE e.Tipo_Identificacion = @Tipo_Identificacion AND e.Identificacion = @Identificacion
            GROUP BY c.Codigo_Curso, c.Nombre_Curso
            ORDER BY c.Codigo_Curso;
        ";

                return await connection.QueryAsync<HistorialAcademico>(sql, new
                {
                    Tipo_Identificacion = tipo,
                    Identificacion = identificacion
                });
            }
        }



    }
}
