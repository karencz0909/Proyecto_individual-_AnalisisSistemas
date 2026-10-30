namespace BecaNet.Api.DTOs;

public class RegistroAuditoriaDTO
{
    public long Id { get; set; }
    public int? IdSolicitud { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string? IdEntidad { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoUsuario { get; set; } = string.Empty;
    public string RolUsuario { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime FechaHoraUtc { get; set; }
}

/// <summary>US-016: Reporte de resultados de una convocatoria.</summary>
public class ReporteConvocatoriaDTO
{
    public int IdConvocatoria { get; set; }
    public string TituloConvocatoria { get; set; } = string.Empty;
    public int TotalSolicitudes { get; set; }
    public int Aprobadas { get; set; }
    public int Rechazadas { get; set; }
    public int EnProceso { get; set; }
    public int Evaluadas { get; set; }
    public int Canceladas { get; set; }
}

/// <summary>US-017: Estadísticas generales del sistema.</summary>
public class EstadisticasGeneralesDTO
{
    public int TotalSolicitudes { get; set; }
    public int TotalEnProceso { get; set; }
    public int TotalEvaluadas { get; set; }
    public int TotalAprobadas { get; set; }
    public int TotalRechazadas { get; set; }
    public int TotalCanceladas { get; set; }
    public int TotalConvocatoriasAbiertas { get; set; }
}
