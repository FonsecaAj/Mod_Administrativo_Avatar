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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
            return Results.Json(new { error = "Usuario y/o contraseña incorrectos" }, statusCode: 401);

        var usuarioDto = await usuarioRepository.ValidarCredencialesAsync(usuario, contrasenna);
        if (usuarioDto == null)
        {
            return Results.Json(new { error = "Usuario y/o contraseña incorrectos" }, statusCode: 401);
        }

        await repository.DesactivarTokensUsuarioAsync(usuario);

        var jwtExpiracionMinutos = await parametroService.ObtenerTiempoExpiracionJwtAsync();
        var refreshExpiracionMinutos = await parametroService.ObtenerTiempoExpiracionRefreshAsync();

        var ahora = DateTime.UtcNow;
        var horaVencimiento = ahora.AddMinutes(jwtExpiracionMinutos);


        var jwtToken = jwtService.GenerarJwtToken(usuario, jwtExpiracionMinutos);
        var refreshToken = jwtService.GenerarRefreshToken();

        await repository.CrearJwtTokenAsync(new JwtToken
        {
            Token = jwtToken,
            UsuarioEmail = usuario,
            FechaExpiracion = horaVencimiento,
            FechaCreacion = ahora,
            Activo = true
        });

        await repository.CrearRefreshTokenAsync(new RefreshToken
        {
            Token = refreshToken,
            UsuarioEmail = usuario,
            FechaExpiracion = ahora.AddMinutes(refreshExpiracionMinutos),
            FechaCreacion = ahora,
            Activo = true
        });

        var loginInfo = new { usuario, fecha = ahora };
        await bitacoraService.RegistrarAsync(
            usuario,
            $"Usuario inició sesión - {JsonSerializer.Serialize(loginInfo)}"
        );

        Console.WriteLine($"Login exitoso para {usuario}");

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
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
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
            Console.WriteLine($"⚠Refresh token invalido o expirado");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        await repository.DesactivarRefreshTokenAsync(request.RefreshToken);
        await repository.DesactivarJwtTokensUsuarioAsync(tokenExistente.UsuarioEmail);

        var jwtExpiracionMinutos = await parametroService.ObtenerTiempoExpiracionJwtAsync();
        var refreshExpiracionMinutos = await parametroService.ObtenerTiempoExpiracionRefreshAsync();

        var nuevoJwtToken = jwtService.GenerarJwtToken(tokenExistente.UsuarioEmail, jwtExpiracionMinutos);
        var nuevoRefreshToken = jwtService.GenerarRefreshToken();

        var ahora = DateTime.UtcNow;
        var horaVencimiento = ahora.AddMinutes(jwtExpiracionMinutos);

        Console.WriteLine($"Token renovado para {tokenExistente.UsuarioEmail}. Expira en {jwtExpiracionMinutos} minutos");

        await repository.CrearJwtTokenAsync(new JwtToken
        {
            Token = nuevoJwtToken,
            UsuarioEmail = tokenExistente.UsuarioEmail,
            FechaExpiracion = horaVencimiento,
            FechaCreacion = ahora,
            Activo = true
        });

        await repository.CrearRefreshTokenAsync(new RefreshToken
        {
            Token = nuevoRefreshToken,
            UsuarioEmail = tokenExistente.UsuarioEmail,
            FechaExpiracion = ahora.AddMinutes(refreshExpiracionMinutos),
            FechaCreacion = ahora,
            Activo = true
        });

        var refreshInfo = new { usuario = tokenExistente.UsuarioEmail, fecha = ahora };
        await bitacoraService.RegistrarAsync(
            tokenExistente.UsuarioEmail,
            $"Token renovado - {JsonSerializer.Serialize(refreshInfo)}"
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
            Console.WriteLine("Token vacío en validación");
            return Results.Json(false, statusCode: 401);
        }

        bool jwtValido = jwtService.ValidarToken(request.Token);

        if (!jwtValido)
        {
            Console.WriteLine("Token JWT inválido");
            return Results.Json(false, statusCode: 401);
        }

        var tokenBd = await repository.ObtenerJwtTokenAsync(request.Token);

        if (tokenBd == null)
        {
            Console.WriteLine("Token no encontrado en BD");
            return Results.Json(false, statusCode: 401);
        }

        var ahora = DateTime.UtcNow;
        var minutosRestantes = (tokenBd.FechaExpiracion - ahora).TotalMinutes;

        if (!tokenBd.Activo || tokenBd.FechaExpiracion < ahora)
        {
            Console.WriteLine("Token inactivo o expirado");
            return Results.Json(false, statusCode: 401);
        }

        Console.WriteLine($"Token válido para {tokenBd.UsuarioEmail}");
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