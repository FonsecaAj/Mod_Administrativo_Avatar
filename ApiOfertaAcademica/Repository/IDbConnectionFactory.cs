using System.Data;

namespace ApiACD3.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}