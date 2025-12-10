using Adm_Direcciones.Services;
using Mod_Matricula;
using Mod_Matricula.Repository;
using Mod_Matricula.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB + repos
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<UbicacionesRepository>();
builder.Services.AddScoped<IUbicacionesServices, UbicacionesService>();

// Bitácora (usa IConfiguration internamente para leer BitacoraService:BaseUrl)
builder.Services.AddHttpClient<BitacoraConsumer>();

// Autenticación (usa IConfiguration para leer AutenticacionApiUrl)
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapUbicacionesEndpoints();

app.Run();
