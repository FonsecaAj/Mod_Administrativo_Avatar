using ApiACD5.Entities;
using ApiACD5.Services;

namespace ApiACD5.Endpoints
{
    public static class PeriodoEndpoints
    {
        public static void MapPeriodoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/periodo").WithTags("Periodo");

            group.MapGet("/", async (IPeriodoService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();

                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerTodos();
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/{id:int}", async (int id, IPeriodoService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();

                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorId(id);
                return Results.Json(result, statusCode: result.StatusCode);
            });


            group.MapPost("/", async (Periodo periodo, IPeriodoService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();

                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Crear(periodo);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapPut("/", async (Periodo periodo, IPeriodoService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();

                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Modificar(periodo);
                return Results.Json(result, statusCode: result.StatusCode);
            });


            group.MapDelete("/{id:int}", async (int id, IPeriodoService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();

                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var result = await service.Eliminar(id);
                return Results.Json(result, statusCode: result.StatusCode);
            });
        }
    }
}
