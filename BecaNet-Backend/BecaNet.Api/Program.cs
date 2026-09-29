using System.Text;
using BecaNet.Api.Data;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Base de datos (SQL Server) ----
builder.Services.AddDbContext<BecaNetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BecaNetDb")));

// ---- Seguridad: JWT ----
builder.Services.AddSingleton<JwtService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Emisor"],
            ValidAudience = jwtConfig["Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Clave"]!))
        };
    });

builder.Services.AddAuthorization();

// ---- Repositorios (patrón Repository - capa de acceso a datos) ----
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IConvocatoriaRepository, ConvocatoriaRepository>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IComiteRepository, ComiteRepository>();
builder.Services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();

// ---- Servicios (capa de lógica de negocio) ----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IConvocatoriaService, ConvocatoriaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IComiteService, ComiteService>();
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

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
        Description = "Fases 1, 2 y 3: Auth, Perfil, Convocatorias, Solicitudes, Documentos, Comités, Evaluaciones, Aprobación y Reportes"
    });

    // Botón "Authorize" en Swagger para probar endpoints protegidos con JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Pega aquí el token que devuelve /api/auth/login"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

// Suscribimos todos los observadores registrados al notificador (patrón Observer)
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();