using ACD1.Entities;
using ACD1.Repository;
using ACD1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IInstitucionRepository, InstitucionRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();
builder.Services.AddHttpClient<ICarreraService, CarreraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /institucion - Crear institución
app.MapPost("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var institucion = new Institucion
    {
        Nombre = dto.Nombre.Trim()
    };

    var id = await repository.CrearAsync(institucion);
    var institucionCreada = await repository.ObtenerPorIdAsync(id);

    if (institucionCreada == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar la institución creada" }, statusCode: 500);

    var nuevoRegistro = new
    {
        institucionCreada.IdInstitucion,
        institucionCreada.Nombre,
        institucionCreada.FechaCreacion,
        institucionCreada.FechaModificacion,
        institucionCreada.Activo
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(nuevoRegistro),
        "INSERT"
    );

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 201,
        Message = $"Institución '{institucionCreada.Nombre}' creada exitosamente",
        ResponseObject = institucionCreada
    }, statusCode: 201);
})
.WithName("CrearInstitucion")
.WithOpenApi();

// PUT /institucion/{id} - Modificar institución
app.MapPut("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var institucionExistente = await repository.ObtenerPorIdAsync(id);
    if (institucionExistente == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Institución no encontrada" }, statusCode: 404);

    var registroAnterior = new
    {
        institucionExistente.IdInstitucion,
        institucionExistente.Nombre,
        institucionExistente.FechaCreacion,
        institucionExistente.FechaModificacion,
        institucionExistente.Activo
    };

    var institucion = new Institucion
    {
        IdInstitucion = id,
        Nombre = dto.Nombre.Trim()
    };

    await repository.ActualizarAsync(institucion);
    var institucionActualizada = await repository.ObtenerPorIdAsync(id);

    if (institucionActualizada == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar la institución actualizada" }, statusCode: 500);

    var registroActual = new
    {
        institucionActualizada.IdInstitucion,
        institucionActualizada.Nombre,
        institucionActualizada.FechaCreacion,
        institucionActualizada.FechaModificacion,
        institucionActualizada.Activo
    };

    var cambios = new
    {
        Anterior = registroAnterior,
        Actual = registroActual
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(cambios),
        "UPDATE"
    );

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = $"Institución actualizada exitosamente a '{institucionActualizada.Nombre}'",
        ResponseObject = institucionActualizada
    }, statusCode: 200);
})
.WithName("ActualizarInstitucion")
.WithOpenApi();

// DELETE /institucion/{id} - Eliminar institución
app.MapDelete("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    ICarreraService carreraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Institución no encontrada" }, statusCode: 404);

    try
    {
        // Se llama a la API ACD2 para verificar si tiene carreras asociadas
        var cantidadCarreras = await carreraService.ContarCarrerasPorInstitucionAsync(id, authorization);

        if (cantidadCarreras > 0)
        {
            // Mensaje claro según requerimiento
            var mensaje = cantidadCarreras == 1
                ? $"No se puede eliminar la institución '{institucion.Nombre}' porque tiene 1 carrera asociada. Primero debe eliminar o reasignar la carrera."
                : $"No se puede eliminar la institución '{institucion.Nombre}' porque tiene {cantidadCarreras} carreras asociadas. Primero debe eliminar o reasignar las carreras.";

            return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = mensaje }, statusCode: 400);
        }
    }
    catch (InvalidOperationException ex)
    {
        // Si el servicio ACD2 no está disponible, retornar error claro
        return Results.Json(
            new BusinessLogicResponse { StatusCode = 503, Message = ex.Message },
            statusCode: 503);
    }

    var registroEliminado = new
    {
        institucion.IdInstitucion,
        institucion.Nombre,
        institucion.FechaCreacion,
        institucion.FechaModificacion,
        institucion.Activo
    };

    await repository.EliminarAsync(id);

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroEliminado),
        "DELETE"
    );

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = $"Institución '{institucion.Nombre}' eliminada exitosamente"
    }, statusCode: 200);
})
.WithName("EliminarInstitucion")
.WithOpenApi();

// GET /institucion - Obtener todas las instituciones (con búsqueda opcional)
app.MapGet("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromQuery] string? nombre,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    IEnumerable<Institucion> instituciones;

    // Búsqueda por nombre si se proporciona el parámetro
    if (!string.IsNullOrWhiteSpace(nombre))
    {
        instituciones = await repository.BuscarPorNombreAsync(nombre.Trim());
    }
    else
    {
        instituciones = await repository.ObtenerTodosAsync();
    }

    var resultado = new
    {
        TotalRegistros = instituciones.Count(),
        Filtro = nombre ?? "sin filtro"
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Instituciones obtenidas correctamente",
        ResponseObject = instituciones
    }, statusCode: 200);
})
.WithName("ObtenerTodasInstituciones")
.WithOpenApi();

// GET /institucion/{id} - Obtener institución por ID
app.MapGet("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Institución no encontrada" }, statusCode: 404);

    var registroConsultado = new
    {
        institucion.IdInstitucion,
        institucion.Nombre,
        institucion.FechaCreacion,
        institucion.FechaModificacion,
        institucion.Activo
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Institución obtenida correctamente",
        ResponseObject = institucion
    }, statusCode: 200);
})
.WithName("ObtenerInstitucionPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarInstitucion(InstitucionDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre de la institución es requerido");

    var nombreTrimmed = dto.Nombre.Trim();

    if (nombreTrimmed.Length < 3)
        return (false, "El nombre debe tener al menos 3 caracteres");

    if (nombreTrimmed.Length > 100)
        return (false, "El nombre no puede exceder los 100 caracteres");

    if (!Regex.IsMatch(nombreTrimmed, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        return (false, "El nombre de la institución solo puede contener letras y espacios");

    if (Regex.IsMatch(nombreTrimmed, @"\s{2,}"))
        return (false, "El nombre no puede contener espacios consecutivos");

    return (true, string.Empty);
}

record InstitucionDto(string Nombre);