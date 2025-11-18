using Dapper;
using ApiACD3.Entities;


namespace ApiACD3.Repository
{
    public class CursoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CursoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Obtener todos los cursos activos
        public async Task<IEnumerable<Curso>> ObtenerTodos()
        {
            using var conn = _connectionFactory.CreateConnection();
            var query = "SELECT * FROM Curso";
            return await conn.QueryAsync<Curso>(query);
        }

        // Obtener un curso por su ID
        public async Task<Curso?> ObtenerPorId(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            var query = "SELECT * FROM Curso WHERE ID_Curso = @ID_Curso";
            return await conn.QueryFirstOrDefaultAsync<Curso>(query, new { ID_Curso = id });
        }

        // Obtener todos los cursos de una carrera específica
        public async Task<IEnumerable<Curso>> ObtenerPorCarrera(int idCarrera)
        {
            using var conn = _connectionFactory.CreateConnection();
            var query = "SELECT * FROM Curso WHERE ID_Carrera = @ID_Carrera";
            return await conn.QueryAsync<Curso>(query, new { ID_Carrera = idCarrera });
        }

        // Crear un nuevo curso
        public async Task<int> Crear(Curso curso)
        {
            using var conn = _connectionFactory.CreateConnection();
            var query = @"
                INSERT INTO Curso (Identificador, ID_Carrera, Nivel, Nombre)
                VALUES (@Identificador, @ID_Carrera, @Nivel, @Nombre)";
            return await conn.ExecuteAsync(query, curso);
        }

        // Actualizar un curso existente
        public async Task<int> Actualizar(Curso curso)
        {
            using var conn = _connectionFactory.CreateConnection();

         
            if (curso.ID_Curso <= 0)
                throw new ArgumentException("El ID del curso no es válido.");

            var query = @"
                UPDATE Curso
                SET 
                    Identificador = @Identificador,
                    ID_Carrera = @ID_Carrera,
                    Nivel = @Nivel,
                    Nombre = @Nombre
                WHERE ID_Curso = @ID_Curso";

          
            var parametros = new
            {
                ID_Curso = curso.ID_Curso,
                curso.Identificador,
                curso.ID_Carrera,
                curso.Nivel,
                curso.Nombre
            };

            return await conn.ExecuteAsync(query, parametros);
        }


        // Eliminar (inactivar) un curso
        public async Task<int> Eliminar(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            var query = "DELETE FROM Curso WHERE ID_Curso = @ID_Curso";
            return await conn.ExecuteAsync(query, new { ID_Curso = id });
        }

        public async Task<IEnumerable<dynamic>> ObtenerCarrerasLookup()
        {
            using var conn = _connectionFactory.CreateConnection();

            var sql = @"SELECT ID_Carrera AS Id, Nombre 
                FROM CARRERA
                WHERE Estado = 1";

            return await conn.QueryAsync<dynamic>(sql);
        }




    }
}
