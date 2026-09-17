namespace BecaNet.Api.Models;

/// <summary>
/// Documento adjunto a una Solicitud (composición: si se borra la Solicitud,
/// se borran sus Documentos - ON DELETE CASCADE en la base de datos).
/// Corresponde al Módulo de carga de documentación (US-008).
/// </summary>
public class Documento
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty; // PDF, JPG, PNG
    public string UrlArchivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; } = DateTime.Now;

    public int IdSolicitud { get; set; }
    public Solicitud? Solicitud { get; set; }
}
