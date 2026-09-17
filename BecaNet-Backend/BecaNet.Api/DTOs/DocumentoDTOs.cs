namespace BecaNet.Api.DTOs;

/// <summary>Datos que se devuelven al consultar los documentos de una solicitud (US-008).</summary>
public class DocumentoDTO
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty;
    public string UrlArchivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
    public int IdSolicitud { get; set; }
}
