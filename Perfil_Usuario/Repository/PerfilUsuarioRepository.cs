using Perfil_Usuario.Entities;
using Dapper;

namespace Perfil_Usuario.Repository
{
    public class PerfilUsuarioRepository
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;


        public PerfilUsuarioRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Consulta el perfil de usuario
        public async Task<PerfilUsuario?> ObtenerPerfilAsync(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT 
                    U.Email,
	                TP.Nombre AS Tipo_Identificacion,
                    U.Identificacion,
                    U.Nombre
                FROM Usuario U
                INNER JOIN TIPO_IDENTIFICACION TP
                    ON U.ID_Tipo_Identificacion = TP.ID_Tipo_Identificacion
                WHERE Email = @Email; ";

            return await connection.QueryFirstOrDefaultAsync<PerfilUsuario>(sql, new
            {
                Email = email
            });
        }

    }
}
