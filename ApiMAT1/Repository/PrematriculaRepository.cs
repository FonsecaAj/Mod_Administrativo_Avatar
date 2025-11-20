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

        // ============================
        //   OBTENER TODAS (CON JOINS)
        // ============================
        public IEnumerable<dynamic> ObtenerTodas()
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT 
                    p.ID_Prematricula,
                    p.ID_Estudiante,
                    p.ID_Carrera,
                    p.ID_Curso,
                    p.ID_Periodo,
                    p.Observaciones,

                    -- NOMBRE COMPLETO DEL ESTUDIANTE
                    (e.Nombre + ' ' + e.Apellido1 + ' ' + e.Apellido2) AS NombreEstudiante,

                    -- CARRERA REAL (User_ApisG.Carrera)
                    c.Nombre_Carrera AS NombreCarrera,

                    -- CURSO REAL (Nombre_Curso)
                    cu.Nombre_Curso AS NombreCurso,

                    -- PERIODO REAL (Año / Numero_Periodo)
                    CONCAT(pe.Año, '-', pe.Numero_Periodo) AS NombrePeriodo

                FROM Prematricula p
                INNER JOIN Estudiante e 
                    ON e.ID_Estudiante = p.ID_Estudiante
                INNER JOIN User_ApisG.Carrera c 
                    ON c.ID_Carrera = p.ID_Carrera
                INNER JOIN Curso cu 
                    ON cu.ID_Curso = p.ID_Curso
                INNER JOIN Periodo pe 
                    ON pe.ID_Periodo = p.ID_Periodo;
            ";

            return connection.Query<dynamic>(sql);
        }

        // ============================
        //   OBTENER POR ID (SIN JOIN)
        // ============================
        public Prematricula? ObtenerPorId(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * 
                FROM Prematricula 
                WHERE ID_Prematricula = @id";

            return connection.QueryFirstOrDefault<Prematricula>(sql, new { id });
        }

        // ============================
        //   CREAR
        // ============================
        public void Crear(Prematricula entidad)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO Prematricula 
                    (ID_Estudiante, ID_Carrera, ID_Curso, Observaciones, ID_Periodo)
                VALUES 
                    (@ID_Estudiante, @ID_Carrera, @ID_Curso, @Observaciones, @ID_Periodo);";

            connection.Execute(sql, entidad);
        }

        // ============================
        //   ACTUALIZAR
        // ============================
        public void Actualizar(Prematricula entidad)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                UPDATE Prematricula
                SET 
                    ID_Estudiante = @ID_Estudiante,
                    ID_Carrera = @ID_Carrera,
                    ID_Curso = @ID_Curso,
                    Observaciones = @Observaciones,
                    ID_Periodo = @ID_Periodo
                WHERE 
                    ID_Prematricula = @ID_Prematricula;";

            connection.Execute(sql, entidad);
        }

        // ============================
        //   ELIMINAR
        // ============================
        public void Eliminar(int id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                DELETE FROM Prematricula 
                WHERE ID_Prematricula = @id;";

            connection.Execute(sql, new { id });
        }
    }
}
