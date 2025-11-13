using System.Data;

namespace Adm_Facturacion.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
