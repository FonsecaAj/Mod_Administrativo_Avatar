using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

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
});

builder.Services.AddHttpClient<IUsuarioService, UsuarioService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IRolService, RolService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IParametroService, ParametroService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IInstitucionService, InstitucionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IModuloService, ModuloService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IRubroApiClient, RubroApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Adm_Notas"]
        ?? throw new InvalidOperationException("Base URL de Adm_Notas no configurada");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<INotaApiClient, NotaApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Adm_Notas"]
        ?? throw new InvalidOperationException("Base URL de Adm_Notas no configurada");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IHistorialAcademicoApiClient, HistorialAcademicoApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:HistorialAcademico"]
        ?? throw new InvalidOperationException("Base URL de Historial no configurada");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IListadoEstudiantesApiClient, ListadoEstudiantesApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Listado_Estudiantes"]
        ?? throw new InvalidOperationException("Base URL de Listado de Estudiantes no configurada");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IFacturaApiClient, FacturaApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Adm_Facturacion"]
        ?? throw new InvalidOperationException("Base URL de Adm_Facturacion no configurada");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IPagoApiClient, PagoApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Pagos"]
        ?? "http://localhost:5070";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

});


builder.Services.AddHttpClient<INotificacionesApiClient, NotificacionesApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Notificaciones"]
        ?? throw new InvalidOperationException("Base URL de Notificaciones no configurada");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IBitacoraService, BitacoraService>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Bitacora"]
        ?? throw new InvalidOperationException("Base URL de Bitacora no configurada");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IPerfilUsuarioApiClient, PerfilUsuarioApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiUrls:ServiciosApi:Perfil_Usuario"]
        ?? throw new InvalidOperationException("Base URL de Perfil_Usuario no configurada");

});

//  Cliente HTTP para Cursos (Adm_Cursos)
builder.Services.AddHttpClient<ICursoApiClient, CursoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Cursos:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Cursos:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});


builder.Services.AddHttpClient<IProfesorApiClient, ProfesorApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Profesores:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Profesores:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});


builder.Services.AddHttpClient<IPeriodoApiClient, PeriodoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Periodos:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Periodos:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IGrupoApiClient, GrupoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Grupos:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Grupos:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IPrematriculaApiClient, PrematriculaApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Prematricula:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Prematricula:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IMatriculaApiClient, MatriculaApiClient>(client =>
{
    var baseUrl = builder.Configuration["Mat_Matricula:BaseUrl"]
                  ?? throw new InvalidOperationException("Mat_Matricula:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IExpedienteApiClient, ExpedienteApiClient>(client =>
{
    var baseUrl = builder.Configuration["Mat_Expediente:BaseUrl"]
                  ?? throw new InvalidOperationException("Mat_Expediente:BaseUrl no configurado");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IUbicacionesApiClient, UbicacionesApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Direcciones:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Direcciones:BaseUrl no configurado");

    client.BaseAddress = new Uri(baseUrl);
});


var app = builder.Build();

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

// ====================================
// NUEVO ENDPOINT: Dashboard Actividades
// ====================================
app.MapGet("/api/dashboard/actividades", async (
    IBitacoraService bitacoraService,
    IAuthService authService,
    ILogger<Program> log) =>
{
    try
    {
        var sesion = authService.ObtenerSesionActual();
        if (sesion == null)
        {
            log.LogWarning("Dashboard: No hay sesión activa");
            return Results.Json(new
            {
                notificaciones = new List<object>(),
                bitacoras = new List<object>()
            });
        }

        log.LogInformation("Dashboard: Obteniendo actividades para usuario {Usuario}", sesion.UsuarioID);

        var filtro = new BitacoraFiltroDto
        {
            Usuario = sesion.UsuarioID,
            PorPagina = 20
        };

        var todasBitacoras = await bitacoraService.ObtenerTodosAsync(sesion.AccessToken, filtro);

        if (todasBitacoras == null || !todasBitacoras.Any())
        {
            log.LogInformation("Dashboard: No hay bitácoras para usuario {Usuario}", sesion.UsuarioID);
            return Results.Ok(new
            {
                notificaciones = new List<object>(),
                bitacoras = new List<object>()
            });
        }

        var notificaciones = todasBitacoras
            .Where(b => !string.IsNullOrEmpty(b.tipo_Accion) &&
                       b.tipo_Accion.Equals("SEND_MAIL", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.fecha_Registro)
            .Take(20)
            .Select(b => new
            {
                iD_Bitacora = b.iD_Bitacora,
                fecha_Registro = b.fecha_Registro,
                usuario = b.usuario,
                descripcion = b.descripcion,
                tipo_Accion = b.tipo_Accion
            })
            .ToList();

        var bitacoras = todasBitacoras
            .Where(b => string.IsNullOrEmpty(b.tipo_Accion) ||
                       !b.tipo_Accion.Equals("SEND_MAIL", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.fecha_Registro)
            .Take(20)
            .Select(b => new
            {
                iD_Bitacora = b.iD_Bitacora,
                fecha_Registro = b.fecha_Registro,
                usuario = b.usuario,
                descripcion = b.descripcion,
                tipo_Accion = b.tipo_Accion
            })
            .ToList();

        log.LogInformation("Dashboard: Usuario {Usuario} - {NotifCount} notificaciones, {BitCount} bitácoras",
            sesion.UsuarioID, notificaciones.Count, bitacoras.Count);

        return Results.Ok(new
        {
            notificaciones,
            bitacoras
        });
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Dashboard: Error al obtener actividades");
        return Results.Ok(new
        {
            notificaciones = new List<object>(),
            bitacoras = new List<object>()
        });
    }
})
.WithName("ObtenerActividadesDashboard")
.WithTags("Dashboard")
.Produces(200);

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