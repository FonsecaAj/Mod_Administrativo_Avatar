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
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var moduloExistente = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloExistente != null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "Ya existe un módulo con ese nombre" }, statusCode: 400);

    var modulo = new Modulo
    {
        Nombre = dto.Nombre.Trim(),
        Activo = dto.Activo,
        Orden = dto.Orden
    };

    var id = await repository.CrearAsync(modulo);

    var moduloCreado = await repository.ObtenerPorIdAsync(id);

    if (moduloCreado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar el módulo creado" }, statusCode: 500);

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

    // Devolver el módulo directamente en ResponseObject
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 201,
        Message = $"Módulo '{moduloCreado.Nombre}' creado correctamente",
        ResponseObject = moduloCreado
    }, statusCode: 201);
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
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var moduloExistente = await repository.ObtenerPorIdAsync(id);
    if (moduloExistente == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Módulo no encontrado" }, statusCode: 404);

    var moduloDuplicado = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloDuplicado != null && moduloDuplicado.IdModulo != id)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "Ya existe un módulo con ese nombre" }, statusCode: 400);

    var registroAnterior = new
    {
        moduloExistente.IdModulo,
        moduloExistente.Nombre,
        moduloExistente.Activo,
        moduloExistente.Orden,
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

    if (moduloActualizado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar el módulo actualizado" }, statusCode: 500);

    var registroActual = new
    {
        moduloActualizado.IdModulo,
        moduloActualizado.Nombre,
        moduloActualizado.Activo,
        moduloActualizado.Orden,
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

    // Devolver el módulo directamente en ResponseObject
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Módulo actualizado correctamente",
        ResponseObject = moduloActualizado
    }, statusCode: 200);
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
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);
    if (modulo == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Módulo no encontrado" }, statusCode: 404);

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
                return Results.Json(new BusinessLogicResponse
                {
                    StatusCode = 400,
                    Message = "No se puede eliminar el módulo porque está asignado a uno o más roles"
                }, statusCode: 400);
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
        modulo.Orden,
        modulo.FechaCreacion,
        modulo.FechaModificacion
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
        Message = "Módulo eliminado exitosamente"
    }, statusCode: 200);
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
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

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

    // Devolver los módulos DIRECTAMENTE
    // El menú dinámico espera un array de módulos
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Módulos obtenidos correctamente",
        ResponseObject = modulos
    }, statusCode: 200);
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
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);

    if (modulo == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Módulo no encontrado" }, statusCode: 404);

    var registroConsultado = new
    {
        modulo.IdModulo,
        modulo.Nombre,
        modulo.Activo,
        modulo.Orden,
        modulo.FechaCreacion,
        modulo.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    // Devolver el módulo directamente en ResponseObject
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Módulo obtenido correctamente",
        ResponseObject = modulo
    }, statusCode: 200);
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