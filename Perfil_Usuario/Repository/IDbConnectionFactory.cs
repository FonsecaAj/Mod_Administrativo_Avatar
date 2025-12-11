using System.Data;

namespace Perfil_Usuario.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();

    }
}
