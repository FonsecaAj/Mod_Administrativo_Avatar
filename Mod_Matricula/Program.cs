using Adm_Direcciones.Services;
using Mod_Matricula;
using Mod_Matricula.Repository;
using Mod_Matricula.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<UbicacionesRepository>();
builder.Services.AddScoped<IUbicacionesServices, UbicacionesService>();

builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293"; // URL del servicio GEN1

builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Configuration["AutenticacionApiUrl"] = "http://localhost:5233";

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapUbicacionesEndpoints();
app.Run();

