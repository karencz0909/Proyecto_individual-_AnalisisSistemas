namespace BecaNet.Api.Models;

/// <summary>
/// Comité evaluador. Corresponde al Módulo de gestión de comités evaluadores (US-010, US-011).
/// </summary>
public class ComiteEvaluador
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Relación N:M con EvaluadorComite a través de ComiteMiembro (agregación)
    public ICollection<ComiteMiembro> Miembros { get; set; } = new List<ComiteMiembro>();

    // Solicitudes que le han sido asignadas a este comité para evaluación
    public ICollection<Solicitud> SolicitudesAsignadas { get; set; } = new List<Solicitud>();
}

/// <summary>
/// Tabla intermedia N:M entre ComiteEvaluador y EvaluadorComite.
/// </summary>
public class ComiteMiembro
{
    public int IdComite { get; set; }
    public ComiteEvaluador? Comite { get; set; }

    public int IdEvaluador { get; set; }
    public EvaluadorComite? Evaluador { get; set; }
}
