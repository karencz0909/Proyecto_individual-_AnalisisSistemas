using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>Datos para crear un comité evaluador con sus miembros (US-010).
/// Debe tener al menos 2 miembros, según la regla de negocio definida.</summary>
public class CrearComiteDTO
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "Un comité debe tener al menos 2 miembros asignados.")]
    public List<int> IdsEvaluadores { get; set; } = new();
}

/// <summary>Datos que se devuelven al consultar un comité.</summary>
public class ComiteDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public List<string> Miembros { get; set; } = new();
    public int SolicitudesAsignadas { get; set; }
}

/// <summary>Datos para asignar una o varias solicitudes a un comité (US-011).</summary>
public class AsignarSolicitudesDTO
{
    [Required]
    public int IdComite { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> IdsSolicitudes { get; set; } = new();
}
