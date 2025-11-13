using ACD2.Entities;
using ACD2.Repository;
using ACD2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();
builder.Services.AddHttpClient<IInstitucionService, InstitucionService>();
builder.Services.AddHttpClient<IProfesorService, ProfesorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /carrera - Crear carrera
app.MapPost("/carrera", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] CarreraDto dto,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IInstitucionService institucionService,
    IProfesorService profesorService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarCarreraAsync(dto, institucionService, profesorService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var carrera = new Carrera
    {
        Nombre = dto.Nombre.Trim(),
        IdInstitucion = dto.IdInstitucion,
        IdDirector = dto.IdDirector
    };

    var id = await repository.CrearAsync(carrera);

    // Obtener el registro completo con las fechas generadas
    var carreraCreada = await repository.ObtenerPorIdAsync(id);

    var nuevoRegistro = new
    {
        carreraCreada.IdCarrera,
        carreraCreada.Nombre,
        carreraCreada.IdInstitucion,
        carreraCreada.IdDirector,
        carreraCreada.FechaCreacion,
        carreraCreada.FechaModificacion,
        carreraCreada.Activo
    };

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Carrera creada - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/carrera/{id}", carreraCreada);
})
.WithName("CrearCarrera")
.WithOpenApi();

// PUT /carrera/{id} - Modificar carrera
app.MapPut("/carrera/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] CarreraDto dto,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IInstitucionService institucionService,
    IProfesorService profesorService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarCarreraAsync(dto, institucionService, profesorService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var carreraExistente = await repository.ObtenerPorIdAsync(id);
    if (carreraExistente == null)
        return Results.NotFound(new { error = "Carrera no encontrada" });

    var registroAnterior = new
    {
        carreraExistente.IdCarrera,
        carreraExistente.Nombre,
        carreraExistente.IdInstitucion,
        carreraExistente.IdDirector,
        carreraExistente.FechaCreacion,
        carreraExistente.FechaModificacion,
        carreraExistente.Activo
    };

    var carrera = new Carrera
    {
        IdCarrera = id,
        Nombre = dto.Nombre.Trim(),
        IdInstitucion = dto.IdInstitucion,
        IdDirector = dto.IdDirector
    };

    await repository.ActualizarAsync(carrera);

    // Obtener el registro completo actualizado
    var carreraActualizada = await repository.ObtenerPorIdAsync(id);

    var registroActual = new
    {
        carreraActualizada.IdCarrera,
        carreraActualizada.Nombre,
        carreraActualizada.IdInstitucion,
        carreraActualizada.IdDirector,
        carreraActualizada.FechaCreacion,
        carreraActualizada.FechaModificacion,
        carreraActualizada.Activo
    };

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Carrera actualizada - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(carreraActualizada);
})
.WithName("ActualizarCarrera")
.WithOpenApi();

// DELETE /carrera/{id} - Eliminar carrera
app.MapDelete("/carrera/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var carrera = await repository.ObtenerPorIdAsync(id);
    if (carrera == null)
        return Results.NotFound(new { error = "Carrera no encontrada" });

    var registroEliminado = new
    {
        carrera.IdCarrera,
        carrera.Nombre,
        carrera.IdInstitucion,
        carrera.IdDirector,
        carrera.FechaCreacion,
        carrera.FechaModificacion,
        carrera.Activo
    };

    await repository.EliminarAsync(id);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Carrera eliminada - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarCarrera")
.WithOpenApi();

// GET /carrera - Obtener todas las carreras
app.MapGet("/carrera", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var carreras = await repository.ObtenerTodosAsync();

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta carreras"
    );

    return Results.Ok(carreras);
})
.WithName("ObtenerTodasCarreras")
.WithOpenApi();

// GET /carrera/{id} - Obtener carrera por ID
app.MapGet("/carrera/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var carrera = await repository.ObtenerPorIdAsync(id);
    if (carrera == null)
        return Results.NotFound(new { error = "Carrera no encontrada" });

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta carrera"
    );

    return Results.Ok(carrera);
})
.WithName("ObtenerCarreraPorId")
.WithOpenApi();

// GET /carrera/institucion/{idInstitucion} - Obtener carreras por instituciÛn
app.MapGet("/carrera/institucion/{idInstitucion}", async (
    int idInstitucion,
    [FromHeader(Name = "Authorization")] string? authorization,
    ICarreraRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var carreras = await repository.ObtenerPorInstitucionAsync(idInstitucion);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta carreras por instituciÛn"
    );

    return Results.Ok(carreras);
})
.WithName("ObtenerCarrerasPorInstitucion")
.WithOpenApi();

app.Run();

static async Task<(bool esValido, string mensaje)> ValidarCarreraAsync(
    CarreraDto dto,
    IInstitucionService institucionService,
    IProfesorService profesorService,
    string? authorization)
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre de la carrera es requerido");

    if (!Regex.IsMatch(dto.Nombre.Trim(), @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$"))
        return (false, "El nombre de la carrera solo puede contener letras y espacios");

    if (dto.IdInstitucion <= 0)
        return (false, "La instituciÛn es requerida");

    if (!await institucionService.ValidarInstitucionAsync(dto.IdInstitucion, authorization))
        return (false, "La instituciÛn no existe");

    if (dto.IdDirector <= 0)
        return (false, "El director es requerido");

    if (!await profesorService.ValidarProfesorAsync(dto.IdDirector, authorization))
        return (false, "El director debe estar registrado como profesor");

    return (true, string.Empty);
}

record CarreraDto(string Nombre, int IdInstitucion, int IdDirector);