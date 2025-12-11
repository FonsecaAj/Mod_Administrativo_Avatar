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


            group.MapGet("/consulta", async (
                [FromServices] IBitacoraService service,
                [FromQuery] int pagina = 1,
                [FromQuery] int porPagina = 10,
                [FromQuery] string? usuario = null,
                [FromQuery] string? tipoAccion = null,
                [FromQuery] DateTime? fechaInicio = null,
                [FromQuery] DateTime? fechaFin = null) =>
            {
                // Crear el objeto request con los parámetros del Query String
                var request = new BitacoraFiltroRequest
                {
                    Pagina = pagina,
                    PorPagina = porPagina,
                    Usuario = usuario,
                    Tipo_Accion = tipoAccion,
                    FechaDesde = fechaInicio,
                    FechaHasta = fechaFin
                };

                // Llamar al método Consultar del Service, que devuelve BusinessLogicResponse
                var response = await service.Consultar(request);

                // El Service ya maneja los status codes (200, 500)
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ConsultarBitacora")
            .WithOpenApi();
        }

            // GET /api/bitacora?usuario=email@ejemplo.com
            group.MapGet("/", async ([FromQuery] string? usuario, [FromServices] IBitacoraService service) =>
            {
                var response = await service.ObtenerTodas(usuario);
                return Results.Json(response.ResponseObject, statusCode: response.StatusCode);
            })
            .WithName("ObtenerBitacoras")
            .WithOpenApi();
        }
    }
}