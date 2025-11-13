using ADM_Pagos.Entities;
using ADM_Pagos.Services;
using Microsoft.AspNetCore.Mvc;

namespace ADM_Pagos
{
    public static class PagoEndPoint
    {


        public static void MapPagoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/pago").WithTags("Pago");

            // Crear pago
            group.MapPost("/", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromBody] PagoRequest request,
                [FromServices] IPagoService service) =>
            {
                var resp = await service.CrearPagoAsync(request, authorization);
                return Results.Json(resp, statusCode: resp.StatusCode);
            })
            .WithName("CrearPago")
            .WithOpenApi();

            // Reversar pago
            group.MapPut("/{idPago}/reversar", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                int idPago,
                [FromServices] IPagoService service) =>
            {
                var resp = await service.ReversarPagoAsync(idPago, authorization);
                return Results.Json(resp, statusCode: resp.StatusCode);
            })
            .WithName("ReversarPago")
            .WithOpenApi();

            // Obtener pago por id (con detalle)
            group.MapGet("/{idPago}", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                int idPago,
                [FromServices] IPagoService service) =>
            {
                var resp = await service.ObtenerPagoAsync(idPago, authorization);
                return Results.Json(resp, statusCode: resp.StatusCode);
            })
            .WithName("ObtenerPago")
            .WithOpenApi();

            // Listado de pagos por periodo
            group.MapGet("/listado", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromQuery] DateTime inicio,
                [FromQuery] DateTime fin,
                [FromServices] IPagoService service) =>
            {
                var resp = await service.ListadoPorPeriodoAsync(inicio, fin, authorization);
                return Results.Json(resp, statusCode: resp.StatusCode);
            })
            .WithName("ListadoPagosPeriodo")
            .WithOpenApi();
        }


    }
}
