using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using USR5.Entities;

namespace USR5.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("UsuarioConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<Usuario?> ValidarCredencialesAsync(string email, string contrasenna)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT EMAIL as Email, 
                              ID_TIPO_IDENTIFICACION as IdTipoIdentificacion,
                              IDENTIFICACION as Identificacion,
                              NOMBRE as Nombre,
                              ID_ROL as IdRol,
                              CONTRASENNA as Contrasenna,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM USUARIO 
                       WHERE EMAIL = @Email AND ACTIVO = 1";

            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });

            if (usuario == null)
                return null;

            var contrasennaEncriptada = EncriptarContrasenna(contrasenna);
            if (usuario.Contrasenna != contrasennaEncriptada)
                return null;

            return usuario;
        }

        private string EncriptarContrasenna(string contrasenna)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasenna));
            return Convert.ToBase64String(bytes);
        }
    }
}