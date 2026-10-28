using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

public class DocumentoResumenDTO
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string UrlArchivo { get; set; } = string.Empty;
}

/// <summary>Solicitud tal como la ve el evaluador al que se le asignó.</summary>
public class AsignacionEvaluadorDTO
{
    public int IdSolicitud { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public string TituloConvocatoria { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public decimal MontoSolicitado { get; set; }
    public bool DecisionEvaluadorRegistrada { get; set; }
    public bool AprobadaPorEvaluador { get; set; }
    public string EstadoSolicitud { get; set; } = string.Empty;
    public string EstadoRevision { get; set; } = string.Empty;
    public DateTime? FechaAsignacion { get; set; }
    public decimal? Puntaje { get; set; }
    public string? ObservacionesEvaluacion { get; set; }
    public List<DocumentoResumenDTO> Documentos { get; set; } = new();
}

/// <summary>El evaluador califica su solicitud asignada.</summary>
public class RevisarSolicitudDTO
{
    [Required, Range(0, 100, ErrorMessage = "El puntaje debe estar entre 0 y 100.")]
    public decimal Puntaje { get; set; }

    [Required(ErrorMessage = "Las observaciones son obligatorias.")]
    [MinLength(5, ErrorMessage = "Escribe al menos 5 caracteres en las observaciones.")]
    public string Observaciones { get; set; } = string.Empty;
}

/// <summary>Solicitud tal como la ve el coordinador (estudiante, evaluador, estado).</summary>
public class SolicitudCoordinadorDTO
{
    public int IdSolicitud { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public string CorreoEstudiante { get; set; } = string.Empty;
    public string TituloConvocatoria { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public decimal MontoSolicitado { get; set; }
    public bool AprobadaPorEvaluador { get; set; }
    public bool AprobadaPorCoordinador { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoRevision { get; set; } = string.Empty;
    public int? IdEvaluadorAsignado { get; set; }
    public string? NombreEvaluador { get; set; }
    public string? CorreoEvaluador { get; set; }
    public decimal? Puntaje { get; set; }
    public string? ObservacionesEvaluacion { get; set; }
    public string? MotivoResolucion { get; set; }
    public bool TieneCertificacionEstudios { get; set; }
    public List<DocumentoResumenDTO> Documentos { get; set; } = new();
}

public class EvaluadorOpcionDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int Asignadas { get; set; }
}

public class DistribuirDTO
{
    /// <summary>Si viene, solo se reparte entre los miembros de ese comité.</summary>
    public int? IdComite { get; set; }
}

public class ResultadoDistribucionDTO
{
    public int TotalAsignadas { get; set; }
    public int OmitidasSinCertificacion { get; set; }
    public List<EvaluadorOpcionDTO> PorEvaluador { get; set; } = new();
}

public class AsignarManualDTO
{
    [Required] public int IdEvaluador { get; set; }
}
