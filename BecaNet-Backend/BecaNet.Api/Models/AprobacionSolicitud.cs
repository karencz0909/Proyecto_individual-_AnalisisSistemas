namespace BecaNet.Api.Models;

public static class TipoAprobacionSolicitud
{
    public const string Evaluador = "EVALUADOR";
    public const string Coordinador = "COORDINADOR";
}

/// <summary>Decisión inmutable registrada por un actor sobre una solicitud.</summary>
public class AprobacionSolicitud
{
    public int Id { get; set; }
    public int IdSolicitud { get; set; }
    public Solicitud? Solicitud { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public string RolUsuario { get; set; } = string.Empty;
    public bool Aprobada { get; set; }
    public string? Motivo { get; set; }
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
}
