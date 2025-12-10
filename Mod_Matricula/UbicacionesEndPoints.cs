using Adm_Direcciones.Services;
using Microsoft.AspNetCore.Mvc;
using Mod_Matricula.Entities;
using Mod_Matricula.Services;

namespace Mod_Matricula
{
    public static class UbicacionesEndPoints
    {

        public static void MapUbicacionesEndpoints(this IEndpointRouteBuilder routes)
        {
            // ======== PROVINCIAS ========
            var provincias = routes.MapGroup("/api/provincias").WithTags(nameof(Provincia));

            provincias.MapGet("/", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromServices] IUbicacionesServices ubicacionesServices,
                [FromServices] IAutenticacionService autenticacionService) =>
            {
                if (!await autenticacionService.ValidarTokenAsync(authorization))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

                var response = await ubicacionesServices.Obtener_Provincias();

                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("GetAllProvincias")
            .WithOpenApi();

            // ======== CANTONES ========
            var cantones = routes.MapGroup("/api/cantones").WithTags(nameof(Canton));

            cantones.MapGet("/", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromQuery] string? provincia,
                [FromServices] IUbicacionesServices ubicacionesServices,
                [FromServices] IAutenticacionService autenticacionService) =>
            {
                if (!await autenticacionService.ValidarTokenAsync(authorization))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

                if (string.IsNullOrWhiteSpace(provincia))
                    return Results.Json(new { error = "Debe especificar el parámetro 'provincia'" }, statusCode: 400);

                if (!int.TryParse(provincia, out int idProvincia) || idProvincia <= 0)
                    return Results.Json(new { error = "El parámetro 'provincia' debe ser válido" }, statusCode: 400);

                var response = await ubicacionesServices.Obtener_Cantones(idProvincia);

                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("GetCantonesPorProvincia")
            .WithOpenApi();

            // ======== DISTRITOS ========
            var distritos = routes.MapGroup("/api/distritos").WithTags(nameof(Distrito));

            distritos.MapGet("/", async (
                [FromHeader(Name = "Authorization")] string? authorization,
                [FromQuery] string? provincia,
                [FromQuery] string? canton,
                [FromServices] IUbicacionesServices ubicacionesServices,
                [FromServices] IAutenticacionService autenticacionService) =>
            {
                if (!await autenticacionService.ValidarTokenAsync(authorization))
                    return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

                if (string.IsNullOrWhiteSpace(provincia) || string.IsNullOrWhiteSpace(canton))
                    return Results.Json(new { error = "Debe especificar 'provincia' y 'canton'" }, statusCode: 400);

                if (!int.TryParse(provincia, out int idProvincia) || !int.TryParse(canton, out int idCanton))
                    return Results.Json(new { error = "Parámetros no válidos" }, statusCode: 400);

                var response = await ubicacionesServices.Obtener_Distritos(idProvincia, idCanton);

                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("GetDistritosPorProvinciaYCanton")
            .WithOpenApi();
        }


    }
}
