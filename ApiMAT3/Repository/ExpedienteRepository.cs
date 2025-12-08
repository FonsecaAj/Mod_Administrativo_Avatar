using ApiMAT3.Entities;
using Dapper;
using System.Collections.Generic;
using System.Data;

namespace ApiMAT3.Repository
{
    public class ExpedienteRepository : IExpedienteRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ExpedienteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Expediente> ObtenerTodos()
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = "SELECT * FROM Expediente";
            return conexion.Query<Expediente>(sql);
        }

        public Expediente ObtenerPorId(string numeroIdentificacion)
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = "SELECT * FROM Expediente WHERE Numero_Identificacion = @Numero_Identificacion";
            return conexion.QueryFirstOrDefault<Expediente>(sql, new { Numero_Identificacion = numeroIdentificacion });
        }

        public void Crear(Expediente expediente)
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = @"
                INSERT INTO Expediente 
                (Numero_Identificacion, Tipo_Identificacion, Email, Nombre_Completo, Fecha_Nacimiento, ID_Provincia, ID_Canton, DistritoID, Telefonos)
                VALUES (@Numero_Identificacion, @Tipo_Identificacion, @Email, @Nombre_Completo, @Fecha_Nacimiento, @ID_Provincia, @ID_Canton, @DistritoID, @Telefonos)";
            conexion.Execute(sql, expediente);
        }

        public void Actualizar(Expediente expediente)
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = @"
                UPDATE Expediente SET
                    Tipo_Identificacion = @Tipo_Identificacion,
                    Email = @Email,
                    Nombre_Completo = @Nombre_Completo,
                    Fecha_Nacimiento = @Fecha_Nacimiento,
                    ID_Provincia = @ID_Provincia,
                    ID_Canton = @ID_Canton,
                    DistritoID = @DistritoID,
                    Telefonos = @Telefonos
                WHERE Numero_Identificacion = @Numero_Identificacion";
            conexion.Execute(sql, expediente);
        }

        public void Eliminar(string numeroIdentificacion)
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = "DELETE FROM Expediente WHERE Numero_Identificacion = @Numero_Identificacion";
            conexion.Execute(sql, new { Numero_Identificacion = numeroIdentificacion });
        }
    }
}
