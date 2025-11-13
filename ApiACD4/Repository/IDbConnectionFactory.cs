using System.Data;

namespace ApiACD4.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
