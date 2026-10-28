namespace BecaNet.Api.Models;

public class NotificacionSolicitud
{
    public long Id { get; set; }
    public int IdSolicitud { get; set; }
    public int IdEstudiante { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Evento { get; set; } = "CAMBIO_ESTADO_SOLICITUD";
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
}
