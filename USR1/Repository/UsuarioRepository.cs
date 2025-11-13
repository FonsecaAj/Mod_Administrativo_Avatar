using Dapper;
using USR1.Entities;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace USR1.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<string> CrearAsync(Usuario usuario)
        {
            using var connection = CreateConnection();
            usuario.Contrasenna = EncriptarContrasenna(usuario.Contrasenna);

            var sql = @"INSERT INTO USUARIO (EMAIL, ID_TIPO_IDENTIFICACION, IDENTIFICACION, NOMBRE, ID_ROL, CONTRASENNA) 
                       VALUES (@Email, @IdTipoIdentificacion, @Identificacion, @Nombre, @IdRol, @Contrasenna)";

            await connection.ExecuteAsync(sql, usuario);
            return usuario.Email;
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            using var connection = CreateConnection();

            // Solo encriptar si la contraseña NO está ya encriptada
            // Las contraseñas encriptadas con SHA256 en Base64 tienen exactamente 44 caracteres
            // Si tiene 44 caracteres, se asume que ya está encriptada (viene de BD)
            if (!string.IsNullOrWhiteSpace(usuario.Contrasenna) && usuario.Contrasenna.Length != 44)
            {
                usuario.Contrasenna = EncriptarContrasenna(usuario.Contrasenna);
            }

            var sql = @"UPDATE USUARIO 
               SET ID_TIPO_IDENTIFICACION = @IdTipoIdentificacion,
                   IDENTIFICACION = @Identificacion,
                   NOMBRE = @Nombre,
                   ID_ROL = @IdRol,
                   CONTRASENNA = @Contrasenna,
                   FECHA_MODIFICACION = GETDATE()
               WHERE EMAIL = @Email";

            await connection.ExecuteAsync(sql, usuario);
        }

        public async Task EliminarAsync(string email)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM USUARIO WHERE EMAIL = @Email";
            await connection.ExecuteAsync(sql, new { Email = email });
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
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
                       FROM USUARIO";
            return await connection.QueryAsync<Usuario>(sql);
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
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
                       WHERE EMAIL = @Email";
            return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
        }

        public async Task<IEnumerable<Usuario>> FiltrarAsync(string? identificacion, string? nombre, int? tipoIdentificacion)
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
                       WHERE (@Identificacion IS NULL OR IDENTIFICACION = @Identificacion)
                         AND (@Nombre IS NULL OR NOMBRE LIKE '%' + @Nombre + '%')
                         AND (@TipoIdentificacion IS NULL OR ID_TIPO_IDENTIFICACION = @TipoIdentificacion)";

            return await connection.QueryAsync<Usuario>(sql, new
            {
                Identificacion = identificacion,
                Nombre = nombre,
                TipoIdentificacion = tipoIdentificacion
            });
        }

        private string EncriptarContrasenna(string contrasenna)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasenna));
            return Convert.ToBase64String(bytes);
        }
    }
}