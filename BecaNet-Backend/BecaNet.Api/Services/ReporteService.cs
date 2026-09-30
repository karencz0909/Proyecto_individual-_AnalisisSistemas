using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IReporteService
{
    Task<ReporteConvocatoriaDTO> ReporteConvocatoriaAsync(int idConvocatoria);
    Task<EstadisticasGeneralesDTO> EstadisticasGeneralesAsync();
}

/// <summary>Módulo de Reportería (US-016, US-017).</summary>
public class ReporteService : IReporteService
{
    private readonly BecaNetDbContext _context;

    public ReporteService(BecaNetDbContext context)
    {
        _context = context;
    }

    /// <summary>US-016: reporte de resultados de una convocatoria específica.</summary>
    public async Task<ReporteConvocatoriaDTO> ReporteConvocatoriaAsync(int idConvocatoria)
    {
        var convocatoria = await _context.Convocatorias.FindAsync(idConvocatoria)
            ?? throw new InvalidOperationException("La convocatoria no existe.");

        var solicitudes = await _context.Solicitudes
            .Where(s => s.IdConvocatoria == idConvocatoria)
            .ToListAsync();

        return new ReporteConvocatoriaDTO
        {
            IdConvocatoria = convocatoria.Id,
            TituloConvocatoria = convocatoria.Titulo,
            TotalSolicitudes = solicitudes.Count,
            Aprobadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Aprobada),
            Rechazadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Rechazada),
            EnProceso = solicitudes.Count(s => s.Estado == EstadoSolicitud.EnProceso),
            Evaluadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Evaluada),
            Canceladas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Cancelada)
        };
    }

    /// <summary>US-017: estadísticas generales de todo el sistema.</summary>
    public async Task<EstadisticasGeneralesDTO> EstadisticasGeneralesAsync()
    {
        var solicitudes = await _context.Solicitudes.ToListAsync();

        return new EstadisticasGeneralesDTO
        {
            TotalSolicitudes = solicitudes.Count,
            TotalEnProceso = solicitudes.Count(s => s.Estado == EstadoSolicitud.EnProceso),
            TotalEvaluadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Evaluada),
            TotalAprobadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Aprobada),
            TotalRechazadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Rechazada),
            TotalCanceladas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Cancelada),
            TotalConvocatoriasAbiertas = await _context.Convocatorias
                .CountAsync(c => c.Estado == EstadoConvocatoria.Abierta)
        };
    }
}