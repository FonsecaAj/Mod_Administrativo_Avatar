using Adm_Notificaciones;
using Adm_Notificaciones.Service;

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

builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Services.AddHttpClient<AutenticacionService>();
builder.Services.AddScoped<NotificacionService>();

builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5155";
builder.Configuration["AutenticacionApiUrl"] = "https://tiusr20pl.cuc-carrera-ti.ac.cr/USR5Login/";

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// USAR CORS
app.UseCors("AllowAll");

app.MapNotificacionEndpoints();
app.Run();







//using Adm_Notificaciones;
//using Adm_Notificaciones.Service;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddHttpClient<BitacoraConsumer>();
//builder.Services.AddHttpClient<AutenticacionService>();
//builder.Services.AddScoped<NotificacionService>();

//builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293";
//builder.Configuration["AutenticacionApiUrl"] = "http://localhost:5233";

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.MapNotificacionEndpoints();
//app.Run();
