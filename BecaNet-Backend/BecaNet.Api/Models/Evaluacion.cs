namespace BecaNet.Api.Models;

/// <summary>
/// Evaluación que un miembro de un comité realiza sobre una Solicitud.
/// Corresponde al Módulo de registro de evaluaciones (US-012).
/// Vinculada al comité asignado a la solicitud: solo un evaluador que
/// pertenezca al comité asignado puede registrar una evaluación.
/// </summary>
public class Evaluacion
{
    public int Id { get; set; }
    public decimal Puntaje { get; set; }       // 0 a 100
    public string? Observaciones { get; set; }
    public DateTime FechaEvaluacion { get; set; } = DateTime.Now;

    public int IdSolicitud { get; set; }
    public Solicitud? Solicitud { get; set; }

    public int IdEvaluador { get; set; }
    public EvaluadorComite? Evaluador { get; set; }
}
