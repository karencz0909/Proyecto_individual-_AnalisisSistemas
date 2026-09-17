using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>Datos que envía el estudiante para postular a una convocatoria (US-007).</summary>
public class CrearSolicitudDTO
{
    [Required]
    public int IdEstudiante { get; set; }

    [Required]
    public int IdConvocatoria { get; set; }

    public string? Observaciones { get; set; }
}

/// <summary>Datos que se devuelven al consultar una solicitud.</summary>
public class SolicitudDTO
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int IdEstudiante { get; set; }
    public string? NombreEstudiante { get; set; }
    public int IdConvocatoria { get; set; }
    public string? TituloConvocatoria { get; set; }
    public int? IdComite { get; set; }
    public int CantidadDocumentos { get; set; }
}

/// <summary>Datos para cancelar una solicitud.</summary>
public class CancelarSolicitudDTO
{
    public string? Motivo { get; set; }
}
