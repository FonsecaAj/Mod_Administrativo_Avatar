using ApiACD3.Entities;
using ApiACD3.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiACD3;

public static class CursoEndpoints
{
    public static void MapCursoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/curso").WithTags("Curso");

        group.MapGet("/", async (ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.ObtenerTodos();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/{id:int}", async (int id, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.ObtenerPorId(id);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/miscursos", async (string id, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.ObtenerMisCursos(id);
            return Results.Json(result, statusCode: result.StatusCode);
        });


        group.MapGet("/carrera/{idCarrera:int}", async (int idCarrera, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.ObtenerPorCarrera(idCarrera);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPost("/", async ([FromBody] Curso curso, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.Crear(curso);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] Curso curso, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            curso.ID_Curso = id;
            var result = await service.Actualizar(curso);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.Eliminar(id);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/lookups", async (ICursoService service, IAutenticacionService auth, HttpContext http) =>
        {
            var token = http.Request.Headers["Authorization"].ToString();
            if (!await auth.ValidarTokenAsync(token))
                return Results.Unauthorized();

            var result = await service.ObtenerLookups();
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}
