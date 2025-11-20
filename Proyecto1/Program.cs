using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.RegularExpressions;
using USR2.Entities;
using USR2.Repository;
using USR2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IRolModuloRepository, RolModuloRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();
builder.Services.AddHttpClient<IModuloApiService, ModuloApiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// POST /rol - Crear rol
app.MapPost("/rol", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] RolDto dto,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarRol(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var rolExistente = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (rolExistente != null)
        return Results.BadRequest(new { error = "Ya existe un rol con ese nombre" });

    var rol = new Rol { Nombre = dto.Nombre.Trim() };
    var id = await repository.CrearAsync(rol);

    var rolCreado = await repository.ObtenerPorIdAsync(id);

    var nuevoRegistro = new
    {
        rolCreado.IdRol,
        rolCreado.Nombre,
        rolCreado.FechaCreacion,
        rolCreado.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(nuevoRegistro),
        "INSERT"
    );

    return Results.Created($"/rol/{id}", rolCreado);
})
.WithName("CrearRol")
.WithOpenApi();

// PUT /rol/{id} - Modificar rol
app.MapPut("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] RolDto dto,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarRol(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var rolExistente = await repository.ObtenerPorIdAsync(id);
    if (rolExistente == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    var rolDuplicado = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (rolDuplicado != null && rolDuplicado.IdRol != id)
        return Results.BadRequest(new { error = "Ya existe un rol con ese nombre" });

    var registroAnterior = new
    {
        rolExistente.IdRol,
        rolExistente.Nombre,
        rolExistente.FechaCreacion,
        rolExistente.FechaModificacion
    };

    var rol = new Rol { IdRol = id, Nombre = dto.Nombre.Trim() };
    await repository.ActualizarAsync(rol);

    var rolActualizado = await repository.ObtenerPorIdAsync(id);

    var registroActual = new
    {
        rolActualizado.IdRol,
        rolActualizado.Nombre,
        rolActualizado.FechaCreacion,
        rolActualizado.FechaModificacion
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

    return Results.Ok(rolActualizado);
})
.WithName("ActualizarRol")
.WithOpenApi();

// DELETE /rol/{id} - Eliminar rol
app.MapDelete("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await repository.ObtenerPorIdAsync(id);
    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    var registroEliminado = new
    {
        rol.IdRol,
        rol.Nombre,
        rol.FechaCreacion,
        rol.FechaModificacion
    };

    await repository.EliminarAsync(id);

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroEliminado),
        "DELETE"
    );

    return Results.NoContent();
})
.WithName("EliminarRol")
.WithOpenApi();

// GET /rol - Obtener todos los roles
app.MapGet("/rol", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var roles = await repository.ObtenerTodosAsync();

    var resultado = new
    {
        TotalRegistros = roles.Count()
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    return Results.Ok(roles);
})
.WithName("ObtenerTodosRoles")
.WithOpenApi();

// GET /rol/{id} - Obtener rol por ID
app.MapGet("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await repository.ObtenerPorIdAsync(id);

    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    var registroConsultado = new
    {
        rol.IdRol,
        rol.Nombre,
        rol.FechaCreacion,
        rol.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    return Results.Ok(rol);
})
.WithName("ObtenerRolPorId")
.WithOpenApi();


// ENDPOINTS DE PERMISOS

// GET /rol-modulo/validar-modulo/{idModulo}
app.MapGet("/rol-modulo/validar-modulo/{idModulo}", async (
    int idModulo,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolModuloRepository rolModuloRepository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    using var connection = new SqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection"));

    var sql = "SELECT COUNT(1) FROM ROL_MODULO WHERE ID_MODULO = @IdModulo";
    var count = await connection.ExecuteScalarAsync<int>(sql, new { IdModulo = idModulo });

    var resultado = new
    {
        IdModulo = idModulo,
        EstaAsignado = count > 0,
        TotalAsignaciones = count
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    return Results.Ok(new
    {
        estaAsignado = count > 0,
        totalAsignaciones = count
    });
})
.WithName("ValidarModuloAsignado")
.WithOpenApi();

// POST /rol/{idRol}/modulos - Asignar múltiples módulos a un rol (MASIVO)
app.MapPost("/rol/{idRol}/modulos", async (
    int idRol,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] AsignarModulosDto dto,
    IRolRepository rolRepository,
    IRolModuloRepository rolModuloRepository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IModuloApiService moduloApiService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await rolRepository.ObtenerPorIdAsync(idRol);
    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    // Validar módulos
    foreach (var idModulo in dto.IdsModulos)
    {
        var modulo = await moduloApiService.ObtenerPorIdAsync(idModulo, authorization);
        if (modulo == null)
            return Results.BadRequest(new { error = $"El módulo con ID {idModulo} no existe" });
    }

    // Obtener asignaciones actuales
    var asignacionesActuales = await rolModuloRepository.ObtenerPorRolAsync(idRol);
    var modulosActuales = asignacionesActuales.Select(a => a.IdModulo).ToList();

    // Calcular diferencias
    var modulosAgregar = dto.IdsModulos.Except(modulosActuales).ToList();
    var modulosEliminar = modulosActuales.Except(dto.IdsModulos).ToList();

    // Agregar nuevos
    foreach (var idModulo in modulosAgregar)
    {
        var rolModulo = new RolModulo
        {
            IdRol = idRol,
            IdModulo = idModulo
        };
        await rolModuloRepository.CrearAsync(rolModulo);
    }

    // Eliminar removidos
    foreach (var idModulo in modulosEliminar)
    {
        await rolModuloRepository.EliminarPorRolYModuloAsync(idRol, idModulo);
    }

    var cambios = new
    {
        IdRol = idRol,
        NombreRol = rol.Nombre,
        ModulosAgregados = modulosAgregar,
        ModulosEliminados = modulosEliminar,
        TotalModulos = dto.IdsModulos.Count
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(cambios),
        "INSERT"
    );

    return Results.Ok(new { mensaje = "Permisos actualizados exitosamente", cambios });
})
.WithName("AsignarModulosARol")
.WithOpenApi();

// GET /rol/{idRol}/modulos - Obtener módulos asignados a un rol
app.MapGet("/rol/{idRol}/modulos", async (
    int idRol,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository rolRepository,
    IRolModuloRepository rolModuloRepository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IModuloApiService moduloApiService,
    ILogger<Program> log) =>
{
    try
    {
        log.LogInformation("USR2: Recibida solicitud de módulos para rol {IdRol}", idRol);

        if (!string.IsNullOrEmpty(authorization))
        {
            log.LogDebug("USR2: Token recibido: {Token}",
                authorization.Substring(0, Math.Min(40, authorization.Length)) + "...");
        }

        if (!await autenticacionService.ValidarTokenAsync(authorization))
        {
            log.LogWarning("USR2: Token no válido");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";
        log.LogDebug("USR2: Usuario autenticado: {Usuario}", usuario);

        var rol = await rolRepository.ObtenerPorIdAsync(idRol);
        if (rol == null)
        {
            log.LogWarning("USR2: Rol {IdRol} no encontrado", idRol);
            return Results.NotFound(new { error = "Rol no encontrado" });
        }

        log.LogDebug("USR2: Rol encontrado: {NombreRol}", rol.Nombre);


        var asignaciones = await rolModuloRepository.ObtenerPorRolAsync(idRol);
        log.LogInformation("USR2: Encontradas {Count} asignaciones para rol {IdRol}",
            asignaciones.Count(), idRol);

        var modulos = new List<RolModuloDetalleDto>();


        foreach (var asignacion in asignaciones)
        {
            log.LogDebug("USR2: Consultando módulo {IdModulo} en USR4", asignacion.IdModulo);

            var modulo = await moduloApiService.ObtenerPorIdAsync(asignacion.IdModulo, authorization);

            if (modulo != null)
            {
                log.LogDebug("USR2: Módulo {IdModulo} obtenido: {Nombre} (Activo: {Activo})",
                    asignacion.IdModulo, modulo.Nombre, modulo.Activo);

                modulos.Add(new RolModuloDetalleDto
                {
                    IdRolModulo = asignacion.IdRolModulo,
                    IdRol = asignacion.IdRol,
                    IdModulo = asignacion.IdModulo,
                    NombreModulo = modulo.Nombre,
                    ModuloActivo = modulo.Activo,
                    FechaCreacion = asignacion.FechaCreacion,
                    FechaModificacion = asignacion.FechaModificacion
                });
            }
            else
            {
                log.LogWarning("USR2: No se pudo obtener info del módulo {IdModulo} desde USR4",
                    asignacion.IdModulo);
            }
        }

        var consulta = new
        {
            IdRol = idRol,
            NombreRol = rol.Nombre,
            TotalModulos = modulos.Count
        };

        await bitacoraService.RegistrarAsync(
            usuario,
            JsonSerializer.Serialize(consulta),
            "SELECT"
        );

        log.LogInformation("USR2: Retornando {Count} módulos para rol {IdRol}",
            modulos.Count, idRol);

        return Results.Ok(modulos);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "USR2: Error al obtener módulos para rol {IdRol}", idRol);
        return Results.Problem($"Error interno: {ex.Message}");
    }
})
.WithName("ObtenerModulosPorRol")
.WithOpenApi();

// GET /rol/{idRol}/menu - Obtener menú para construcción en ADM1
app.MapGet("/rol/{idRol}/menu", async (
    int idRol,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolModuloRepository rolModuloRepository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IModuloApiService moduloApiService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var asignaciones = await rolModuloRepository.ObtenerPorRolAsync(idRol);
    var modulos = new List<ModuloDto>();

    foreach (var asignacion in asignaciones)
    {
        var modulo = await moduloApiService.ObtenerPorIdAsync(asignacion.IdModulo);
        if (modulo != null && modulo.Activo)
        {
            modulos.Add(modulo);
        }
    }

    var consulta = new
    {
        IdRol = idRol,
        TotalModulosActivos = modulos.Count
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(consulta),
        "SELECT"
    );

    return Results.Ok(new { modulos });
})
.WithName("ObtenerMenuPorRol")
.WithOpenApi();

// DELETE /rol/{idRol}/modulos/{idModulo} - Eliminar módulo específico de un rol
app.MapDelete("/rol/{idRol}/modulos/{idModulo}", async (
    int idRol,
    int idModulo,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository rolRepository,
    IRolModuloRepository rolModuloRepository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IModuloApiService moduloApiService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await rolRepository.ObtenerPorIdAsync(idRol);
    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    var existe = await rolModuloRepository.ExisteAsignacionAsync(idRol, idModulo);
    if (!existe)
        return Results.NotFound(new { error = "Asignación no encontrada" });

    var modulo = await moduloApiService.ObtenerPorIdAsync(idModulo, authorization);

    var registroAnterior = new
    {
        IdRol = idRol,
        NombreRol = rol.Nombre,
        IdModulo = idModulo,
        NombreModulo = modulo?.Nombre ?? "Desconocido",
        Asignado = true
    };

    await rolModuloRepository.EliminarPorRolYModuloAsync(idRol, idModulo);

    var registroActual = new
    {
        IdRol = idRol,
        NombreRol = rol.Nombre,
        IdModulo = idModulo,
        NombreModulo = modulo?.Nombre ?? "Desconocido",
        Asignado = false
    };

    var cambios = new
    {
        Anterior = registroAnterior,
        Actual = registroActual
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        JsonSerializer.Serialize(cambios),
        "DELETE"
    );

    return Results.NoContent();
})
.WithName("EliminarModuloDeRol")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarRol(string nombre)
{
    if (string.IsNullOrWhiteSpace(nombre))
        return (false, "El nombre del rol es requerido");

    if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        return (false, "El nombre del rol solo puede contener letras y espacios");

    return (true, string.Empty);
}

record RolDto(string Nombre);
record AsignarModulosDto(List<int> IdsModulos);