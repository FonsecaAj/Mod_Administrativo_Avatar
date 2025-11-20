using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using USR5.Entities;
using USR5.Repository;
using USR5.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddScoped<IAutenticacionRepository, AutenticacionRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpClient<IParametroService, ParametroService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

// POST /login - Iniciar sesión
app.MapPost("/login", async (
    [FromHeader(Name = "usuario")] string? usuario,
    [FromHeader(Name = "contrasenna")] string? contrasenna,
    IUsuarioRepository usuarioRepository,
    IParametroService parametroService,
    IJwtService jwtService,
    IAutenticacionRepository repository,
    IBitacoraService bitacoraService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasenna))
        {
            // Bitácora en background
            _ = bitacoraService.RegistrarAsync(
                usuario ?? "desconocido",
                JsonSerializer.Serialize(new
                {
                    Usuario = usuario ?? "desconocido",
                    Motivo = "Credenciales vacías",
                    Fecha = DateTime.UtcNow,
                    Exitoso = false
                }),
                "GENERICA"
            );

            return Results.Json(new { error = "Usuario y/o contraseña incorrectos" }, statusCode: 401);
        }

        var usuarioDto = await usuarioRepository.ValidarCredencialesAsync(usuario, contrasenna);
        if (usuarioDto == null)
        {
            // Bitácora en background
            _ = bitacoraService.RegistrarAsync(
                usuario,
                JsonSerializer.Serialize(new
                {
                    Usuario = usuario,
                    Motivo = "Credenciales incorrectas",
                    Fecha = DateTime.UtcNow,
                    Exitoso = false
                }),
                "GENERICA"
            );

            return Results.Json(new { error = "Usuario y/o contraseña incorrectos" }, statusCode: 401);
        }

        // Ejecutar operaciones en paralelo
        var ahora = DateTime.UtcNow;

        var desactivarTask = repository.DesactivarTokensUsuarioAsync(usuario);
        var jwtExpTask = parametroService.ObtenerTiempoExpiracionJwtAsync();
        var refreshExpTask = parametroService.ObtenerTiempoExpiracionRefreshAsync();

        await Task.WhenAll(desactivarTask, jwtExpTask, refreshExpTask);

        var jwtExpiracionMinutos = await jwtExpTask;
        var refreshExpiracionMinutos = await refreshExpTask;
        var horaVencimiento = ahora.AddMinutes(jwtExpiracionMinutos);

        var jwtToken = jwtService.GenerarJwtToken(usuario, jwtExpiracionMinutos);
        var refreshToken = jwtService.GenerarRefreshToken();

        // Crear ambos tokens en paralelo
        await Task.WhenAll(
            repository.CrearJwtTokenAsync(new JwtToken
            {
                Token = jwtToken,
                UsuarioEmail = usuario,
                FechaExpiracion = horaVencimiento,
                FechaCreacion = ahora,
                Activo = true
            }),
            repository.CrearRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UsuarioEmail = usuario,
                FechaExpiracion = ahora.AddMinutes(refreshExpiracionMinutos),
                FechaCreacion = ahora,
                Activo = true
            })
        );

        // Bitácora en background
        _ = bitacoraService.RegistrarAsync(
            usuario,
            JsonSerializer.Serialize(new
            {
                Usuario = usuario,
                Fecha = ahora,
                ExpiracionJwt = horaVencimiento,
                Exitoso = true
            }),
            "GENERICA"
        );

        return Results.Created("/login", new LoginResponse
        {
            ExpiresIn = horaVencimiento,
            AccessToken = jwtToken,
            RefreshToken = refreshToken,
            UsuarioId = usuario
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en login: {ex.Message}");
        return Results.Json(new { error = "Error interno del servidor" }, statusCode: 500);
    }
})
.WithName("Login")
.WithOpenApi();

// POST /refresh - Renovar token
app.MapPost("/refresh", async (
    [FromBody] RefreshRequest request,
    IParametroService parametroService,
    IJwtService jwtService,
    IAutenticacionRepository repository,
    IBitacoraService bitacoraService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);

        var tokenExistente = await repository.ObtenerRefreshTokenAsync(request.RefreshToken);

        if (tokenExistente == null || !tokenExistente.Activo || tokenExistente.FechaExpiracion < DateTime.UtcNow)
        {
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        // Operaciones en paralelo
        var ahora = DateTime.UtcNow;

        var desactivarRefreshTask = repository.DesactivarRefreshTokenAsync(request.RefreshToken);
        var desactivarJwtTask = repository.DesactivarJwtTokensUsuarioAsync(tokenExistente.UsuarioEmail);
        var jwtExpTask = parametroService.ObtenerTiempoExpiracionJwtAsync();
        var refreshExpTask = parametroService.ObtenerTiempoExpiracionRefreshAsync();

        await Task.WhenAll(desactivarRefreshTask, desactivarJwtTask, jwtExpTask, refreshExpTask);

        var jwtExpiracionMinutos = await jwtExpTask;
        var refreshExpiracionMinutos = await refreshExpTask;

        var nuevoJwtToken = jwtService.GenerarJwtToken(tokenExistente.UsuarioEmail, jwtExpiracionMinutos);
        var nuevoRefreshToken = jwtService.GenerarRefreshToken();
        var horaVencimiento = ahora.AddMinutes(jwtExpiracionMinutos);

        // Crear tokens en paralelo
        await Task.WhenAll(
            repository.CrearJwtTokenAsync(new JwtToken
            {
                Token = nuevoJwtToken,
                UsuarioEmail = tokenExistente.UsuarioEmail,
                FechaExpiracion = horaVencimiento,
                FechaCreacion = ahora,
                Activo = true
            }),
            repository.CrearRefreshTokenAsync(new RefreshToken
            {
                Token = nuevoRefreshToken,
                UsuarioEmail = tokenExistente.UsuarioEmail,
                FechaExpiracion = ahora.AddMinutes(refreshExpiracionMinutos),
                FechaCreacion = ahora,
                Activo = true
            })
        );

        // Bitácora en background
        _ = bitacoraService.RegistrarAsync(
            tokenExistente.UsuarioEmail,
            JsonSerializer.Serialize(new
            {
                Usuario = tokenExistente.UsuarioEmail,
                Fecha = ahora,
                ExpiracionJwt = horaVencimiento,
                Accion = "Token renovado"
            }),
            "GENERICA"
        );

        return Results.Created("/refresh", new RefreshResponse
        {
            ExpiresIn = horaVencimiento,
            AccessToken = nuevoJwtToken,
            RefreshToken = nuevoRefreshToken
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en refresh: {ex.Message}");
        return Results.Json(new { error = "Error interno del servidor" }, statusCode: 500);
    }
})
.WithName("Refresh")
.WithOpenApi();

// POST /validate - Validar token
app.MapPost("/validate", async (
    [FromBody] ValidateRequest request,
    IJwtService jwtService,
    IAutenticacionRepository repository) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Results.Json(false, statusCode: 401);
        }

        // Validar JWT y consultar BD en paralelo
        var jwtValidoTask = Task.Run(() => jwtService.ValidarToken(request.Token));
        var tokenBdTask = repository.ObtenerJwtTokenAsync(request.Token);

        await Task.WhenAll(jwtValidoTask, tokenBdTask);

        var jwtValido = await jwtValidoTask;
        var tokenBd = await tokenBdTask;

        if (!jwtValido || tokenBd == null)
        {
            return Results.Json(false, statusCode: 401);
        }

        var ahora = DateTime.UtcNow;

        if (!tokenBd.Activo || tokenBd.FechaExpiracion < ahora)
        {
            return Results.Json(false, statusCode: 401);
        }

        return Results.Ok(true);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en validate: {ex.Message}");
        return Results.Json(false, statusCode: 401);
    }
})
.WithName("Validate")
.WithOpenApi();

// POST /logout - Cerrar sesión
app.MapPost("/logout", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IAutenticacionRepository repository,
    IJwtService jwtService,
    IBitacoraService bitacoraService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var token = authorization.Replace("Bearer ", "").Trim();

        if (!jwtService.ValidarToken(token))
        {
            return Results.Json(new { error = "Token inválido" }, statusCode: 401);
        }

        var tokenBd = await repository.ObtenerJwtTokenAsync(token);
        if (tokenBd == null)
        {
            return Results.Json(new { error = "Token no encontrado" }, statusCode: 401);
        }

        var usuario = tokenBd.UsuarioEmail;

        // Desactivar tokens
        await repository.DesactivarTokensUsuarioAsync(usuario);

        // Bitácora en background
        _ = bitacoraService.RegistrarAsync(
            usuario,
            JsonSerializer.Serialize(new
            {
                Usuario = usuario,
                Fecha = DateTime.UtcNow,
                Accion = "Cierre de sesion exitoso"
            }),
            "GENERICA"
        );

        return Results.Ok(new { mensaje = "Sesión cerrada exitosamente" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en logout: {ex.Message}");
        return Results.Json(new { error = "Error interno del servidor" }, statusCode: 500);
    }
})
.WithName("Logout")
.WithOpenApi();

app.Run();

record RefreshRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);

record ValidateRequest(
    [property: JsonPropertyName("token")] string Token
);

record LoginResponse
{
    [JsonPropertyName("expires_in")]
    public DateTime ExpiresIn { get; init; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    [JsonPropertyName("usuarioID")]
    public string UsuarioId { get; init; } = string.Empty;
}

record RefreshResponse
{
    [JsonPropertyName("expires_in")]
    public DateTime ExpiresIn { get; init; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;
}