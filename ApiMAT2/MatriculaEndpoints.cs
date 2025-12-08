using ApiMAT2.Entities;
using ApiMAT2.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiMAT2
{
    public static class MatriculaEndpoints
    {
        public static void MapMatriculaEndpoints(this WebApplication app)
        {
           
            app.MapPost("/matricula", async ([FromBody] MatriculaRequest request, [FromServices] IMatriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Crear(request);
                    return Results.Ok(new { message = "Matrícula creada correctamente." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

           
            app.MapPut("/matricula", async ([FromBody] MatriculaRequest request, [FromServices] IMatriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Actualizar(request);
                    return Results.Ok(new { message = "Matrícula modificada correctamente." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

           
       
            app.MapDelete("/matricula/{id}", async ([FromRoute] int id, [FromServices] IMatriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Eliminar(id);
                    return Results.Ok(new { message = "Matrícula eliminada correctamente." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });


            app.MapGet("/matricula/{idCurso}/{idGrupo}", async ([FromRoute] int idCurso, [FromRoute] int idGrupo, [FromServices] IMatriculaService service, IAutenticacionService auth, HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    var resultado = service.ObtenerPorCursoYGrupo(idCurso, idGrupo);
                    return Results.Ok(resultado);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });
        }
    }
}
