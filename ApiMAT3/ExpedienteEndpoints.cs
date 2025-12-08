using ApiMAT3.Entities;
using ApiMAT3.Services;

namespace ApiMAT3.Endpoints
{
    public static class ExpedienteEndpoints
    {
        public static void MapExpedienteEndpoints(this IEndpointRouteBuilder app)
        {
            
            app.MapGet("/expediente", async (IExpedienteService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var expedientes = service.ObtenerTodos();
                return Results.Ok(expedientes);
            });

            
            app.MapGet("/expediente/{numeroIdentificacion}", async (string numeroIdentificacion, IExpedienteService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var expediente = service.ObtenerPorId(numeroIdentificacion);
                return expediente is not null ? Results.Ok(expediente) : Results.NotFound();
            });

            
            app.MapPost("/expediente", async (Expediente expediente, IExpedienteService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                service.Crear(expediente);
                return Results.Ok("Expediente creado correctamente.");
            });

       
            app.MapPut("/expediente", async (Expediente expediente, IExpedienteService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                service.Actualizar(expediente);
                return Results.Ok("Expediente actualizado correctamente.");
            });

        
            app.MapDelete("/expediente/{numeroIdentificacion}", async (string numeroIdentificacion, IExpedienteService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                service.Eliminar(numeroIdentificacion);
                return Results.Ok("Expediente eliminado correctamente.");
            });
        }
    }
}
