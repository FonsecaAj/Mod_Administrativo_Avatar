using Dapper;
using USR2.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace USR2.Repository
{
    public class RolRepository : IRolRepository
    {
        private readonly string _connectionString;

        public RolRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(Rol rol)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO ROL (NOMBRE) 
                       OUTPUT INSERTED.ID_ROL
                       VALUES (@Nombre)";

            return await connection.ExecuteScalarAsync<int>(sql, new { rol.Nombre });
        }

        public async Task ActualizarAsync(Rol rol)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE ROL 
                       SET NOMBRE = @Nombre,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_ROL = @IdRol";

            await connection.ExecuteAsync(sql, new { rol.Nombre, rol.IdRol });
        }

        public async Task EliminarAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM ROL WHERE ID_ROL = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_ROL as IdRol, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM ROL";
            return await connection.QueryAsync<Rol>(sql);
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_ROL as IdRol, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM ROL 
                       WHERE ID_ROL = @Id";
            return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { Id = id });
        }

        public async Task<Rol?> ObtenerPorNombreAsync(string nombre)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_ROL as IdRol, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM ROL 
                       WHERE LOWER(LTRIM(RTRIM(NOMBRE))) = LOWER(@Nombre)";
            return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { Nombre = nombre.Trim() });
        }
    }
}