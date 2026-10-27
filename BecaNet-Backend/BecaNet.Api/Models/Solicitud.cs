namespace BecaNet.Api.Models;

public class Solicitud
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public string Estado { get; set; } = EstadoSolicitud.EnProceso;
    public string? Observaciones { get; set; }

    /// <summary>Texto en el que el estudiante explica por qué solicita la beca.</summary>
    public string? Motivo { get; set; }

    public string? MotivoResolucion { get; set; } // motivo de aprobación/rechazo (US-013)
    public DateTime? FechaResolucion { get; set; } // fecha en que se aprobó/rechazó (US-013)

    public int IdEstudiante { get; set; }
    public Estudiante? Estudiante { get; set; }

    public int IdConvocatoria { get; set; }
    public Convocatoria? Convocatoria { get; set; }

    public int? IdComite { get; set; }
    public ComiteEvaluador? Comite { get; set; }

    // --- Asignación a un evaluador (una solicitud la revisa un evaluador) ---
    public int? IdEvaluadorAsignado { get; set; }
    public EvaluadorComite? EvaluadorAsignado { get; set; }
    public string EstadoRevision { get; set; } = EstadosRevision.SinAsignar;
    public DateTime? FechaAsignacion { get; set; }

    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    public ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
}
