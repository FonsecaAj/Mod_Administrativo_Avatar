using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using USR5.Entities;

namespace USR5.Repository
{
    public class AutenticacionRepository : IAutenticacionRepository
    {
        private readonly string _connectionString;

        public AutenticacionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<string> CrearRefreshTokenAsync(RefreshToken token)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO REFRESH_TOKEN 
                       (TOKEN, USUARIO_EMAIL, FECHA_EXPIRACION, FECHA_CREACION, ACTIVO) 
                       VALUES 
                       (@Token, @UsuarioEmail, @FechaExpiracion, @FechaCreacion, @Activo)";

            await connection.ExecuteAsync(sql, token);
            return token.Token;
        }

        public async Task<RefreshToken?> ObtenerRefreshTokenAsync(string token)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT 
                           ID_REFRESH_TOKEN as IdRefreshToken,
                           TOKEN as Token,
                           USUARIO_EMAIL as UsuarioEmail,
                           FECHA_EXPIRACION as FechaExpiracion,
                           FECHA_CREACION as FechaCreacion,
                           ACTIVO as Activo
                       FROM REFRESH_TOKEN 
                       WHERE TOKEN = @Token";

            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task DesactivarRefreshTokenAsync(string token)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE REFRESH_TOKEN SET ACTIVO = 0 WHERE TOKEN = @Token";
            await connection.ExecuteAsync(sql, new { Token = token });
        }

        public async Task<string> CrearJwtTokenAsync(JwtToken token)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO JWT_TOKEN 
                       (TOKEN, USUARIO_EMAIL, FECHA_EXPIRACION, FECHA_CREACION, ACTIVO) 
                       VALUES 
                       (@Token, @UsuarioEmail, @FechaExpiracion, @FechaCreacion, @Activo)";

            await connection.ExecuteAsync(sql, token);
            return token.Token;
        }

        public async Task<JwtToken?> ObtenerJwtTokenAsync(string token)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT 
                           ID_JWT_TOKEN as IdJwtToken,
                           TOKEN as Token,
                           USUARIO_EMAIL as UsuarioEmail,
                           FECHA_EXPIRACION as FechaExpiracion,
                           FECHA_CREACION as FechaCreacion,
                           ACTIVO as Activo
                       FROM JWT_TOKEN 
                       WHERE TOKEN = @Token";

            return await connection.QueryFirstOrDefaultAsync<JwtToken>(sql, new { Token = token });
        }

        public async Task DesactivarJwtTokenAsync(string token)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE JWT_TOKEN SET ACTIVO = 0 WHERE TOKEN = @Token";
            await connection.ExecuteAsync(sql, new { Token = token });
        }

        public async Task DesactivarJwtTokensUsuarioAsync(string email)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE JWT_TOKEN SET ACTIVO = 0 WHERE USUARIO_EMAIL = @Email";
            await connection.ExecuteAsync(sql, new { Email = email });
        }

        public async Task DesactivarTokensUsuarioAsync(string email)
        {
            using var connection = CreateConnection();

            var sqlJwt = "UPDATE JWT_TOKEN SET ACTIVO = 0 WHERE USUARIO_EMAIL = @Email";
            await connection.ExecuteAsync(sqlJwt, new { Email = email });

            var sqlRefresh = "UPDATE REFRESH_TOKEN SET ACTIVO = 0 WHERE USUARIO_EMAIL = @Email";
            await connection.ExecuteAsync(sqlRefresh, new { Email = email });
        }
    }
}