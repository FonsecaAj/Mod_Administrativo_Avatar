using ApiACD6.Entities;
using ApiACD6.Services;

namespace ApiACD6.Endpoints
{
    public static class ProfesorEndpoints
    {
        public static void MapProfesorEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/profesor").WithTags("Profesor");

            
            group.MapGet("/", async (IProfesorService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerTodos();
                return Results.Json(result, statusCode: result.StatusCode);
            });

      
            group.MapGet("/{idProfesor:int}", async (int idProfesor, IProfesorService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorId(idProfesor);
                return Results.Json(result, statusCode: result.StatusCode);
            });

          
            group.MapPost("/", async (Profesor profesor, IProfesorService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Crear(profesor);
                return Results.Json(result, statusCode: result.StatusCode);
            });


            group.MapPut("/{idProfesor:int}", async (int idProfesor, Profesor profesor, IProfesorService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                profesor.ID_Profesor = idProfesor; // aseguramos que el ID está correcto

                var result = await service.Actualizar(profesor);
                return Results.Json(result, statusCode: result.StatusCode);
            });



            group.MapDelete("/{idProfesor:int}", async (int idProfesor, IProfesorService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Eliminar(idProfesor);
                return Results.Json(result, statusCode: result.StatusCode);
            });
        }
    }
}
