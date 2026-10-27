using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

public class CrearSolicitudDTO
{
    [Required] public int IdEstudiante { get; set; }
    [Required] public int IdConvocatoria { get; set; }
    public string? Observaciones { get; set; }
    public string? Motivo { get; set; }
}

/// <summary>Lo que envía un estudiante autenticado: su id sale del token, no se escribe.</summary>
public class CrearMiSolicitudDTO
{
    [Required(ErrorMessage = "Debes elegir una convocatoria.")]
    public int IdConvocatoria { get; set; }

    [Required(ErrorMessage = "Cuéntanos por qué solicitas la beca.")]
    [MinLength(20, ErrorMessage = "El motivo debe tener al menos 20 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}

public class SolicitudDTO
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoRevision { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Motivo { get; set; }
    public string? MotivoResolucion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public int IdEstudiante { get; set; }
    public string? NombreEstudiante { get; set; }
    public int IdConvocatoria { get; set; }
    public string? TituloConvocatoria { get; set; }
    public int? IdComite { get; set; }
    public int CantidadDocumentos { get; set; }
    public bool TieneCertificacionEstudios { get; set; }
}

public class CancelarSolicitudDTO
{
    public string? Motivo { get; set; }
}
