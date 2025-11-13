using Adm_Facturacion.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adm_Facturacion
{
    public static class FacturaEndPoints
    {
        public static void MapFacturaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/factura").WithTags("Factura");

            // Crear factura
            group.MapPost("/", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromBody] string identificacion,
                [FromServices] IFacturaService service) =>
            {
                var response = await service.CrearFacturaAsync(identificacion, token);
                return Results.Json(response, statusCode: response.StatusCode);
            });

            // Reversar factura
            group.MapPut("/{idFactura}/reversar", async (
                [FromHeader(Name = "Authorization")] string? token,
                int idFactura,
                [FromServices] IFacturaService service) =>
            {
                var response = await service.ReversarFacturaAsync(idFactura, token);
                return Results.Json(response, statusCode: response.StatusCode);
            });

            // Obtener factura individual
            group.MapGet("/{idFactura}", async (
                [FromHeader(Name = "Authorization")] string? token,
                int idFactura,
                [FromServices] IFacturaService service) =>
            {
                var response = await service.ObtenerFacturaAsync(idFactura, token);
                return Results.Json(response, statusCode: response.StatusCode);
            });

            // Listado facturas por periodo
            group.MapGet("/listado", async (
                [FromHeader(Name = "Authorization")] string? token,
                [FromQuery] DateTime inicio,
                [FromQuery] DateTime fin,
                [FromServices] IFacturaService service) =>
            {
                var response = await service.ObtenerFacturasPorPeriodoAsync(inicio, fin, token);
                return Results.Json(response, statusCode: response.StatusCode);
            });
        }

    }
}
