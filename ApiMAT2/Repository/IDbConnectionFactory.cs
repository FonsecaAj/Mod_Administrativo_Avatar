
using System.Data;

namespace ApiMAT2.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();

    }
}
