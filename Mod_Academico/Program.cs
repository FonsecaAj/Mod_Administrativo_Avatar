using Adm_Direcciones.Services;
using Mod_Academico;
using Mod_Academico.Repository;
using Mod_Academico.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==== Conexión BD ====
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<HistorialAcademicoRepository>();
builder.Services.AddScoped<IHistorialAcademicoService, HistorialAcademicoService>();

// ==== Bitácora ====
builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293"; // GEN1

// ==== Autenticación (USR5) ====
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Configuration["AutenticacionApiUrl"] = "http://localhost:5233";

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHistorialEndpoints();
app.Run();
