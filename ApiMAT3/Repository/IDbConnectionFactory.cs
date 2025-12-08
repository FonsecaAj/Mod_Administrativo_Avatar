using System.Data;

namespace ApiMAT3.Repository
{
    
        public interface IDbConnectionFactory
        {
            IDbConnection CreateConnection();

        }
    
}
