// Repository/ParametroRepository.cs
using Dapper;
using USR3.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace USR3.Repository
{
    public class ParametroRepository : IParametroRepository
    {
        private readonly string _connectionString;

        public ParametroRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task CrearAsync(Parametro parametro)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO PARAMETRO (ID_PARAMETRO, VALOR) 
                       VALUES (@IdParametro, @Valor)";

            await connection.ExecuteAsync(sql, new { parametro.IdParametro, parametro.Valor });
        }

        public async Task ActualizarAsync(Parametro parametro)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE PARAMETRO 
                       SET VALOR = @Valor,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_PARAMETRO = @IdParametro";

            await connection.ExecuteAsync(sql, new { parametro.Valor, parametro.IdParametro });
        }

        public async Task EliminarAsync(string id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM PARAMETRO WHERE ID_PARAMETRO = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Parametro>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_PARAMETRO as IdParametro, 
                              VALOR as Valor, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM PARAMETRO";
            return await connection.QueryAsync<Parametro>(sql);
        }

        public async Task<IEnumerable<Parametro>> ObtenerPaginadosAsync(int pagina, int porPagina)
        {
            using var connection = CreateConnection();
            var offset = (pagina - 1) * porPagina;

            var sql = @"SELECT ID_PARAMETRO as IdParametro, 
                              VALOR as Valor, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM PARAMETRO
                       ORDER BY ID_PARAMETRO
                       OFFSET @Offset ROWS
                       FETCH NEXT @PorPagina ROWS ONLY";

            return await connection.QueryAsync<Parametro>(sql, new { Offset = offset, PorPagina = porPagina });
        }

        public async Task<int> ObtenerTotalAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(*) FROM PARAMETRO";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<Parametro?> ObtenerPorIdAsync(string id)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_PARAMETRO as IdParametro, 
                              VALOR as Valor, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM PARAMETRO 
                       WHERE ID_PARAMETRO = @Id";
            return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, new { Id = id });
        }
    }
}