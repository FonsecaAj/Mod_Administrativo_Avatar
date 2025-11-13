using ApiACD4.Entities;
using ApiACD4.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiACD4
{
    public static class GrupoEndpoints
    {
        public static void MapGrupoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/grupo").WithTags(nameof(Grupo));
         



            group.MapGet("/", async (
                IGrupoService service,
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
                IGrupoService service,
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

       
            group.MapGet("/por-curso/{idCurso:int}", async (
                int idCurso,
                IGrupoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorCurso(idCurso);

                return result.StatusCode switch
                {
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    404 => Results.Json(result, statusCode: 404),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

      
            group.MapPost("/", async (
                [FromBody] Grupo grupoData,
                IGrupoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Crear(grupoData);

                return result.StatusCode switch
                {
                    201 => Results.Json(result, statusCode: 201),
                    200 => Results.Json(result, statusCode: 200),
                    400 => Results.Json(result, statusCode: 400),
                    500 => Results.Json(result, statusCode: 500),
                    _ => Results.Json(result, statusCode: result.StatusCode)
                };
            });

      
            group.MapPut("/", async (
                [FromBody] Grupo grupoData,
                IGrupoService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Actualizar(grupoData);

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
                IGrupoService service,
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
        }
    }
}
