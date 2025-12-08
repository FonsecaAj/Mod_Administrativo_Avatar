using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApiACD3.Repository
{
    // Nueva clase para la conexión específica de Matrícula/Estudiante
    public class CursoDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public CursoDbConnectionFactory(IConfiguration configuration)
        {
            // Usa la conexión específica "ConexionCURSOS"
            _connectionString = configuration.GetConnectionString("ConexionCURSOS")
                                ?? throw new InvalidOperationException("La cadena de conexión 'ConexionCURSOS' no está configurada.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}