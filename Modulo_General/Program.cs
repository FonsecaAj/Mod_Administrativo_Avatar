using Microsoft.AspNetCore.Connections;
using Avatar_Mod_Administración;
using Avatar_Mod_Administración.Repository;
using Avatar_Mod_Administración.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AGREGAR CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// USAR CORS
app.UseCors("AllowAll");

app.MapBitacoraEndpoints();
app.Run();

