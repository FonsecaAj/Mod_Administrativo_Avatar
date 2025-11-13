using System.Data;

namespace Estudiantes_Matriculados.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
