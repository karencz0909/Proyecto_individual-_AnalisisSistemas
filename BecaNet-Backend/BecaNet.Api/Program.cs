using BecaNet.Api.Data;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using BecaNet.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Base de datos (SQL Server) ----
builder.Services.AddDbContext<BecaNetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BecaNetDb")));

// ---- Repositorios (patrón Repository - capa de acceso a datos) ----
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IComiteRepository, ComiteRepository>();
builder.Services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();

// ---- Servicios (capa de lógica de negocio) ----
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IComiteService, ComiteService>();
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();

// ---- Patrón Observer: notificaciones de cambio de estado de Solicitud ----
builder.Services.AddSingleton<SolicitudNotificador>();
builder.Services.AddSingleton<ISolicitudObserver, NotificacionEstudianteObserver>();
builder.Services.AddSingleton<ISolicitudObserver, RegistroAuditoriaObserver>();

// ---- API + Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BecaNet API",
        Version = "v1",
        Description = "Módulos: Solicitudes, Documentación, Comités Evaluadores y Evaluaciones (Sprint 3)"
    });
});

// ---- CORS (para que el frontend en Angular pueda consumir la API) ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Suscribimos todos los observadores registrados al notificador
var notificador = app.Services.GetRequiredService<SolicitudNotificador>();
foreach (var observador in app.Services.GetServices<ISolicitudObserver>())
{
    notificador.Suscribir(observador);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // disponible en /swagger
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // sirve los archivos de wwwroot/uploads
app.UseCors("PermitirAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
