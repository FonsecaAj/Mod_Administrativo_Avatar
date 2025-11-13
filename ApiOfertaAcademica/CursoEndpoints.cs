using ApiACD3.Entities;
using ApiACD3.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiACD3
{
    public static class CursoEndpoints
    {
        public static void MapCursoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/curso").WithTags(nameof(Curso));

         
            group.MapGet("/", async (
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerTodos();

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

      
            group.MapGet("/{id:int}", async (
                int id,
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorId(id);

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

           
            group.MapGet("/carrera/{idCarrera:int}", async (
                int idCarrera,
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorCarrera(idCarrera);

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

       
            group.MapPost("/", async (
                [FromBody] Curso cursoData,
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Crear(cursoData);

                return result.StatusCode switch
                {
                    201 => Results.Json(result, statusCode: 201),
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    500 => Results.Json(result, statusCode: 500),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });


            group.MapPut("/{id:int}", async (
                int id,
                [FromBody] Curso cursoData,
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();
                
                cursoData.ID_Curso = id;

                var result = await service.Actualizar(cursoData);

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    500 => Results.Json(result, statusCode: 500),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

           
            group.MapDelete("/{id:int}", async (
                int id,
                ICursoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Eliminar(id);

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    500 => Results.Json(result, statusCode: 500),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
        
            });


            group.MapGet("/lookups", async (ICursoService service) =>
            {
                var result = await service.ObtenerLookups();
                return Results.Json(result, statusCode: result.StatusCode);
            });


        }
    }
}
