using Avatar_Mod_Administración.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


//INYECCIÓN DE DEPENDENCIAS PARA UN CLIENTE HTTP DE UNA VEZ CON SIGLETOWNE

builder.Services.AddHttpClient<IRubroApiClient, RubroApiClient>(client =>
{
    var baseUrl = builder.Configuration["Adm_Notas:BaseUrl"]
                  ?? throw new InvalidOperationException("Adm_Notas:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<INotaApiClient, NotaApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiciosApi:Adm_Notas"]
                  ?? throw new InvalidOperationException("Adm_Notas:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IHistorialAcademicoApiClient, HistorialAcademicoApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiciosApi:HistorialAcademico"]
        ?? throw new InvalidOperationException("Base URL de Historial no configurada");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IListadoEstudiantesApiClient, ListadoEstudiantesApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiciosApi:Listado_Estudiantes"]
        ?? throw new InvalidOperationException("Base URL de Listado de Estudiantes no configurada");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IFacturaApiClient, FacturaApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiciosApi:Adm_Facturacion"]
        ?? throw new InvalidOperationException("Base URL de Adm_Facturacion no configurada");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IPagoApiClient, PagoApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiciosApi:Pagos"]
        ?? "http://localhost:5070";
    client.BaseAddress = new Uri(baseUrl);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
