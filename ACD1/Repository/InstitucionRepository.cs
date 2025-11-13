using Dapper;
using ACD1.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ACD1.Repository
{
    public class InstitucionRepository : IInstitucionRepository
    {
        private readonly string _connectionString;

        public InstitucionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(Institucion institucion)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO INSTITUCION (NOMBRE) 
                       OUTPUT INSERTED.ID_INSTITUCION
                       VALUES (@Nombre)";

            return await connection.ExecuteScalarAsync<int>(sql, new { institucion.Nombre });
        }

        public async Task ActualizarAsync(Institucion institucion)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE INSTITUCION 
                       SET NOMBRE = @Nombre,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_INSTITUCION = @IdInstitucion";

            await connection.ExecuteAsync(sql, institucion);
        }

        public async Task EliminarAsync(int idInstitucion)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM INSTITUCION WHERE ID_INSTITUCION = @IdInstitucion";
            await connection.ExecuteAsync(sql, new { IdInstitucion = idInstitucion });
        }

        public async Task<IEnumerable<Institucion>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_INSTITUCION as IdInstitucion,
                              NOMBRE as Nombre,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM INSTITUCION
                       ORDER BY NOMBRE";
            return await connection.QueryAsync<Institucion>(sql);
        }

        public async Task<IEnumerable<Institucion>> BuscarPorNombreAsync(string nombre)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_INSTITUCION as IdInstitucion,
                              NOMBRE as Nombre,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM INSTITUCION
                       WHERE NOMBRE LIKE @Nombre
                       ORDER BY NOMBRE";
            return await connection.QueryAsync<Institucion>(sql, new { Nombre = $"%{nombre}%" });
        }

        public async Task<Institucion?> ObtenerPorIdAsync(int idInstitucion)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_INSTITUCION as IdInstitucion,
                              NOMBRE as Nombre,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM INSTITUCION 
                       WHERE ID_INSTITUCION = @IdInstitucion";
            return await connection.QueryFirstOrDefaultAsync<Institucion>(sql, new { IdInstitucion = idInstitucion });
        }
    }
}