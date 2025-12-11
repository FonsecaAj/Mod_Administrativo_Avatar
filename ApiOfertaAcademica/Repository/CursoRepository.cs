using Dapper;
using ApiACD3.Entities;
using System.Collections.Generic;
using System.Data;

namespace ApiACD3.Repository
{
    public class CursoRepository
    {
        // Factoría para la base de datos de Oferta Académica (ConexionSQL) - Usada en la mayoría de métodos CRUD
        private readonly IDbConnectionFactory _ofertaAcademicaFactory;

        // Factoría para la base de datos de Matrícula (ConexionCURSOS) - Usada para ObtenerMisCursos
        private readonly IDbConnectionFactory _matriculaFactory;

        // El constructor ahora inyecta ambas implementaciones concretas.
        public CursoRepository(
            DbConnectionFactory ofertaAcademicaFactory,
            CursoDbConnectionFactory matriculaFactory)
        {
            // Almacenamos la referencia a la factoría de Oferta Académica
            _ofertaAcademicaFactory = ofertaAcademicaFactory;
            // Almacenamos la referencia a la factoría de Matrícula
            _matriculaFactory = matriculaFactory;
        }

        // Obtener todos los cursos activos
        public async Task<IEnumerable<Curso>> ObtenerTodos()
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();
            var query = "SELECT * FROM Curso";
            return await conn.QueryAsync<Curso>(query);
        }

        // Obtener un curso por su ID
        public async Task<Curso?> ObtenerPorId(int id)
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();
            var query = "SELECT * FROM Curso WHERE ID_Curso = @ID_Curso";
            return await conn.QueryFirstOrDefaultAsync<Curso>(query, new { ID_Curso = id });
        }

        // Obtener todos los cursos de una carrera específica
        public async Task<IEnumerable<Curso>> ObtenerPorCarrera(int idCarrera)
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();
            var query = "SELECT * FROM Curso WHERE ID_Carrera = @ID_Carrera";
            return await conn.QueryAsync<Curso>(query, new { ID_Carrera = idCarrera });
        }

        // Crear un nuevo curso
        public async Task<int> Crear(Curso curso)
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();
            var query = @"
                INSERT INTO Curso (Identificador, ID_Carrera, Nivel, Nombre)
                VALUES (@Identificador, @ID_Carrera, @Nivel, @Nombre)";
            return await conn.ExecuteAsync(query, curso);
        }

        // Actualizar un curso existente
        public async Task<int> Actualizar(Curso curso)
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();

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
            using var conn = _ofertaAcademicaFactory.CreateConnection();
            var query = "DELETE FROM Curso WHERE ID_Curso = @ID_Curso";
            return await conn.ExecuteAsync(query, new { ID_Curso = id });
        }

        public async Task<IEnumerable<dynamic>> ObtenerCarrerasLookup()
        {
            using var conn = _ofertaAcademicaFactory.CreateConnection();

            var sql = @"SELECT ID_Carrera AS Id, Nombre 
                FROM CARRERA
                WHERE Estado = 1";

            return await conn.QueryAsync<dynamic>(sql);
        }

        // Obtener cursos para estudiantes (USA CONEXIÓN DE MATRÍCULA)
        public async Task<List<Curso>> ObtenerMisCursos(string id)
        {
            // *** USANDO LA CONEXIÓN DE MATRÍCULA (_matriculaFactory) ***
            using var conn = _matriculaFactory.CreateConnection();

            // Query SQL con ALIASES para garantizar el mapeo correcto a la entidad Curso.
            // Usamos AS para asignar el valor de la columna SQL al nombre de la propiedad C#.
            var query = @"
    SELECT DISTINCT
        -- Mapeo 1: C.Codigo_Curso (Columna SQL) se mapea a dos propiedades en C#:
        --           - Codigo_Curso (Propiedad redundante)
        --           - Identificador (Propiedad principal del código)
        C.Codigo_Curso,
        C.Codigo_Curso AS Identificador, 
        
        -- Mapeo 2: C.Nombre_Curso (Columna SQL) se mapea a la propiedad Nombre (Entidad C#)
        C.Nombre_Curso AS Nombre,        
        
        -- Campo requerido por la entidad (aunque no se selecciona, se necesita un valor)
        -- Si esta columna existe en la tabla Curso de la BD de Matrícula, SELECCIÓNALA:
        C.ID_Curso 
        -- Si ID_Curso no existe en la tabla Curso de la BD de Matrícula, 
        -- reemplaza la línea anterior con: NULL AS ID_Curso 
        -- o selecciona cualquier clave primaria de las tablas unidas.

    FROM 
        dbo.Estudiante AS E
    INNER JOIN 
        dbo.Matricula AS M ON E.ID_Estudiante = M.ID_Estudiante
    INNER JOIN 
        dbo.Grupo AS G ON M.ID_Grupo = G.ID_Grupo
    INNER JOIN 
        dbo.Curso AS C ON G.ID_Curso = C.ID_Curso
    WHERE 
        E.Identificacion = @Identificacion;";

            var cursos = await conn.QueryAsync<Curso>(query, new { Identificacion = id });

            // Devolver la lista.
            return cursos.ToList();
        }

        public async Task<List<MisCursoDto>> ObtenerMisCursosConDetalle(string identificacion)
        {
            // Usamos la conexión de la BD de matrícula
            using var conn = _matriculaFactory.CreateConnection();

            var sql = @"
        SELECT DISTINCT
            C.ID_Curso,
            C.Codigo_Curso,
            C.Nombre_Curso AS Nombre,

            -- Datos del grupo
            G.Nombre_Grupo AS Grupo,
            ISNULL(
                'Profesor ' + CAST(G.ID_Profesor AS VARCHAR(10)),
                ''
            ) AS Profesor,
            ISNULL(G.Horario, '') AS Horario,

            -- Periodo formateado
            CASE 
                WHEN P.ID_Periodo IS NULL THEN ''
                ELSE 'Año ' 
                     + CAST(P.[Año] AS VARCHAR(4)) 
                     + ' - Periodo ' 
                     + CAST(P.Numero_Periodo AS VARCHAR(2))
            END AS Periodo,

            -- ¿Es periodo actual?
            CASE 
                WHEN P.ID_Periodo IS NOT NULL
                     AND GETDATE() BETWEEN P.Fecha_Inicio AND P.Fecha_Fin
                THEN 1 
                ELSE 0 
            END AS EsPeriodoActual
        FROM dbo.Estudiante  AS E
        INNER JOIN dbo.Matricula AS M ON E.ID_Estudiante = M.ID_Estudiante
        INNER JOIN dbo.Grupo     AS G ON M.ID_Grupo      = G.ID_Grupo
        INNER JOIN dbo.Curso     AS C ON G.ID_Curso      = C.ID_Curso
        LEFT  JOIN dbo.Periodo   AS P ON G.ID_Periodo    = P.ID_Periodo
        WHERE E.Identificacion = @Identificacion;
    ";

            var resultado = await conn.QueryAsync<MisCursoDto>(
                sql,
                new { Identificacion = identificacion }
            );

            return resultado.ToList();
        }





    }
}