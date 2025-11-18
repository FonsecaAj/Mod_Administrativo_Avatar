using ADM_Pagos.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ADM_Pagos.Repository
{
    public class PagoRepository
    {
        private readonly IDbConnectionFactory _dbconnection;
        private readonly string _dbConexionFactura;

        public PagoRepository(IDbConnectionFactory dbconnection, IConfiguration config)
        {
            _dbconnection = dbconnection;
            _dbConexionFactura = config["ConnectionStrings:FacturasConnection"]
                ?? throw new InvalidOperationException("BD FACTURAS ERRONEA");
        }

        // Crear pago + detalle ("Servicios estudiantiles")
        public async Task<int> CrearPagoAsync(int idFactura, decimal monto, string metodo)
        {
            using var conn = _dbconnection.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var sqlPago = @"
INSERT INTO Pago (ID_Factura, Fecha_Pago, Monto_Pago, Metodo_Pago, Estado)
VALUES (@ID_Factura, GETDATE(), @Monto, @Metodo, 'Completado');
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var idPago = await conn.ExecuteScalarAsync<int>(
                    sqlPago, new { ID_Factura = idFactura, Monto = monto, Metodo = metodo }, tx);

                var sqlDet = @"
INSERT INTO Pago_Detalle (ID_Pago, Descripcion)
VALUES (@ID_Pago, @Descripcion);";

                await conn.ExecuteAsync(sqlDet, new
                {
                    ID_Pago = idPago,
                    Descripcion = "Servicios estudiantiles"
                }, tx);

                tx.Commit();

                // 🔹 Actualizar el estado de la factura en la otra base
                using (var con2 = new SqlConnection(_dbConexionFactura))
                {
                    await con2.OpenAsync();
                    var sqlActualizarFactura = @"
UPDATE Factura SET Estado = 'Pagada'
WHERE ID_Factura = @ID_Factura";
                    await con2.ExecuteAsync(sqlActualizarFactura, new { ID_Factura = idFactura });
                }

                return idPago;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<int> ReversarPagoAsync(int idPago)
        {
            using var conn = _dbconnection.CreateConnection();
            conn.Open();

            // Obtener el ID_Factura asociado a ese pago
            var sqlSelect = @"SELECT ID_Factura FROM Pago WHERE ID_Pago = @ID;";
            var idFactura = await conn.ExecuteScalarAsync<int?>(sqlSelect, new { ID = idPago });

            if (idFactura == null)
                throw new InvalidOperationException($"No se encontró una factura asociada al pago {idPago}.");

            //Reversar el pago
            var sqlUpdatePago = @"UPDATE Pago SET Estado = 'Reversado' WHERE ID_Pago = @ID;";
            var filasAfectadas = await conn.ExecuteAsync(sqlUpdatePago, new { ID = idPago });

            // Actualizar la factura en la otra base (Modulo_Matricula)
            using (var con2 = new SqlConnection(_dbConexionFactura))
            {
                await con2.OpenAsync();
                var sqlActualizarFactura = @"
UPDATE Factura SET Estado = 'Pendiente'
WHERE ID_Factura = @ID_Factura";
                await con2.ExecuteAsync(sqlActualizarFactura, new { ID_Factura = idFactura });
            }

            return filasAfectadas;
        }

        public async Task<Pago?> ObtenerPagoAsync(int idPago)
        {
            using var conn = _dbconnection.CreateConnection();
            var sql = @"SELECT * FROM Pago WHERE ID_Pago = @ID;";
            return await conn.QuerySingleOrDefaultAsync<Pago>(sql, new { ID = idPago });
        }

        public async Task<IEnumerable<PagoDetalle>> ObtenerDetallesAsync(int idPago)
        {
            using var conn = _dbconnection.CreateConnection();
            var sql = @"SELECT * FROM Pago_Detalle WHERE ID_Pago = @ID_Pago;";
            return await conn.QueryAsync<PagoDetalle>(sql, new { ID_Pago = idPago });
        }

        public async Task<IEnumerable<Pago>> ListadoPorPeriodoAsync(DateTime inicio, DateTime fin)
        {
            using var conn = _dbconnection.CreateConnection();
            var sql = @"
SELECT * FROM Pago 
WHERE Fecha_Pago BETWEEN @Inicio AND @Fin
ORDER BY Fecha_Pago DESC;";
            return await conn.QueryAsync<Pago>(sql, new { Inicio = inicio, Fin = fin });
        }
    }
}
