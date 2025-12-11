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
            string sql = @"
        SELECT 
            e.Numero_Identificacion,
            e.Tipo_Identificacion,
            e.Email,
            e.Nombre_Completo,
            e.Fecha_Nacimiento,
            e.ID_Provincia,
            e.ID_Canton,
            e.DistritoID,
            e.Telefonos,
            p.Nombre_Provincia AS Nombre_Provincia,
            c.Nombre_Canton    AS Nombre_Canton,
            d.Nombre_Distrito  AS Nombre_Distrito
        FROM Expediente e
        INNER JOIN Provincia p ON e.ID_Provincia = p.ID_Provincia
        INNER JOIN Canton c    ON e.ID_Canton     = c.ID_Canton
        INNER JOIN Distrito d  ON e.DistritoID    = d.ID_Distrito";

            return conexion.Query<Expediente>(sql);
        }

        public Expediente ObtenerPorId(string numeroIdentificacion)
        {
            using var conexion = _connectionFactory.CreateConnection();
            string sql = @"
        SELECT 
            e.Numero_Identificacion,
            e.Tipo_Identificacion,
            e.Email,
            e.Nombre_Completo,
            e.Fecha_Nacimiento,
            e.ID_Provincia,
            e.ID_Canton,
            e.DistritoID,
            e.Telefonos,
            p.Nombre_Provincia AS Nombre_Provincia,
            c.Nombre_Canton    AS Nombre_Canton,
            d.Nombre_Distrito  AS Nombre_Distrito
        FROM Expediente e
        INNER JOIN Provincia p ON e.ID_Provincia = p.ID_Provincia
        INNER JOIN Canton c    ON e.ID_Canton     = c.ID_Canton
        INNER JOIN Distrito d  ON e.DistritoID    = d.ID_Distrito
        WHERE e.Numero_Identificacion = @Numero_Identificacion";

            return conexion.QueryFirstOrDefault<Expediente>(
                sql,
                new { Numero_Identificacion = numeroIdentificacion });
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
