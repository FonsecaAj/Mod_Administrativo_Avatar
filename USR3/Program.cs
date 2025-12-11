using USR3.Entities;
using USR3.Repository;
using USR3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// POST /parametro - Crear parámetro
app.MapPost("/parametro", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ParametroDto dto,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarParametro(dto.IdParametro, dto.Valor);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var parametroExistente = await repository.ObtenerPorIdAsync(dto.IdParametro.Trim().ToUpper());
    if (parametroExistente != null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "El parámetro ya existe" }, statusCode: 400);

    var parametro = new Parametro
    {
        IdParametro = dto.IdParametro.Trim().ToUpper(),
        Valor = dto.Valor.Trim()
    };
    await repository.CrearAsync(parametro);

    var parametroCreado = await repository.ObtenerPorIdAsync(parametro.IdParametro);

    var nuevoRegistro = new
    {
        parametroCreado.IdParametro,
        parametroCreado.Valor,
        parametroCreado.FechaCreacion,
        parametroCreado.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(nuevoRegistro),
        "INSERT"
    );

    // Devolver BusinessLogicResponse
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 201,
        Message = $"Parámetro creado correctamente: {parametroCreado.IdParametro}",
        ResponseObject = parametroCreado
    }, statusCode: 201);
})
.WithName("CrearParametro")
.WithOpenApi();

// PUT /parametro/{id} - Modificar parámetro
app.MapPut("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ParametroDto dto,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarParametro(dto.IdParametro, dto.Valor);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    if (id.ToUpper() != dto.IdParametro.Trim().ToUpper())
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "El ID del parámetro no coincide" }, statusCode: 400);

    var parametroExistente = await repository.ObtenerPorIdAsync(id.ToUpper());
    if (parametroExistente == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Parámetro no encontrado" }, statusCode: 404);

    var registroAnterior = new
    {
        parametroExistente.IdParametro,
        parametroExistente.Valor,
        parametroExistente.FechaCreacion,
        parametroExistente.FechaModificacion
    };

    var parametro = new Parametro
    {
        IdParametro = dto.IdParametro.Trim().ToUpper(),
        Valor = dto.Valor.Trim()
    };
    await repository.ActualizarAsync(parametro);

    var parametroActualizado = await repository.ObtenerPorIdAsync(parametro.IdParametro);

    var registroActual = new
    {
        parametroActualizado.IdParametro,
        parametroActualizado.Valor,
        parametroActualizado.FechaCreacion,
        parametroActualizado.FechaModificacion
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

    // Devolver BusinessLogicResponse
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Parámetro actualizado correctamente",
        ResponseObject = parametroActualizado
    }, statusCode: 200);
})
.WithName("ActualizarParametro")
.WithOpenApi();

// DELETE /parametro/{id} - Eliminar parámetro
app.MapDelete("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var parametro = await repository.ObtenerPorIdAsync(id.ToUpper());
    if (parametro == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Parámetro no encontrado" }, statusCode: 404);

    var registroEliminado = new
    {
        parametro.IdParametro,
        parametro.Valor,
        parametro.FechaCreacion,
        parametro.FechaModificacion
    };

    await repository.EliminarAsync(id.ToUpper());

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroEliminado),
        "DELETE"
    );

    // Devolver BusinessLogicResponse en lugar de NoContent()
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Parámetro eliminado exitosamente"
    }, statusCode: 200);
})
.WithName("EliminarParametro")
.WithOpenApi();

// GET /parametro - Obtener todos los parámetros CON PAGINACIÓN
app.MapGet("/parametro", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    [FromQuery] int pagina = 1,
    [FromQuery] int porPagina = 30) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    if (pagina < 1) pagina = 1;
    if (porPagina < 1 || porPagina > 100) porPagina = 30;

    var total = await repository.ObtenerTotalAsync();
    var parametros = await repository.ObtenerPaginadosAsync(pagina, porPagina);
    var totalPaginas = (int)Math.Ceiling(total / (double)porPagina);

    var resultado = new
    {
        TotalRegistros = total,
        Pagina = pagina,
        PorPagina = porPagina,
        TotalPaginas = totalPaginas
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    return Results.Ok(new
    {
        datos = parametros,
        paginacion = resultado
    });
})
.WithName("ObtenerTodosParametros")
.WithOpenApi();

// GET /parametro/{id} - Obtener parámetro por ID
app.MapGet("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var parametro = await repository.ObtenerPorIdAsync(id.ToUpper());

    if (parametro == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    var registroConsultado = new
    {
        parametro.IdParametro,
        parametro.Valor,
        parametro.FechaCreacion,
        parametro.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    return Results.Ok(parametro);
})
.WithName("ObtenerParametroPorId")
.WithOpenApi();

// GET /parametro/public/{id} - Endpoint público
app.MapGet("/parametro/public/{id}", async (
    string id,
    IParametroRepository repository) =>
{
    var parametrosPermitidos = new[] { "JWTEXPMIN", "REFEXPMIN" };

    var idUpper = id.ToUpper();
    if (!parametrosPermitidos.Contains(idUpper))
        return Results.NotFound(new { error = "Parámetro no disponible públicamente" });

    var parametro = await repository.ObtenerPorIdAsync(idUpper);

    if (parametro == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    return Results.Ok(parametro);
})
.WithName("ObtenerParametroPublico")
.WithOpenApi()
.WithTags("Público");

app.Run();

static (bool esValido, string mensaje) ValidarParametro(string idParametro, string valor)
{
    if (string.IsNullOrWhiteSpace(idParametro))
        return (false, "El identificador del parámetro es requerido");

    if (string.IsNullOrWhiteSpace(valor))
        return (false, "El valor del parámetro es requerido");

    var idTrimmed = idParametro.Trim();

    if (idTrimmed.Length > 10)
        return (false, "El identificador del parámetro no puede exceder 10 caracteres");

    if (!Regex.IsMatch(idTrimmed, @"^[A-Z_]+$"))
        return (false, "El identificador del parámetro solo puede contener letras en mayúscula y guiones bajos");

    if (valor.Trim().Length > 500)
        return (false, "El valor del parámetro no puede exceder 500 caracteres");

    return (true, string.Empty);
}

record ParametroDto(string IdParametro, string Valor);