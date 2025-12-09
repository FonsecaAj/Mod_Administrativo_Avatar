using Dapper;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Repository
{
    public class BitacoraRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public BitacoraRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<int> Registrar(Bitacora bitacora)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"INSERT INTO Bitacora (Fecha_Registro, Usuario, Descripcion, Tipo_Accion)
                            VALUES (GETDATE(), @Usuario, @Descripcion, @Tipo_Accion);
                            SELECT SCOPE_IDENTITY();";

                return await connection.ExecuteScalarAsync<int>(sql, new
                {
                    bitacora.Usuario,
                    bitacora.Descripcion,
                    bitacora.Tipo_Accion
                });
            }
        }

        public async Task<List<Bitacora>> ObtenerTodas(string? usuario = null)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                string sql;
                object? parametros = null;

                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    sql = @"SELECT ID_Bitacora, Fecha_Registro, Usuario, Descripcion, Tipo_Accion 
                            FROM Bitacora 
                            WHERE Usuario = @Usuario
                            ORDER BY Fecha_Registro DESC";
                    parametros = new { Usuario = usuario };
                }
                else
                {
                    sql = @"SELECT ID_Bitacora, Fecha_Registro, Usuario, Descripcion, Tipo_Accion 
                            FROM Bitacora 
                            ORDER BY Fecha_Registro DESC";
                }

                var result = await connection.QueryAsync<Bitacora>(sql, parametros);
                return result.ToList();
            }
        }
    }
}