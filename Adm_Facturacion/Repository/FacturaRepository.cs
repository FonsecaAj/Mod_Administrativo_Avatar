using Adm_Facturacion.Entities;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Adm_Facturacion.Repository
{
    public class FacturaRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public FacturaRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Crear factura (encabezado + detalle)
        public async Task<int> CrearFacturaAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            decimal montoBase = 30000;
            decimal impuesto = montoBase * 0.02m;
            decimal total = montoBase + impuesto;

            var sqlFactura = @"
                INSERT INTO Factura (Identificacion_Estudiante, Fecha_Emision, Monto_Base, Impuesto, Total, Estado)
                VALUES (@Identificacion, GETDATE(), @Monto_Base, @Impuesto, @Total, 'Pendiente');
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var idFactura = await connection.ExecuteScalarAsync<int>(sqlFactura, new
            {
                Identificacion = identificacion,
                Monto_Base = montoBase,
                Impuesto = impuesto,
                Total = total
            });

            var sqlDetalle = @"
                INSERT INTO Factura_Detalle (ID_Factura, Descripcion, Monto)
                VALUES (@ID_Factura, 'Servicios estudiantiles', @Monto);";

            await connection.ExecuteAsync(sqlDetalle, new { ID_Factura = idFactura, Monto = montoBase });

            return idFactura;
        }

        // Reversar factura
        public async Task<int> ReversarFacturaAsync(int idFactura, string detalle)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Abrimos la conexión antes de usarla
            if (connection is Microsoft.Data.SqlClient.SqlConnection sqlConn)
                await sqlConn.OpenAsync();
            else
                connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlFactura = @"
            UPDATE Factura 
            SET Estado = 'Anulada'
            WHERE ID_Factura = @ID;";
                await connection.ExecuteAsync(sqlFactura, new { ID = idFactura }, transaction);

                var sqlDetalle = @"
            UPDATE Factura_Detalle 
            SET Detalle = CONCAT(
                COALESCE(Descripcion, ''), 
                CHAR(13) + CHAR(10) + ' [REVERSIÓN] Motivo: ', @Detalle
            )
            WHERE ID_Factura = @ID;";
                int filas = await connection.ExecuteAsync(sqlDetalle, new { Detalle = detalle, ID = idFactura }, transaction);

                transaction.Commit();
                return filas;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException("Error al reversar la factura", ex);
            }
        }


        // Obtener factura individual
        public async Task<Factura?> ObtenerFacturaAsync(int idFactura)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Consulta el encabezado + detalle
            var sql = @"
        SELECT 
            f.ID_Factura, f.Identificacion_Estudiante, f.Fecha_Emision, 
            f.Monto_Base, f.Impuesto, f.Total, f.Estado,
            d.ID_Detalle, d.Descripcion, d.Monto
        FROM Factura f
        LEFT JOIN Factura_Detalle d ON f.ID_Factura = d.ID_Factura
        WHERE f.ID_Factura = @ID;";

            // Diccionario para mapear encabezado → detalle
            var facturaDict = new Dictionary<int, Factura>();

            var list = await connection.QueryAsync<Factura, FacturaDetalle, Factura>(
                sql,
                (factura, detalle) =>
                {
                    if (!facturaDict.TryGetValue(factura.ID_Factura, out var currentFactura))
                    {
                        currentFactura = factura;
                        currentFactura.Detalles = new List<FacturaDetalle>();
                        facturaDict.Add(currentFactura.ID_Factura, currentFactura);
                    }

                    if (detalle != null)
                        currentFactura.Detalles.Add(detalle);

                    return currentFactura;
                },
                new { ID = idFactura },
                splitOn: "ID_Detalle"
            );

            return facturaDict.Values.FirstOrDefault();
        }

        // Listado por periodo (filtrando por fecha)
        public async Task<IEnumerable<Factura>> ObtenerFacturasPorPeriodoAsync(DateTime inicio, DateTime fin)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Trae todas las facturas y sus detalles del periodo
            var sql = @"
        SELECT 
            f.ID_Factura, f.Identificacion_Estudiante, f.Fecha_Emision, 
            f.Monto_Base, f.Impuesto, f.Total, f.Estado,
            d.ID_Detalle, d.Descripcion, d.Monto
        FROM Factura f
        LEFT JOIN Factura_Detalle d ON f.ID_Factura = d.ID_Factura
        WHERE f.Fecha_Emision BETWEEN @Inicio AND @Fin
        ORDER BY f.Fecha_Emision DESC;";

            var facturaDict = new Dictionary<int, Factura>();

            var list = await connection.QueryAsync<Factura, FacturaDetalle, Factura>(
                sql,
                (factura, detalle) =>
                {
                    if (!facturaDict.TryGetValue(factura.ID_Factura, out var currentFactura))
                    {
                        currentFactura = factura;
                        currentFactura.Detalles = new List<FacturaDetalle>();
                        facturaDict.Add(currentFactura.ID_Factura, currentFactura);
                    }

                    if (detalle != null)
                        currentFactura.Detalles.Add(detalle);

                    return currentFactura;
                },
                new { Inicio = inicio, Fin = fin },
                splitOn: "ID_Detalle"
            );

            return facturaDict.Values;
        }
        public async Task<IEnumerable<Factura>> Facturas_UsuarioAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Consulta que trae datos de Estudiante, Factura y Factura_Detalle
            var sql = @"
        SELECT 
            e.ID_Estudiante,
            e.Identificacion,
            e.Nombre + ' ' + e.Apellido1 + ' ' + e.Apellido2 AS Nombre_Completo,
            f.ID_Factura,
            f.Fecha_Emision,
            f.Monto_Base,
            f.Impuesto,
            f.Total,
            f.Estado,
            fd.ID_Detalle,
            fd.Descripcion,
            fd.Monto AS Monto_Detalle,
            fd.Detalle
        FROM Estudiante e
        JOIN Factura f ON e.Identificacion = f.Identificacion_Estudiante
        LEFT JOIN Factura_Detalle fd ON f.ID_Factura = fd.ID_Factura
        WHERE e.Identificacion = @Identificacion
        ORDER BY f.Fecha_Emision DESC, fd.ID_Detalle;";

            var facturaDict = new Dictionary<int, Factura>();

            // Definimos el mapeo para Estudiante, Factura y FacturaDetalle
            // Aquí Factura actuará como contenedor del Estudiante y la lista de Detalles
            await connection.QueryAsync<Estudiante, Factura, FacturaDetalle, Factura>(
                sql,
                (estudiante, factura, detalle) =>
                {
                    if (!facturaDict.TryGetValue(factura.ID_Factura, out var currentFactura))
                    {
                        // Si la factura no existe en el diccionario, la agregamos
                        currentFactura = factura;
                        currentFactura.Estudiante = estudiante; // Asignamos los datos del estudiante
                        currentFactura.Detalles = new List<FacturaDetalle>();
                        facturaDict.Add(currentFactura.ID_Factura, currentFactura);
                    }

                    // Aseguramos que la factura tenga asignado el estudiante si viene en la fila
                    if (currentFactura.Estudiante == null)
                    {
                        currentFactura.Estudiante = estudiante;
                    }


                    if (detalle != null)
                    {
                        // El alias 'Monto_Detalle' debe mapear correctamente a 'Monto' en FacturaDetalle.
                        // Y 'Detalle' en la consulta SQL debe mapear a 'Detalle' en FacturaDetalle si existe esa propiedad.
                        currentFactura.Detalles.Add(detalle);
                    }

                    return currentFactura;
                },
                new { Identificacion = identificacion },
                splitOn: "ID_Factura, ID_Detalle" // Separamos las columnas donde empieza el siguiente objeto (Factura y FacturaDetalle)
            );

            return facturaDict.Values;
        }

    }
}
