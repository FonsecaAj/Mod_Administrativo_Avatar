using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiMAT3.Repository
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("ConexionSQL"));
        }
    }
}
