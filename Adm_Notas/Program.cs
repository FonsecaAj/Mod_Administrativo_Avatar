using Adm_Direcciones.Services;
using Adm_Notas;
using Adm_Notas.Entities;
using Adm_Notas.Repository;
using Adm_Notas.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==== Conexión BD y Repositorios ====
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<RubroRepository>();
builder.Services.AddScoped<NotaRepository>();

// ==== Servicios ====
builder.Services.AddScoped<IRubroServices, RubroServices>();
builder.Services.AddScoped<INotasServices, NotaService>();

// ==== Bitácora ====
builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/"; // GEN1

// ==== Autenticación (USR5) ====
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Configuration["AutenticacionApiUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/"; // USR5

var app = builder.Build();

// Swagger UI

    app.UseSwagger();
    app.UseSwaggerUI();


// Mapear endpoints
app.MapRubrosEndpoints();

app.Run();
