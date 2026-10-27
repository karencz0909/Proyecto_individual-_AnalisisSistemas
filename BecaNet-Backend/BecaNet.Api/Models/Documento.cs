namespace BecaNet.Api.Models;

public class Documento
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty;       // PDF, JPG, PNG
    public string TipoDocumento { get; set; } = TiposDocumento.Otro; // CERTIFICACION_ESTUDIOS u OTRO
    public string UrlArchivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; } = DateTime.Now;

    public int IdSolicitud { get; set; }
    public Solicitud? Solicitud { get; set; }
}
