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


builder.Configuration["BitacoraService:BaseUrl"] = builder.Configuration["BitacoraService:BaseUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/modgeneral/";
builder.Configuration["AutenticacionApiUrl"] = builder.Configuration["AutenticacionApiUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";
builder.Configuration["FacturacionServiceUrl"] = builder.Configuration["FacturacionServiceUrl"] ?? "https://tiusr20pl.cuc-carrera-ti.ac.cr/admfacturacion/";


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPagoEndpoints();
app.Run();
