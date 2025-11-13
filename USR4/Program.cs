using USR4.Entities;
using USR4.Repository;
using USR4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/modulo", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ModuloDto dto,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var moduloExistente = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloExistente != null)
        return Results.BadRequest(new { error = "Ya existe un módulo con ese nombre" });

    var modulo = new Modulo
    {
        Nombre = dto.Nombre.Trim(),
        Activo = dto.Activo,
        Orden = dto.Orden
    };

    var id = await repository.CrearAsync(modulo);

    var moduloCreado = await repository.ObtenerPorIdAsync(id);

    var nuevoRegistro = new
    {
        moduloCreado.IdModulo,
        moduloCreado.Nombre,
        moduloCreado.Activo,
        moduloCreado.Orden,
        moduloCreado.FechaCreacion,
        moduloCreado.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(nuevoRegistro),
        "INSERT"
    );

    return Results.Created($"/modulo/{id}", moduloCreado);
})
.WithName("CrearModulo")
.WithOpenApi();


app.MapPut("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ModuloDto dto,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var moduloExistente = await repository.ObtenerPorIdAsync(id);
    if (moduloExistente == null)
        return Results.NotFound(new { error = "Módulo no encontrado" });

    var moduloDuplicado = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloDuplicado != null && moduloDuplicado.IdModulo != id)
        return Results.BadRequest(new { error = "Ya existe un módulo con ese nombre" });

    var registroAnterior = new
    {
        moduloExistente.IdModulo,
        moduloExistente.Nombre,
        moduloExistente.Activo,
        moduloExistente.FechaCreacion,
        moduloExistente.FechaModificacion
    };

    var modulo = new Modulo
    {
        IdModulo = id,
        Nombre = dto.Nombre.Trim(),
        Activo = dto.Activo,
        Orden = dto.Orden
    };

    await repository.ActualizarAsync(modulo);

    var moduloActualizado = await repository.ObtenerPorIdAsync(id);

    var registroActual = new
    {
        moduloActualizado.IdModulo,
        moduloActualizado.Nombre,
        moduloActualizado.Activo,
        moduloActualizado.FechaCreacion,
        moduloActualizado.FechaModificacion
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

    return Results.Ok(moduloActualizado);
})
.WithName("ActualizarModulo")
.WithOpenApi();


app.MapDelete("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IConfiguration configuration) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);
    if (modulo == null)
        return Results.NotFound(new { error = "Módulo no encontrado" });

    // Validar si el módulo está asignado a algún rol
    try
    {
        var usr2ApiUrl = configuration["USR2ApiUrl"] ?? "http://localhost:5204";
        using var httpClient = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{usr2ApiUrl}/rol-modulo/validar-modulo/{id}");
        request.Headers.Add("Authorization", authorization);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var validacion = JsonSerializer.Deserialize<JsonElement>(content);

            if (validacion.TryGetProperty("estaAsignado", out var estaAsignado) &&
                estaAsignado.GetBoolean())
            {
                return Results.BadRequest(new
                {
                    error = "No se puede eliminar el módulo porque está asignado a uno o más roles"
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al validar asignaciones: {ex.Message}");
    }

    var registroEliminado = new
    {
        modulo.IdModulo,
        modulo.Nombre,
        modulo.Activo,
        modulo.FechaCreacion,
        modulo.FechaModificacion
    };

    await repository.EliminarAsync(id);

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroEliminado),
        "DELETE"
    );

    return Results.NoContent();
})
.WithName("EliminarModulo")
.WithOpenApi();

app.MapGet("/modulo", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromQuery] string? nombre,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulos = await repository.ObtenerTodosAsync(nombre);

    var resultado = new
    {
        TotalRegistros = modulos.Count(),
        Filtro = nombre ?? "sin filtro"
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    return Results.Ok(modulos);
})
.WithName("ObtenerTodosModulos")
.WithOpenApi();


app.MapGet("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);

    if (modulo == null)
        return Results.NotFound(new { error = "Módulo no encontrado" });

    var registroConsultado = new
    {
        modulo.IdModulo,
        modulo.Nombre,
        modulo.Activo,
        modulo.FechaCreacion,
        modulo.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    return Results.Ok(modulo);
})
.WithName("ObtenerModuloPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarModulo(string nombre)
{
    if (string.IsNullOrWhiteSpace(nombre))
        return (false, "El nombre del módulo es requerido");

    if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        return (false, "El nombre del módulo solo puede contener letras y espacios");

    if (nombre.Trim().Length < 3)
        return (false, "El nombre debe tener al menos 3 caracteres");

    if (nombre.Trim().Length > 100)
        return (false, "El nombre no puede exceder los 100 caracteres");

    if (Regex.IsMatch(nombre.Trim(), @"\s{2,}"))
        return (false, "El nombre no puede contener espacios consecutivos");

    return (true, string.Empty);
}

record ModuloDto(string Nombre, bool Activo, int Orden);