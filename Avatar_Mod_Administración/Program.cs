using Avatar_Mod_Administración.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IUsuarioService, UsuarioService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IRolService, RolService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IParametroService, ParametroService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IInstitucionService, InstitucionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IModuloService, ModuloService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Agregar compresión de respuestas
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Usar compresión
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// GET /api/rol/{idRol}/modulos
app.MapGet("/api/rol/{idRol}/modulos", async (
    int idRol,
    HttpContext context,
    IRolService rolService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        log.LogInformation("Solicitud de módulos para rol {IdRol}", idRol);

        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        log.LogDebug("Sesion encontrada para: {UsuarioID}", sesion.UsuarioID);

        string token;
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            token = authHeader.ToString();
        }
        else
        {
            token = sesion.AccessToken;
        }

        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = $"Bearer {token}";
        }

        // Eliminar substring innecesario
        log.LogDebug("Token obtenido correctamente");

        var modulos = await rolService.ObtenerModulosPorRolAsync(idRol, token);

        if (modulos == null)
        {
            log.LogError("Error al obtener módulos de USR2");
            return Results.Problem("Error al comunicarse con USR2", statusCode: 503);
        }

        if (modulos.Count == 0)
        {
            log.LogWarning("No hay módulos para rol {IdRol}", idRol);
            return Results.Ok(new List<object>());
        }

        log.LogInformation("Obtenidos {Count} módulos para rol {IdRol}", modulos.Count, idRol);

        return Results.Ok(modulos);
    }
    catch (HttpRequestException httpEx)
    {
        log.LogError(httpEx, "Error de conexion con USR2");
        return Results.Problem($"Error de conexión: {httpEx.Message}", statusCode: 503);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al obtener modulos");
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ObtenerModulosPorRol")
.WithTags("Menu");

// GET /api/sesion/verificar
app.MapGet("/api/sesion/verificar", async (
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();

        if (sesion == null)
        {
            log.LogWarning("No hay sesion activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var renovado = await authService.RenovarSesionAutomaticaAsync();

        if (!renovado)
        {
            log.LogWarning("No se pudo renovar el token");
            return Results.Json(new { error = "Sesión expirada" }, statusCode: 401);
        }

        log.LogDebug("Sesion valida para: {UsuarioID}", sesion.UsuarioID);

        return Results.Ok(new
        {
            mensaje = "Sesion válida",
            usuario = sesion.UsuarioID
        });
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al verificar sesion");
        return Results.Json(new { error = "Error interno" }, statusCode: 500);
    }
})
.WithName("VerificarSesion")
.WithTags("Sesion");

// POST /api/bitacora
app.MapPost("/api/bitacora", async (
    HttpContext context,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión para registrar bitácora");
            return Results.StatusCode(401);
        }

        using var reader = new StreamReader(context.Request.Body);
        var bodyOriginal = await reader.ReadToEndAsync();

        log.LogDebug("Body recibido");

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        var gen1Url = builder.Configuration["GEN1ApiUrl"] ?? "http://localhost:5155";

        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(bodyOriginal);
            var root = jsonDoc.RootElement;

            var usuario = root.TryGetProperty("usuario", out var usuarioElem)
                ? usuarioElem.GetString()
                : sesion.UsuarioID;

            var descripcion = root.TryGetProperty("descripcion", out var descElem)
                ? descElem.GetString()
                : "{\"accion\":\"Sin descripción\"}";

            var payloadGEN1 = new
            {
                usuario = usuario,
                descripcion = descripcion
            };

            var jsonGEN1 = System.Text.Json.JsonSerializer.Serialize(payloadGEN1);

            log.LogDebug("Enviando a GEN1");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{gen1Url}/api/bitacora");
            request.Content = new StringContent(jsonGEN1, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                log.LogDebug("Bitacora registrada");
                return Results.StatusCode(201);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            log.LogWarning("Error de GEN1: {StatusCode} - {Error}",
                response.StatusCode, errorContent);
            return Results.StatusCode((int)response.StatusCode);
        }
        catch (System.Text.Json.JsonException ex)
        {
            log.LogError(ex, "Error al parsear JSON");
            return Results.BadRequest(new { error = "JSON inválido" });
        }
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al registrar bitácora");
        return Results.StatusCode(500);
    }
})
.WithName("RegistrarBitacora")
.WithTags("Bitacora");

// GET /api/modulos
app.MapGet("/api/modulos", async (
    IModuloService moduloService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var modulos = await moduloService.ObtenerTodosAsync(sesion.AccessToken);

        log.LogInformation("Obtenidos {Count} módulos", modulos.Count);

        return Results.Ok(modulos);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al obtener módulos");
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ObtenerModulos")
.WithTags("Menu");

// GET /api/modulos/{idModulo}
app.MapGet("/api/modulos/{idModulo}", async (
    int idModulo,
    IModuloService moduloService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var modulo = await moduloService.ObtenerPorIdAsync(idModulo, sesion.AccessToken);

        if (modulo == null)
        {
            log.LogWarning("Módulo {IdModulo} no encontrado", idModulo);
            return Results.NotFound(new { error = "Módulo no encontrado" });
        }

        log.LogDebug("Módulo {IdModulo} obtenido", idModulo);

        return Results.Ok(modulo);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al obtener módulo {IdModulo}", idModulo);
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ObtenerModulo")
.WithTags("Menu");

// GET /api/roles
app.MapGet("/api/roles", async (
    IRolService rolService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var roles = await rolService.ObtenerTodosAsync(sesion.AccessToken);

        log.LogInformation("Obtenidos {Count} roles", roles.Count);

        return Results.Ok(roles);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al obtener roles");
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ObtenerRoles")
.WithTags("Menu");

// GET /api/permisos/{idRol}
app.MapGet("/api/permisos/{idRol}", async (
    int idRol,
    IRolService rolService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        log.LogInformation("Solicitando permisos para rol {IdRol}", idRol);

        if (idRol == 0)
        {
            log.LogWarning("Intento de obtener permisos para rol 0");
            return Results.BadRequest(new { error = "ID de rol inválido" });
        }

        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("No hay sesión activa");
            return Results.Json(new { error = "No autorizado" }, statusCode: 401);
        }

        var permisos = await rolService.ObtenerModulosPorRolAsync(idRol, sesion.AccessToken);

        log.LogInformation("Obtenidos {Count} permisos para rol {IdRol}", permisos.Count, idRol);

        return Results.Ok(permisos);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error al obtener permisos del rol {IdRol}", idRol);
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ObtenerPermisos")
.WithTags("Permisos");

app.Run();