using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Data;

public class BecaNetDbContext : DbContext
{
    public BecaNetDbContext(DbContextOptions<BecaNetDbContext> options) : base(options) { }

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<CoordinadorBecas> Coordinadores => Set<CoordinadorBecas>();
    public DbSet<EvaluadorComite> Evaluadores => Set<EvaluadorComite>();
    public DbSet<Convocatoria> Convocatorias => Set<Convocatoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<ComiteEvaluador> Comites => Set<ComiteEvaluador>();
    public DbSet<ComiteMiembro> ComiteMiembros => Set<ComiteMiembro>();
    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---- Herencia Usuario -> Estudiante / CoordinadorBecas / EvaluadorComite ----
        // Estrategia Table-Per-Type (TPT): cada subclase tiene su propia tabla,
        // compartiendo la llave primaria con la tabla base Usuario.
        modelBuilder.Entity<Estudiante>().ToTable("Estudiante");
        modelBuilder.Entity<CoordinadorBecas>().ToTable("CoordinadorBecas");
        modelBuilder.Entity<EvaluadorComite>().ToTable("EvaluadorComite");

        modelBuilder.Entity<Estudiante>().HasKey(e => e.Id);
        modelBuilder.Entity<CoordinadorBecas>().HasKey(c => c.Id);
        modelBuilder.Entity<EvaluadorComite>().HasKey(ev => ev.Id);

        // ---- Convocatoria ----
        modelBuilder.Entity<Convocatoria>(cfg =>
        {
            cfg.Property(c => c.Estado).HasMaxLength(20);
            cfg.HasOne(c => c.Coordinador)
               .WithMany(co => co.Convocatorias)
               .HasForeignKey(c => c.IdCoordinador)
               .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Solicitud ----
        modelBuilder.Entity<Solicitud>(cfg =>
        {
            cfg.Property(s => s.Estado).HasMaxLength(20);

            cfg.HasOne(s => s.Estudiante)
               .WithMany(e => e.Solicitudes)
               .HasForeignKey(s => s.IdEstudiante)
               .OnDelete(DeleteBehavior.Restrict);

            cfg.HasOne(s => s.Convocatoria)
               .WithMany(c => c.Solicitudes)
               .HasForeignKey(s => s.IdConvocatoria)
               .OnDelete(DeleteBehavior.Restrict);

            cfg.HasOne(s => s.Comite)
               .WithMany(c => c.SolicitudesAsignadas)
               .HasForeignKey(s => s.IdComite)
               .OnDelete(DeleteBehavior.Restrict);

            // Un estudiante solo puede tener UNA solicitud por convocatoria (US-007)
            cfg.HasIndex(s => new { s.IdEstudiante, s.IdConvocatoria }).IsUnique();
        });

        // ---- Documento (composición: se borra en cascada con la Solicitud) ----
        modelBuilder.Entity<Documento>(cfg =>
        {
            cfg.HasOne(d => d.Solicitud)
               .WithMany(s => s.Documentos)
               .HasForeignKey(d => d.IdSolicitud)
               .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- ComiteMiembro (tabla intermedia N:M, llave compuesta) ----
        modelBuilder.Entity<ComiteMiembro>(cfg =>
        {
            cfg.HasKey(cm => new { cm.IdComite, cm.IdEvaluador });

            cfg.HasOne(cm => cm.Comite)
               .WithMany(c => c.Miembros)
               .HasForeignKey(cm => cm.IdComite)
               .OnDelete(DeleteBehavior.Cascade);

            cfg.HasOne(cm => cm.Evaluador)
               .WithMany(e => e.Comites)
               .HasForeignKey(cm => cm.IdEvaluador)
               .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Evaluacion ----
        modelBuilder.Entity<Evaluacion>(cfg =>
        {
            cfg.Property(e => e.Puntaje).HasColumnType("decimal(5,2)");

            cfg.HasOne(e => e.Solicitud)
               .WithMany(s => s.Evaluaciones)
               .HasForeignKey(e => e.IdSolicitud)
               .OnDelete(DeleteBehavior.Restrict);

            cfg.HasOne(e => e.Evaluador)
               .WithMany(ev => ev.Evaluaciones)
               .HasForeignKey(e => e.IdEvaluador)
               .OnDelete(DeleteBehavior.Restrict);

            // Un evaluador solo puede evaluar una vez la misma solicitud
            cfg.HasIndex(e => new { e.IdSolicitud, e.IdEvaluador }).IsUnique();
        });
    }
}
