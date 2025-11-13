using Adm_Direcciones.Services;
using Microsoft.AspNetCore.Mvc;
using Mod_Academico.Services;

namespace Mod_Academico
{
    public static class HistorialAcademicoEndPoints
    {

        public static void MapHistorialEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/historialacademico").WithTags("Historial Académico");

            group.MapGet("/", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromQuery] string tipo,
                [FromQuery] string identificacion,
                [FromServices] IHistorialAcademicoService service,
                [FromServices] IAutenticacionService auth,
                [FromServices] BitacoraConsumer bitacora) =>
            {
                if (!await auth.ValidarTokenAsync(token))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                string usuario = await auth.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

                var response = await service.ObtenerHistorial(tipo, identificacion);

                await bitacora.RegistrarAccionAsync(usuario, "SELECT", new
                {
                    accion = "ConsultaHistorialAcadémico",
                    identificacion,
                    cantidad = response?.ResponseObject is IEnumerable<object> lista ? lista.Count() : 0
                });

                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerHistorialAcademico")
            .WithOpenApi();
        }



    }
}
