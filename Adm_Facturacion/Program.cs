using Adm_Facturacion;
using Adm_Facturacion.Repository;
using Adm_Facturacion.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<FacturaRepository>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<BitacoraConsumer>();

builder.Configuration["AutenticacionApiUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";
builder.Configuration["BitacoraService:BaseUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/";

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapFacturaEndpoints();
app.Run();
