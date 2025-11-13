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



    }
}
