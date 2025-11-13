using System.Data;

namespace ADM_Pagos.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();

    }
}
