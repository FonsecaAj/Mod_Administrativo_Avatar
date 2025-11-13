using ADM_Pagos;
using ADM_Pagos.Repository;
using ADM_Pagos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<PagoRepository>();

builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Services.AddHttpClient<FacturaCliente>();


builder.Configuration["BitacoraService:BaseUrl"] = builder.Configuration["BitacoraService:BaseUrl"] ?? "http://localhost:5293";
builder.Configuration["AutenticacionApiUrl"] = builder.Configuration["AutenticacionApiUrl"] ?? "http://localhost:5233";
builder.Configuration["FacturacionServiceUrl"] = builder.Configuration["FacturacionServiceUrl"] ?? "http://localhost:5183";


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPagoEndpoints();
app.Run();
