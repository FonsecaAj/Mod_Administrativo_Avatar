using Microsoft.Data.SqlClient;
using System.Data;
using USR2.Entities;
using Dapper;

namespace USR2.Repository
{
    public class RolModuloRepository : IRolModuloRepository
    {
        private readonly string _connectionString;

        public RolModuloRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(RolModulo rolModulo)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO ROL_MODULO (ID_ROL, ID_MODULO) 
                       OUTPUT INSERTED.ID_ROL_MODULO
                       VALUES (@IdRol, @IdModulo)";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                rolModulo.IdRol,
                rolModulo.IdModulo
            });
        }

        public async Task EliminarAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM ROL_MODULO WHERE ID_ROL_MODULO = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task EliminarPorRolYModuloAsync(int idRol, int idModulo)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM ROL_MODULO WHERE ID_ROL = @IdRol AND ID_MODULO = @IdModulo";
            await connection.ExecuteAsync(sql, new { IdRol = idRol, IdModulo = idModulo });
        }

        public async Task<IEnumerable<RolModulo>> ObtenerPorRolAsync(int idRol)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_ROL_MODULO as IdRolModulo,
                              ID_ROL as IdRol,
                              ID_MODULO as IdModulo,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion
                       FROM ROL_MODULO
                       WHERE ID_ROL = @IdRol
                       ORDER BY ID_MODULO";
            return await connection.QueryAsync<RolModulo>(sql, new { IdRol = idRol });
        }

        public async Task<RolModulo?> ObtenerPorIdAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_ROL_MODULO as IdRolModulo,
                              ID_ROL as IdRol,
                              ID_MODULO as IdModulo,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion
                       FROM ROL_MODULO
                       WHERE ID_ROL_MODULO = @Id";
            return await connection.QueryFirstOrDefaultAsync<RolModulo>(sql, new { Id = id });
        }

        public async Task<bool> ExisteAsignacionAsync(int idRol, int idModulo)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM ROL_MODULO WHERE ID_ROL = @IdRol AND ID_MODULO = @IdModulo";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { IdRol = idRol, IdModulo = idModulo });
            return count > 0;
        }

        public async Task EliminarTodosPorRolAsync(int idRol)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM ROL_MODULO WHERE ID_ROL = @IdRol";
            await connection.ExecuteAsync(sql, new { IdRol = idRol });
        }
    }
}
