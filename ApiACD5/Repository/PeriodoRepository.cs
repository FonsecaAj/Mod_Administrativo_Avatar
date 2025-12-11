using ApiACD5.Entities;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ApiACD5.Repository
{
    public class PeriodoRepository : IPeriodoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PeriodoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Periodo> ObtenerTodos()
        {
            using var db = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM Periodo";
            return db.Query<Periodo>(sql).ToList();
        }

        public Periodo? ObtenerPorId(int id)
        {
            using var db = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM Periodo WHERE ID_Periodo = @ID_Periodo";
            return db.QueryFirstOrDefault<Periodo>(sql, new { ID_Periodo = id });
        }

        public int Crear(Periodo periodo)
        {
            using var db = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Periodo (Año, Numero_Periodo, Fecha_Inicio, Fecha_Fin)
                VALUES (@Año, @Numero_Periodo, @Fecha_Inicio, @Fecha_Fin);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return db.ExecuteScalar<int>(sql, periodo);
        }

        public int Modificar(Periodo periodo)
        {
            using var db = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Periodo
                SET Año = @Año,
                    Numero_Periodo = @Numero_Periodo,
                    Fecha_Inicio = @Fecha_Inicio,
                    Fecha_Fin = @Fecha_Fin
                WHERE ID_Periodo = @ID_Periodo";
            return db.Execute(sql, periodo);
        }

        public int Eliminar(int id)
        {
            using var db = _connectionFactory.CreateConnection();
            var sql = "DELETE FROM Periodo WHERE ID_Periodo = @ID_Periodo";
            return db.Execute(sql, new { ID_Periodo = id });
        }
    }
}
