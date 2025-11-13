using System.Data;

namespace Avatar_Mod_Administración.Repository
{
    public interface IDbConnectionFactory
    {

        IDbConnection CreateConnection();

    }
}
