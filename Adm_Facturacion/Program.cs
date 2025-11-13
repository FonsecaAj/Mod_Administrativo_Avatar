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

builder.Configuration["AutenticacionApiUrl"] = "http://localhost:5233";
builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293";

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapFacturaEndpoints();
app.Run();
