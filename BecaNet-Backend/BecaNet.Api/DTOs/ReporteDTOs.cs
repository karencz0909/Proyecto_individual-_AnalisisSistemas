namespace BecaNet.Api.DTOs;

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