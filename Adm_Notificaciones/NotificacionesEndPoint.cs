using Adm_Notificaciones.Entities;
using Adm_Notificaciones.Service;
using Microsoft.AspNetCore.Mvc;

namespace Adm_Notificaciones
{
    public static class NotificacionesEndPoint
    {

        public static void MapNotificacionEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/notificar").WithTags("Notificaciones");

            group.MapPost("/", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromBody] NotificacionRequest request,
                [FromServices] NotificacionService service) =>
            {
                var response = await service.EnviarCorreoAsync(request, token);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("EnviarNotificacion")
            .WithOpenApi();
        }

    }
}
