using Dapper;
using ACD2.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ACD2.Repository
{
    public class CarreraRepository : ICarreraRepository
    {
        private readonly string _connectionString;

        public CarreraRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(Carrera carrera)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO CARRERA (NOMBRE, ID_INSTITUCION, ID_DIRECTOR) 
                       OUTPUT INSERTED.ID_CARRERA
                       VALUES (@Nombre, @IdInstitucion, @IdDirector)";

            return await connection.ExecuteScalarAsync<int>(sql, carrera);
        }

        public async Task ActualizarAsync(Carrera carrera)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE CARRERA 
                       SET NOMBRE = @Nombre,
                           ID_INSTITUCION = @IdInstitucion,
                           ID_DIRECTOR = @IdDirector,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_CARRERA = @IdCarrera";

            await connection.ExecuteAsync(sql, carrera);
        }

        public async Task EliminarAsync(int idCarrera)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM CARRERA WHERE ID_CARRERA = @IdCarrera";
            await connection.ExecuteAsync(sql, new { IdCarrera = idCarrera });
        }

        public async Task<IEnumerable<Carrera>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_CARRERA as IdCarrera,
                              NOMBRE as Nombre,
                              ID_INSTITUCION as IdInstitucion,
                              ID_DIRECTOR as IdDirector,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM CARRERA";
            return await connection.QueryAsync<Carrera>(sql);
        }

        public async Task<Carrera?> ObtenerPorIdAsync(int idCarrera)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_CARRERA as IdCarrera,
                              NOMBRE as Nombre,
                              ID_INSTITUCION as IdInstitucion,
                              ID_DIRECTOR as IdDirector,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM CARRERA 
                       WHERE ID_CARRERA = @IdCarrera";
            return await connection.QueryFirstOrDefaultAsync<Carrera>(sql, new { IdCarrera = idCarrera });
        }

        public async Task<IEnumerable<Carrera>> ObtenerPorInstitucionAsync(int idInstitucion)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_CARRERA as IdCarrera,
                              NOMBRE as Nombre,
                              ID_INSTITUCION as IdInstitucion,
                              ID_DIRECTOR as IdDirector,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM CARRERA 
                       WHERE ID_INSTITUCION = @IdInstitucion";
            return await connection.QueryAsync<Carrera>(sql, new { IdInstitucion = idInstitucion });
        }
    }
}