using Microsoft.AspNetCore.Mvc;
using Perfil_Usuario.Services;



    namespace Perfil_Usuario
    {
        public static class PerfilUsuarioEndPoints
        {
            public static void MapPerfilUsuarioEndpoints(this IEndpointRouteBuilder routes)
            {
                var group = routes.MapGroup("/api/perfil").WithTags("Perfil de Usuario");

                group.MapGet("/", async (
                    [FromHeader(Name = "Authorization")] string? token,
                    [FromQuery] string email,
                    [FromServices] IPerfilUsuarioService perfilService,
                    [FromServices] IAutenticacionService auth
                ) =>
                {
                    // 1. Validar token
                    if (!await auth.ValidarTokenAsync(token))
                        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

                    
                    var response = await perfilService.ObtenerPerfilAsync(email);

                    
                    return Results.Json(response, statusCode: response.StatusCode);

                })
                .WithName("ObtenerPerfilUsuario")
                .WithOpenApi();
            }
        }
    }


