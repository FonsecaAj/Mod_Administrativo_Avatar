using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;

namespace Avatar_Mod_Administración
{
    public static class BitacoraEndPoints
    {
        public static void MapBitacoraEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/bitacora").WithTags(nameof(Bitacora));

            // POST /api/bitacora
            group.MapPost("/", async ([FromBody] BitacoraRequest request, [FromServices] IBitacoraService service) =>
            {
                var response = await service.Registrar(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("RegistrarBitacora")
            .WithOpenApi();

            // GET /api/bitacora
            group.MapGet("/", async ([FromServices] IBitacoraService service) =>
            {
                var response = await service.ObtenerTodas();
                return Results.Json(response.ResponseObject, statusCode: response.StatusCode);
            })
            .WithName("ObtenerBitacoras")
            .WithOpenApi();
        }
    }
}