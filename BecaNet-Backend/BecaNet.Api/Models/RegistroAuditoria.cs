namespace BecaNet.Api.Models;

/// <summary>Registro de auditoría de solo inserción desde la aplicación.</summary>
public class RegistroAuditoria
{
    public long Id { get; set; }
    public int? IdSolicitud { get; set; }
    public string Entidad { get; set; } = "SOLICITUD";
    public string? IdEntidad { get; set; }
    public int IdUsuario { get; set; }
    public string RolUsuario { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
}
