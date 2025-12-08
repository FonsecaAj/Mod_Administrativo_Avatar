using ApiMAT1.Entities;
using Dapper;
using System.Collections.Generic;
using System.Data;

namespace ApiMAT1.Repository
{
    public class PrematriculaRepository : IPrematriculaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PrematriculaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Prematricula> ObtenerTodas()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Prematricula";
            return connection.Query<Prematricula>(sql);
        }

        public Prematricula? ObtenerPorId(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Prematricula WHERE ID_Prematricula = @id";
            return connection.QueryFirstOrDefault<Prematricula>(sql, new { id });
        }

        public void Crear(Prematricula entidad)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"INSERT INTO Prematricula 
                (ID_Estudiante, ID_Carrera, ID_Curso, Observaciones, ID_Periodo)
                VALUES (@ID_Estudiante, @ID_Carrera, @ID_Curso, @Observaciones, @ID_Periodo)";
            connection.Execute(sql, entidad);
        }

        public void Actualizar(Prematricula entidad)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"UPDATE Prematricula
                SET ID_Estudiante = @ID_Estudiante,
                    ID_Carrera = @ID_Carrera,
                    ID_Curso = @ID_Curso,
                    Observaciones = @Observaciones,
                    ID_Periodo = @ID_Periodo
                WHERE ID_Prematricula = @ID_Prematricula";
            connection.Execute(sql, entidad);
        }

        public void Eliminar(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Prematricula WHERE ID_Prematricula = @id";
            connection.Execute(sql, new { id });
        }

        public async Task<IEnumerable<PrematriculaDetallada>> Obtener_Prematri_Estudiante(string identificacion)
        {
            using var connection = _connectionFactory.CreateConnection();

            // La consulta SQL proporcionada (modificada para usar el parámetro @Identificacion)
            const string sql = @"
                SELECT 
                    e.Identificacion,
                    e.Nombre + ' ' + e.Apellido1 + ' ' + e.Apellido2 AS Nombre_Completo,
                    e.Carrera AS Carrera_Estudiante,
                    c.Nombre_Carrera,
                    cur.Nombre_Curso,
                    cur.Codigo_Curso,
                    pm.Observaciones,
                    per.Año,
                    per.Numero_Periodo,
                    per.Fecha_Inicio,
                    per.Fecha_Fin
                FROM Estudiante e
                JOIN Prematricula pm ON e.ID_Estudiante = pm.ID_Estudiante
                LEFT JOIN [User_ApisG].[Carrera] c ON pm.ID_Carrera = c.ID_Carrera
                LEFT JOIN Curso cur ON pm.ID_Curso = cur.ID_Curso
                LEFT JOIN Periodo per ON pm.ID_Periodo = per.ID_Periodo
                WHERE e.Identificacion = @Identificacion
                ORDER BY per.Año DESC, per.Numero_Periodo DESC, pm.ID_Prematricula;";

            // Dapper mapeará los resultados directamente a la clase PrematriculaDetallada
            var result = await connection.QueryAsync<PrematriculaDetallada>(sql, new { Identificacion = identificacion });

            return result;
        }
    }
}
