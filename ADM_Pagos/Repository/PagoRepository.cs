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

        public async Task<int> ReversarPagoAsync(int idPago, string detalle)
        {
            using var connection = _dbconnection.CreateConnection();

 
            if (connection is SqlConnection sqlConn)
                await sqlConn.OpenAsync();
            else
                connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Obtener la factura asociada
                var idFactura = await connection.ExecuteScalarAsync<int?>(
                    "SELECT ID_Factura FROM Pago WHERE ID_Pago = @ID",
                    new { ID = idPago }, transaction);

                if (idFactura == null)
                    throw new InvalidOperationException($"No existe una factura asociada al pago {idPago}.");

                //  Reversar el pago
                var sqlPago = @"
                    UPDATE Pago 
                    SET Estado = 'Reversado'
                    WHERE ID_Pago = @ID;";
                await connection.ExecuteAsync(sqlPago, new { ID = idPago }, transaction);

                // Actualizar detalle del pago agregando motivo
                var sqlDetalle = @"
                    UPDATE Pago_Detalle 
                    SET Detalle = CONCAT(
                        COALESCE(Descripcion, ''), 
                        CHAR(13) + CHAR(10) + ' [REVERSIÓN] Motivo: ', @Detalle
                    )
                    WHERE ID_Pago = @ID;";
                int filas = await connection.ExecuteAsync(sqlDetalle, new { Detalle = detalle, ID = idPago }, transaction);

                //  Actualizar factura en otra base
                using (var con2 = new SqlConnection(_dbConexionFactura))
                {
                    await con2.OpenAsync();

                    var sqlFactura = @"
                        UPDATE Factura 
                        SET Estado = 'Pendiente'
                        WHERE ID_Factura = @ID_Factura;";
                    await con2.ExecuteAsync(sqlFactura, new { ID_Factura = idFactura });

                    // Agregar motivo en Factura_Detalle también
                    var sqlDetFactura = @"
                        UPDATE Factura_Detalle 
                        SET Detalle = CONCAT(
                            COALESCE(Descripcion, ''), 
                            CHAR(13) + CHAR(10) + ' [REVERSIÓN DE PAGO] Motivo: ', @Detalle
                        )
                        WHERE ID_Factura = @ID_Factura;";
                    await con2.ExecuteAsync(sqlDetFactura, new { ID_Factura = idFactura, Detalle = detalle });
                }

                transaction.Commit();
                return filas;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException("Error al reversar el pago", ex);
            }
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
