using ApiMAT2.Entities;
using ApiMAT2.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiMAT2
{
    public static class MatriculaEndpoints
    {
        public static void MapMatriculaEndpoints(this WebApplication app)
        {
            // Grupo base
            var group = app.MapGroup("/api/matricula").WithTags("Matricula");

            // ================== POST: CREAR ==================
            group.MapPost("/", async (
                [FromBody] MatriculaRequest request,
                [FromServices] IMatriculaService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Crear(request);

                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 201,
                        Message = "Matrícula creada correctamente.",
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
                catch (Exception ex)
                {
                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = ex.Message,
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
            });

            // ================== PUT: ACTUALIZAR ==================
            group.MapPut("/", async (
                [FromBody] MatriculaRequest request,
                [FromServices] IMatriculaService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Actualizar(request);

                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 200,
                        Message = "Matrícula modificada correctamente.",
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
                catch (Exception ex)
                {
                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = ex.Message,
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
            });

            // ================== DELETE: ELIMINAR ==================
            group.MapDelete("/{id:int}", async (
                [FromRoute] int id,
                [FromServices] IMatriculaService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    service.Eliminar(id);

                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 200,
                        Message = "Matrícula eliminada correctamente.",
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
                catch (Exception ex)
                {
                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = ex.Message,
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
            });

            // ================== GET: LISTADO POR CURSO / GRUPO ==================
            group.MapGet("/{idCurso:int}/{idGrupo:int}", async (
                [FromRoute] int idCurso,
                [FromRoute] int idGrupo,
                [FromServices] IMatriculaService service,
                IAutenticacionService auth,
                HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                try
                {
                    var resultado = service.ObtenerPorCursoYGrupo(idCurso, idGrupo);
                    // resultado debería ser IEnumerable<MatriculaListadoDto>

                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 200,
                        Message = "Listado de matrículas obtenido correctamente.",
                        ResponseObject = resultado
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
                catch (Exception ex)
                {
                    var response = new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = ex.Message,
                        ResponseObject = null
                    };

                    return Results.Json(response, statusCode: response.StatusCode);
                }
            });

            group.MapGet("/lookups", async (
              IMatriculaService service,
              IAutenticacionService auth,
              HttpContext http) =>
            {
                var authorization = http.Request.Headers["Authorization"].ToString();
                if (!await auth.ValidarTokenAsync(authorization))
                    return Results.Unauthorized();

                var lookups = service.ObtenerLookups();

                var response = new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Catálogos de matrícula obtenidos correctamente.",
                    ResponseObject = lookups   
                };

                return Results.Json(response, statusCode: response.StatusCode);
            });


        }
    }
}
