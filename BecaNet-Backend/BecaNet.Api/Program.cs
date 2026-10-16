using System.Text;
using BecaNet.Api.Data;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Base de Datos SQL Server ----
builder.Services.AddDbContext<BecaNetDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BecaNetDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ---- JWT ----
builder.Services.AddSingleton<JwtService>();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var claveSecret = jwtConfig["Secret"] ?? jwtConfig["Key"] ?? "TuClaveSecretaSuperSeguraBecaNet2026_JWT_Token_Key_Long_String";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"] ?? "BecaNet.Api",
            ValidAudience = jwtConfig["Audience"] ?? "BecaNet.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecret))
        };
    });

builder.Services.AddAuthorization();

// ---- Repositorios (patrón Repository) ----
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IConvocatoriaRepository, ConvocatoriaRepository>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IComiteRepository, ComiteRepository>();
builder.Services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();

// ---- Servicios (lógica de negocio) ----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IConvocatoriaService, ConvocatoriaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IComiteService, ComiteService>();
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// ---- Patrón Observer ----
builder.Services.AddSingleton<SolicitudNotificador>();
builder.Services.AddSingleton<ISolicitudObserver, NotificacionEstudianteObserver>();
builder.Services.AddSingleton<ISolicitudObserver, RegistroAuditoriaObserver>();

var app = builder.Build();

// Suscribimos los observadores al notificador
var notificador = app.Services.GetRequiredService<SolicitudNotificador>();
foreach (var observador in app.Services.GetServices<ISolicitudObserver>())
{
    notificador.Suscribir(observador);
}

// ---- Creación de Base de Datos y Usuarios de prueba ----
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<BecaNetDbContext>();

        db.Database.EnsureCreated();

        if (!db.Coordinadores.Any(c => c.Correo == "coordinador@becanet.gt"))
        {
            db.Coordinadores.Add(new CoordinadorBecas
            {
                Nombre = "Ana Ramírez",
                Correo = "coordinador@becanet.gt",
                Contrasena = PasswordHasher.Hashear("Coord1234"),
                Cargo = "Coordinadora de Becas"
            });
        }

        if (!db.Evaluadores.Any(e => e.Correo == "evaluador1@becanet.gt"))
        {
            db.Evaluadores.Add(new EvaluadorComite
            {
                Nombre = "Carlos Pérez",
                Correo = "evaluador1@becanet.gt",
                Contrasena = PasswordHasher.Hashear("Eval1234"),
                Especialidad = "Evaluación Académica"
            });
        }

        if (!db.Evaluadores.Any(e => e.Correo == "evaluador2@becanet.gt"))
        {
            db.Evaluadores.Add(new EvaluadorComite
            {
                Nombre = "Lucía Gómez",
                Correo = "evaluador2@becanet.gt",
                Contrasena = PasswordHasher.Hashear("Eval1234"),
                Especialidad = "Evaluación Socioeconómica"
            });
        }

        db.SaveChanges();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error preparando la base de datos.");
    }
}

app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // sirve los archivos de wwwroot/uploads
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();