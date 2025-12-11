using Estudiantes_Matriculados;
using Estudiantes_Matriculados.Repository;
using Estudiantes_Matriculados.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Inyección de conexión a BD =====
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// ===== Repositorios y servicios =====
builder.Services.AddScoped<ListadoRepository>();
builder.Services.AddScoped<IEstudiantesService, ListadoService>();

builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Configuration["AutenticacionApiUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";

builder.Services.AddHttpClient<BitacoraConsumer>();



var app = builder.Build();



    app.UseSwagger();
    app.UseSwaggerUI();


app.MapEstudiantesEndpoints();
app.Run();
