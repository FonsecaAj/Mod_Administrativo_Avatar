using ApiMAT1.Entities;
using ApiMAT1.Services;

namespace ApiMAT1.Endpoints
{
    public static class PrematriculaEndpoints
    {
        public static void MapPrematriculaEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/prematricula").WithTags("Prematricula");

            group.MapGet("/", async (IPrematriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var token = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Unauthorized();

                var result = await service.ObtenerTodas();
                return Results.Ok(result);
            });

            group.MapGet("/{id:int}", async (int id, IPrematriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var token = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Unauthorized();

                var result = await service.ObtenerPorId(id);
                return Results.Ok(result);
            });

            group.MapPost("/", async (Prematricula entidad, IPrematriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var token = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Unauthorized();

                var result = await service.Crear(entidad);
                return Results.Ok(result);
            });

            group.MapPut("/{id:int}", async (int id, Prematricula entidad, IPrematriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var token = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Unauthorized();

                entidad.ID_Prematricula = id;   // 🔥 Aseguramos que el ID venga correcto

                var result = await service.Actualizar(entidad);

                return Results.Ok(result);
            });


            group.MapDelete("/{id:int}", async (int id, IPrematriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var token = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Unauthorized();

                var result = await service.Eliminar(id);
                return Results.Ok(result);
            });
        }
    }
}
