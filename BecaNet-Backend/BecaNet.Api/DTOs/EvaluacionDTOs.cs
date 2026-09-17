using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>Datos que envía un evaluador al registrar su evaluación (US-012).</summary>
public class RegistrarEvaluacionDTO
{
    [Required]
    public int IdSolicitud { get; set; }

    [Required]
    public int IdEvaluador { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "El puntaje debe estar entre 0 y 100.")]
    public decimal Puntaje { get; set; }

    [Required(ErrorMessage = "Las observaciones son obligatorias para dejar constancia de la decisión.")]
    public string Observaciones { get; set; } = string.Empty;
}

/// <summary>Datos que se devuelven al consultar una evaluación.</summary>
public class EvaluacionDTO
{
    public int Id { get; set; }
    public decimal Puntaje { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaEvaluacion { get; set; }
    public int IdSolicitud { get; set; }
    public int IdEvaluador { get; set; }
    public string? NombreEvaluador { get; set; }
}
