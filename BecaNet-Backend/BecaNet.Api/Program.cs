using System.Text;
using System.Text.RegularExpressions;
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
builder.Services.AddScoped<IGestionUsuariosService, GestionUsuariosService>();
builder.Services.AddScoped<IConvocatoriaService, ConvocatoriaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IComiteService, ComiteService>();
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();

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

// ---- Creación de Base de Datos y datos de prueba ----
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<BecaNetDbContext>();

        db.Database.EnsureCreated();

        // EnsureCreated no modifica una base que ya existía. Aplicamos el parche
        // idempotente después de asegurar la base y antes de consultar sus tablas.
        var rutasScript = new[]
        {
            Path.Combine(app.Environment.ContentRootPath, "BecaNet_Nuevos_Requisitos.sql"),
            Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "BecaNet_Nuevos_Requisitos.sql"))
        };
        var rutaScript = rutasScript.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException(
                "No se encontró BecaNet_Nuevos_Requisitos.sql; no se puede actualizar el esquema de la base de datos.",
                rutasScript[0]);
        var scriptEsquema = await File.ReadAllTextAsync(rutaScript);
        var lotesSql = Regex.Split(scriptEsquema, @"(?im)^\s*GO\s*;?\s*$");
        foreach (var loteSql in lotesSql.Where(lote => !string.IsNullOrWhiteSpace(lote)))
            await db.Database.ExecuteSqlRawAsync(loteSql);

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

        if (!db.Evaluadores.Any(e => e.Correo == "evaluador3@becanet.gt"))
        {
            db.Evaluadores.Add(new EvaluadorComite
            {
                Nombre = "Mario Díaz",
                Correo = "evaluador3@becanet.gt",
                Contrasena = PasswordHasher.Hashear("Eval1234"),
                Especialidad = "Evaluación Documental"
            });
        }

        if (!db.Estudiantes.Any(e => e.Correo == "estudiante@becanet.gt"))
        {
            db.Estudiantes.Add(new Estudiante
            {
                Nombre = "Mariana López",
                Correo = "estudiante@becanet.gt",
                Contrasena = PasswordHasher.Hashear("Estu1234"),
                NivelAcademico = "Universitario",
                InstitucionProcedencia = "Universidad Mariano Gálvez"
            });
        }

        db.SaveChanges();

        // Convocatorias abiertas para que el estudiante tenga a qué postular
        if (!db.Convocatorias.Any())
        {
            var coordinador = db.Coordinadores.First(c => c.Correo == "coordinador@becanet.gt");

            db.Convocatorias.Add(new Convocatoria
            {
                Titulo = "Beca de Excelencia Académica 2026",
                Descripcion = "Apoyo económico para estudiantes con rendimiento sobresaliente.",
                Requisitos = "Promedio mínimo de 85 puntos y certificación de estudios vigente.",
                FechaApertura = DateTime.Today.AddDays(-7),
                FechaCierre = DateTime.Today.AddMonths(3),
                Estado = EstadoConvocatoria.Abierta,
                IdCoordinador = coordinador.Id
            });

            db.Convocatorias.Add(new Convocatoria
            {
                Titulo = "Beca de Apoyo Socioeconómico 2026",
                Descripcion = "Cobertura de matrícula para estudiantes con necesidad económica comprobada.",
                Requisitos = "Estudio socioeconómico y certificación de estudios vigente.",
                FechaApertura = DateTime.Today.AddDays(-3),
                FechaCierre = DateTime.Today.AddMonths(2),
                Estado = EstadoConvocatoria.Abierta,
                IdCoordinador = coordinador.Id
            });

            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "No se pudo preparar o actualizar la base de datos. La API no puede continuar con un esquema incompleto.");
        throw;
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
