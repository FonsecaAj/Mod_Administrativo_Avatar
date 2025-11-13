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

// POST /institucion - Crear instituciÛn
app.MapPost("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var institucion = new Institucion
    {
        Nombre = dto.Nombre.Trim()
    };

    var id = await repository.CrearAsync(institucion);
    var institucionCreada = await repository.ObtenerPorIdAsync(id);

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

    return Results.Created($"/institucion/{id}", institucionCreada);
})
.WithName("CrearInstitucion")
.WithOpenApi();

// PUT /institucion/{id} - Modificar instituciÛn
app.MapPut("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var institucionExistente = await repository.ObtenerPorIdAsync(id);
    if (institucionExistente == null)
        return Results.NotFound(new { error = "InstituciÛn no encontrada" });

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

    return Results.Ok(institucionActualizada);
})
.WithName("ActualizarInstitucion")
.WithOpenApi();

// DELETE /institucion/{id} - Eliminar instituciÛn
app.MapDelete("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    ICarreraService carreraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.NotFound(new { error = "InstituciÛn no encontrada" });

    try
    {
        // Se llama a la API ACD2 para verificar si tiene carreras asociadas
        var cantidadCarreras = await carreraService.ContarCarrerasPorInstitucionAsync(id, authorization);

        if (cantidadCarreras > 0)
        {
            // Mensaje claro seg˙n requerimiento
            var mensaje = cantidadCarreras == 1
                ? $"No se puede eliminar la instituciÛn '{institucion.Nombre}' porque tiene 1 carrera asociada. Primero debe eliminar o reasignar la carrera."
                : $"No se puede eliminar la instituciÛn '{institucion.Nombre}' porque tiene {cantidadCarreras} carreras asociadas. Primero debe eliminar o reasignar las carreras.";

            return Results.BadRequest(new { error = mensaje });
        }
    }
    catch (InvalidOperationException ex)
    {
        // Si el servicio ACD2 no est· disponible, retornar error claro
        return Results.Json(
            new { error = ex.Message },
            statusCode: 503); // Service Unavailable
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

    return Results.NoContent();
})
.WithName("EliminarInstitucion")
.WithOpenApi();

// GET /institucion - Obtener todas las instituciones (con b˙squeda opcional)
app.MapGet("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromQuery] string? nombre,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    IEnumerable<Institucion> instituciones;

    // B˙squeda por nombre si se proporciona el par·metro
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

    return Results.Ok(instituciones);
})
.WithName("ObtenerTodasInstituciones")
.WithOpenApi();

// GET /institucion/{id} - Obtener instituciÛn por ID
app.MapGet("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.NotFound(new { error = "InstituciÛn no encontrada" });

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

    return Results.Ok(institucion);
})
.WithName("ObtenerInstitucionPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarInstitucion(InstitucionDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre de la instituciÛn es requerido");

    if (!Regex.IsMatch(dto.Nombre.Trim(), @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$"))
        return (false, "El nombre de la instituciÛn solo puede contener letras y espacios");

    return (true, string.Empty);
}

record InstitucionDto(string Nombre);