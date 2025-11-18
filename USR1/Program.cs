using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using USR1.Entities;
using USR1.Repository;
using USR1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpClient<IRolService, RolService>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /usuario - Crear usuario
app.MapPost("/usuario", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] UsuarioDto dto,
    IUsuarioRepository repository,
    IRolService rolService,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarUsuarioAsync(dto, rolService, authorization);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var usuarioExistente = await repository.ObtenerPorEmailAsync(dto.Email);
    if (usuarioExistente != null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "El email ya está registrado" }, statusCode: 400);

    var email = dto.Email.Trim().ToLower();
    var rolAsignado = await ObtenerRolPorDominioAsync(email, dto.RolDeseado, rolService, authorization);

    if (rolAsignado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "No se pudo asignar el rol correspondiente al dominio" }, statusCode: 400);

    var usuario = new Usuario
    {
        Email = email,
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Identificacion = dto.Identificacion.Trim(),
        Nombre = dto.Nombre.Trim(),
        IdRol = rolAsignado.IdRol,
        Contrasenna = dto.Contrasenna
    };

    var emailCreado = await repository.CrearAsync(usuario);

    var usuarioCreado = await repository.ObtenerPorEmailAsync(emailCreado);

    if (usuarioCreado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar el usuario creado" }, statusCode: 500);

    var nuevoRegistro = new
    {
        usuarioCreado.Email,
        usuarioCreado.IdTipoIdentificacion,
        usuarioCreado.Identificacion,
        usuarioCreado.Nombre,
        usuarioCreado.IdRol,
        RolAsignado = rolAsignado.Nombre
    };

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(nuevoRegistro),
        "INSERT"
    );

    var response = new
    {
        usuarioCreado.Email,
        usuarioCreado.IdTipoIdentificacion,
        usuarioCreado.Identificacion,
        usuarioCreado.Nombre,
        usuarioCreado.IdRol,
        RolNombre = rolAsignado.Nombre,
        usuarioCreado.FechaCreacion,
        usuarioCreado.FechaModificacion,
        usuarioCreado.Activo
    };

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 201,
        Message = $"Usuario creado correctamente con email {emailCreado}",
        ResponseObject = response
    }, statusCode: 201);
})
.WithName("CrearUsuario")
.WithOpenApi();

// PUT /usuario/{email} - Modificar usuario
app.MapPut("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] UsuarioDto dto,
    IUsuarioRepository repository,
    IRolService rolService,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarUsuarioParaEdicionAsync(dto, rolService, authorization);
    if (!validacion.esValido)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = validacion.mensaje }, statusCode: 400);

    var usuarioExistente = await repository.ObtenerPorEmailAsync(email);
    if (usuarioExistente == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Usuario no encontrado" }, statusCode: 404);

    var registroAnterior = new
    {
        usuarioExistente.Email,
        usuarioExistente.IdTipoIdentificacion,
        usuarioExistente.Identificacion,
        usuarioExistente.Nombre,
        usuarioExistente.IdRol
    };

    var emailNuevo = dto.Email.Trim().ToLower();
    var rolAsignado = await ObtenerRolPorDominioAsync(emailNuevo, dto.RolDeseado, rolService, authorization);

    if (rolAsignado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 400, Message = "No se pudo asignar el rol correspondiente al dominio" }, statusCode: 400);

    var usuario = new Usuario
    {
        Email = email,
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Identificacion = dto.Identificacion.Trim(),
        Nombre = dto.Nombre.Trim(),
        IdRol = rolAsignado.IdRol,
        Contrasenna = string.IsNullOrWhiteSpace(dto.Contrasenna)
            ? usuarioExistente.Contrasenna
            : dto.Contrasenna
    };

    await repository.ActualizarAsync(usuario);

    var usuarioActualizado = await repository.ObtenerPorEmailAsync(email);

    if (usuarioActualizado == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 500, Message = "Error al recuperar el usuario actualizado" }, statusCode: 500);

    var registroActual = new
    {
        usuarioActualizado.Email,
        usuarioActualizado.IdTipoIdentificacion,
        usuarioActualizado.Identificacion,
        usuarioActualizado.Nombre,
        usuarioActualizado.IdRol
    };

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var cambios = new
    {
        Anterior = registroAnterior,
        Actual = registroActual,
        CambioContrasenna = !string.IsNullOrWhiteSpace(dto.Contrasenna)
    };

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(cambios),
        "UPDATE"
    );

    var response = new
    {
        usuarioActualizado.Email,
        usuarioActualizado.IdTipoIdentificacion,
        usuarioActualizado.Identificacion,
        usuarioActualizado.Nombre,
        usuarioActualizado.IdRol,
        RolNombre = rolAsignado.Nombre,
        usuarioActualizado.FechaCreacion,
        usuarioActualizado.FechaModificacion,
        usuarioActualizado.Activo
    };

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Usuario actualizado correctamente",
        ResponseObject = response
    }, statusCode: 200);
})
.WithName("ActualizarUsuario")
.WithOpenApi();

// DELETE /usuario/{email} - Eliminar usuario
app.MapDelete("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await repository.ObtenerPorEmailAsync(email);
    if (usuario == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Usuario no encontrado" }, statusCode: 404);

    var registroEliminado = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol
    };

    await repository.EliminarAsync(email);

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(registroEliminado),
        "DELETE"
    );

    // Devolver BusinessLogicResponse en lugar de NoContent()
    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Usuario eliminado exitosamente"
    }, statusCode: 200);
})
.WithName("EliminarUsuario")
.WithOpenApi();

// GET /usuario - Obtener todos los usuarios
app.MapGet("/usuario", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuarios = await repository.ObtenerTodosAsync();

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var resultado = new
    {
        TotalRegistros = usuarios.Count()
    };

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(resultado),
        "SELECT"
    );

    var response = usuarios.Select(u => new
    {
        u.Email,
        u.IdTipoIdentificacion,
        u.Identificacion,
        u.Nombre,
        u.IdRol,
        u.Contrasenna,
        u.FechaCreacion,
        u.FechaModificacion,
        u.Activo
    });

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Usuarios obtenidos correctamente",
        ResponseObject = response
    }, statusCode: 200);
})
.WithName("ObtenerTodosUsuarios")
.WithOpenApi();

// GET /usuario/{email} - Obtener usuario por email
app.MapGet("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuario = await repository.ObtenerPorEmailAsync(email);
    if (usuario == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Usuario no encontrado" }, statusCode: 404);

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var registroConsultado = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol
    };

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(registroConsultado),
        "SELECT"
    );

    var response = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol,
        usuario.Contrasenna,
        usuario.FechaCreacion,
        usuario.FechaModificacion,
        usuario.Activo
    };

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Usuario obtenido correctamente",
        ResponseObject = response
    }, statusCode: 200);
})
.WithName("ObtenerUsuarioPorEmail")
.WithOpenApi();

app.MapGet("/tipoidentificacion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IConfiguration configuration,
    IAutenticacionService autenticacionService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    using var connection = new Microsoft.Data.SqlClient.SqlConnection(
        configuration.GetConnectionString("DefaultConnection"));

    var sql = @"SELECT ID_TIPO_IDENTIFICACION as IdTipoIdentificacion, 
                       NOMBRE as Nombre 
                FROM TIPO_IDENTIFICACION 
                WHERE ID_TIPO_IDENTIFICACION = @Id";

    var tipoIdentificacion = await connection.QueryFirstOrDefaultAsync<TipoIdentificacionDto>(
        sql,
        new { Id = id });

    if (tipoIdentificacion == null)
        return Results.Json(new BusinessLogicResponse { StatusCode = 404, Message = "Tipo de identificación no encontrado" }, statusCode: 404);

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Tipo de identificación obtenido correctamente",
        ResponseObject = tipoIdentificacion
    }, statusCode: 200);
})
.WithName("ObtenerTipoIdentificacionPorId")
.WithOpenApi();

// GET /usuario/filtrar - Filtrar usuarios
app.MapGet("/usuario/filtrar", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromQuery] string? identificacion,
    [FromQuery] string? nombre,
    [FromQuery] int? tipo,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" }, statusCode: 401);

    var usuarios = await repository.FiltrarAsync(identificacion, nombre, tipo);

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var filtrosAplicados = new
    {
        Identificacion = identificacion ?? "sin filtro",
        Nombre = nombre ?? "sin filtro",
        TipoIdentificacion = tipo?.ToString() ?? "sin filtro",
        ResultadosEncontrados = usuarios.Count()
    };

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        JsonSerializer.Serialize(filtrosAplicados),
        "SELECT"
    );

    var response = usuarios.Select(u => new
    {
        u.Email,
        u.IdTipoIdentificacion,
        u.Identificacion,
        u.Nombre,
        u.IdRol,
        u.Contrasenna,
        u.FechaCreacion,
        u.FechaModificacion,
        u.Activo
    });

    return Results.Json(new BusinessLogicResponse
    {
        StatusCode = 200,
        Message = "Usuarios filtrados correctamente",
        ResponseObject = response
    }, statusCode: 200);
})
.WithName("FiltrarUsuarios")
.WithOpenApi();

app.Run();


static async Task<RolDto?> ObtenerRolPorDominioAsync(string email, string? rolDeseado, IRolService rolService, string? authorization)
{
    string nombreRolRequerido;

    if (email.EndsWith("@cuc.cr"))
    {
        nombreRolRequerido = "estudiante";
    }
    else if (email.EndsWith("@cuc.ac.cr"))
    {
        if (string.IsNullOrWhiteSpace(rolDeseado))
        {
            nombreRolRequerido = "profesor";
        }
        else
        {
            var rolLower = rolDeseado.Trim().ToLower();
            if (rolLower == "profesor" || rolLower == "administrador")
            {
                nombreRolRequerido = rolLower;
            }
            else
            {
                return null;
            }
        }
    }
    else
    {
        return null;
    }

    return await rolService.ObtenerRolPorNombreAsync(nombreRolRequerido, authorization);
}

static async Task<(bool esValido, string mensaje)> ValidarUsuarioAsync(
    UsuarioDto dto,
    IRolService rolService,
    string? authorization)
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return (false, "El email es requerido");

    var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    if (!Regex.IsMatch(dto.Email.Trim(), emailPattern))
        return (false, "El formato del email no es válido");

    var email = dto.Email.Trim().ToLower();
    if (!email.EndsWith("@cuc.cr") && !email.EndsWith("@cuc.ac.cr"))
        return (false, "El email debe pertenecer a los dominios cuc.cr o cuc.ac.cr");

    if (email.EndsWith("@cuc.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "estudiante")
            return (false, "Los usuarios con dominio cuc.cr solo pueden ser estudiantes");
    }

    if (email.EndsWith("@cuc.ac.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "profesor" && rolLower != "administrador")
            return (false, "Los usuarios con dominio cuc.ac.cr solo pueden ser profesores o administradores");
    }

    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre completo es requerido");

    if (string.IsNullOrWhiteSpace(dto.Identificacion))
        return (false, "La identificación es requerida");

    if (dto.IdTipoIdentificacion <= 0)
        return (false, "El tipo de identificación es requerido");

    if (string.IsNullOrWhiteSpace(dto.Contrasenna))
        return (false, "La contraseña es requerida");

    return (true, string.Empty);
}

static async Task<(bool esValido, string mensaje)> ValidarUsuarioParaEdicionAsync(
    UsuarioDto dto,
    IRolService rolService,
    string? authorization)
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return (false, "El email es requerido");

    var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    if (!Regex.IsMatch(dto.Email.Trim(), emailPattern))
        return (false, "El formato del email no es válido");

    var email = dto.Email.Trim().ToLower();
    if (!email.EndsWith("@cuc.cr") && !email.EndsWith("@cuc.ac.cr"))
        return (false, "El email debe pertenecer a los dominios cuc.cr o cuc.ac.cr");

    if (email.EndsWith("@cuc.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "estudiante")
            return (false, "Los usuarios con dominio cuc.cr solo pueden ser estudiantes");
    }

    if (email.EndsWith("@cuc.ac.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "profesor" && rolLower != "administrador")
            return (false, "Los usuarios con dominio cuc.ac.cr solo pueden ser profesores o administradores");
    }

    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre completo es requerido");

    if (string.IsNullOrWhiteSpace(dto.Identificacion))
        return (false, "La identificación es requerida");

    if (dto.IdTipoIdentificacion <= 0)
        return (false, "El tipo de identificación es requerido");

    if (!string.IsNullOrWhiteSpace(dto.Contrasenna) && dto.Contrasenna.Length < 6)
        return (false, "La contraseña debe tener al menos 6 caracteres");

    return (true, string.Empty);
}

record UsuarioDto(
    string Email,
    int IdTipoIdentificacion,
    string Identificacion,
    string Nombre,
    string Contrasenna,
    string? RolDeseado
);

record TipoIdentificacionDto(int IdTipoIdentificacion, string Nombre);