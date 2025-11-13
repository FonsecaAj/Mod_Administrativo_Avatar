using Adm_Direcciones.Services;
using Adm_Notas.Entities;
using Adm_Notas.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adm_Notas
{
    public static class NotasEndPoints
    {

        public static void MapRubrosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/rubros").WithTags(nameof(Rubro));

            // POST /api/rubros/cargardesglose
            group.MapPost("/cargardesglose", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromBody] DesgloseRequest request,
                [FromServices] IRubroServices service,
                [FromServices] IAutenticacionService auth) =>
            {
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var response = await service.CargarDesglose(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("CargarDesglose")
            .WithOpenApi();

            // GET /api/rubros/obtenerdesglose?idGrupo=#
            group.MapGet("/obtenerdesglose", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromQuery] int idGrupo,
                [FromServices] IRubroServices service,
                [FromServices] IAutenticacionService auth) =>
            {
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var response = await service.ObtenerDesglose(idGrupo);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerDesglose")
            .WithOpenApi();

            // POST /api/rubros/asignarnotarubro
            group.MapPost("/asignarnotarubro", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromBody] NotaRequest request,
                [FromServices] INotasServices service,
                [FromServices] IAutenticacionService auth) =>
            {
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var response = await service.AsignarNota(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("AsignarNotaRubro")
            .WithOpenApi();

            // GET /api/rubros/obtenernotas?idEstudiante=#&idCurso=#
            group.MapGet("/obtenernotas", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromQuery] int idEstudiante,
                [FromQuery] int idCurso,
                [FromServices] INotasServices service,
                [FromServices] IAutenticacionService auth) =>
            {
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var response = await service.ObtenerNotas(idEstudiante, idCurso);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerNotasEstudiante")
            .WithOpenApi();
        }


    }
}
