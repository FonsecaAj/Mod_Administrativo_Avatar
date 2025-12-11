using Microsoft.AspNetCore.Connections;
using Avatar_Mod_Administración;
using Avatar_Mod_Administración.Repository;
using Avatar_Mod_Administración.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapBitacoraEndpoints();
app.Run();

