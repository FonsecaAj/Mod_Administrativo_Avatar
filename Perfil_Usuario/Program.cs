using Perfil_Usuario;
using Perfil_Usuario.Repository;
using Perfil_Usuario.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Swagger
// =======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
// Conexión BD
// =======================
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<PerfilUsuarioRepository>();
builder.Services.AddScoped<IPerfilUsuarioService, PerfilUsuarioService>();

// =======================
// Bitácora (GEN1)
// =======================
builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] =
    "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/";

// =======================
// Autenticación (USR5)
// =======================
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Configuration["AutenticacionApiUrl"] =
    "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";

var app = builder.Build();

// =======================
// Swagger
// =======================
app.UseSwagger();
app.UseSwaggerUI();

// =======================
// Endpoints
// =======================
app.MapPerfilUsuarioEndpoints();

app.Run();
