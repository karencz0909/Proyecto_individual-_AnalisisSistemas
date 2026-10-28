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
    public DbSet<AprobacionSolicitud> Aprobaciones => Set<AprobacionSolicitud>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<NotificacionSolicitud> Notificaciones => Set<NotificacionSolicitud>();

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
            cfg.Property(s => s.MontoSolicitado).HasColumnType("decimal(12,2)");

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

            // Evaluador al que se le asignó la revisión de la solicitud
            cfg.Property(s => s.EstadoRevision).HasMaxLength(20);
            cfg.HasOne(s => s.EvaluadorAsignado)
               .WithMany()
               .HasForeignKey(s => s.IdEvaluadorAsignado)
               .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AprobacionSolicitud>(cfg =>
        {
            cfg.ToTable("AprobacionSolicitud");
            cfg.Property(a => a.Tipo).HasMaxLength(20).IsRequired();
            cfg.Property(a => a.RolUsuario).HasMaxLength(30).IsRequired();
            cfg.Property(a => a.Motivo).HasMaxLength(1000);
            cfg.HasIndex(a => new { a.IdSolicitud, a.Tipo }).IsUnique();
            cfg.HasOne(a => a.Solicitud).WithMany(s => s.Aprobaciones)
                .HasForeignKey(a => a.IdSolicitud).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RegistroAuditoria>(cfg =>
        {
            cfg.ToTable("RegistroAuditoria");
            cfg.Property(a => a.RolUsuario).HasMaxLength(30).IsRequired();
            cfg.Property(a => a.Operacion).HasMaxLength(80).IsRequired();
            cfg.Property(a => a.Detalle).HasMaxLength(2000);
            cfg.HasIndex(a => new { a.IdSolicitud, a.FechaHoraUtc });
            cfg.HasOne<Solicitud>().WithMany().HasForeignKey(a => a.IdSolicitud).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificacionSolicitud>(cfg =>
        {
            cfg.ToTable("NotificacionSolicitud");
            cfg.Property(n => n.EstadoAnterior).HasMaxLength(20).IsRequired();
            cfg.Property(n => n.EstadoNuevo).HasMaxLength(20).IsRequired();
            cfg.Property(n => n.Evento).HasMaxLength(60).IsRequired();
            cfg.HasIndex(n => new { n.IdSolicitud, n.FechaHoraUtc });
            cfg.HasOne<Solicitud>().WithMany().HasForeignKey(n => n.IdSolicitud).OnDelete(DeleteBehavior.Restrict);
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
