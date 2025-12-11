using ApiMAT3.Endpoints;
using ApiMAT3.Repository;
using ApiMAT3.Services;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using System.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiMAT3", Version = "v1" });

    // Para que Swagger mande Authorization: Bearer <token>
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer {tu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
builder.Services.AddScoped<IExpedienteService, ExpedienteService>();

builder.Services.AddHttpClient<BitacoraConsumer>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BitacoraService:BaseUrl"] ?? "http://localhost:5293");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();



    app.UseSwagger();
    app.UseSwaggerUI();


app.MapExpedienteEndpoints();


app.Run();

