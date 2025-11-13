using System.Data;

namespace ApiMAT1.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}