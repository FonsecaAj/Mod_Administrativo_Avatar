using ApiACD3;
using ApiACD3.Repository;
using ApiACD3.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://127.0.0.1:5088"); // Puerto diferente

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiACD3 - Cursos", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {tu_token}"
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

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<CursoRepository>();
builder.Services.AddScoped<ICursoService, CursoService>();

builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//if (app.Environment.IsDevelopment())

    app.UseSwagger();
    app.UseSwaggerUI();


app.MapCursoEndpoints();

app.Run();