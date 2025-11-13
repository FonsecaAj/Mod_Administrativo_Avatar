
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiACD6.Entities;


namespace ApiACD6.Repository
{
    public class ProfesorRepository : IProfesorRepository
    {
        private readonly IConfiguration _configuration;

        public ProfesorRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection =>
            new SqlConnection(_configuration.GetConnectionString("ConexionSQL"));

        public IEnumerable<Profesor> ObtenerTodos()
        {
            using var db = Connection;
            return db.Query<Profesor>("SELECT * FROM PROFESOR");
        }

        public Profesor ObtenerPorId(int idProfesor)
        {
            using var db = Connection;
            return db.QueryFirstOrDefault<Profesor>(
                "SELECT * FROM PROFESOR WHERE ID_Profesor = @ID_Profesor",
                new { ID_Profesor = idProfesor });
        }

        public void Crear(Profesor profesor)
        {
            using var db = Connection;
            var sql = @"INSERT INTO PROFESOR 
                        (Numero_Identificacion, Tipo_Identificacion, Email, Nombre_Completo, Fecha_Nacimiento, Telefono)
                        VALUES (@Numero_Identificacion, @Tipo_Identificacion, @Email, @Nombre_Completo, @Fecha_Nacimiento, @Telefono)";
            db.Execute(sql, profesor);
        }

        public void Actualizar(Profesor profesor)
        {
            using var db = Connection;
            var sql = @"UPDATE PROFESOR SET
                        Numero_Identificacion = @Numero_Identificacion,
                        Tipo_Identificacion = @Tipo_Identificacion,
                        Email = @Email,
                        Nombre_Completo = @Nombre_Completo,
                        Fecha_Nacimiento = @Fecha_Nacimiento,
                        Telefono = @Telefono
                        WHERE ID_Profesor = @ID_Profesor";
            db.Execute(sql, profesor);
        }

        public void Eliminar(int idProfesor)
        {
            using var db = Connection;
            db.Execute("DELETE FROM PROFESOR WHERE ID_Profesor = @ID_Profesor", new { ID_Profesor = idProfesor });
        }
    }
}
