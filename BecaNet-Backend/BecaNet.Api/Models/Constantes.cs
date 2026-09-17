namespace BecaNet.Api.Models;

/// <summary>
/// Estados posibles de una Solicitud. Se usan como cadenas de texto para que
/// coincidan exactamente con el CHECK CONSTRAINT definido en el script SQL Server
/// (BecaNet_Script_SQLServer.sql).
/// </summary>
public static class EstadoSolicitud
{
    public const string EnProceso = "EN_PROCESO";
    public const string Evaluada = "EVALUADA";
    public const string Aprobada = "APROBADA";
    public const string Rechazada = "RECHAZADA";
    public const string Cancelada = "CANCELADA";
}

/// <summary>
/// Estados posibles de una Convocatoria.
/// </summary>
public static class EstadoConvocatoria
{
    public const string Borrador = "BORRADOR";
    public const string Abierta = "ABIERTA";
    public const string Cerrada = "CERRADA";
}

/// <summary>
/// Tipos de archivo permitidos para la carga de documentación (US-008).
/// </summary>
public static class TiposArchivoPermitidos
{
    public static readonly string[] Extensiones = { ".pdf", ".jpg", ".jpeg", ".png" };
    public static readonly string[] TiposValidos = { "PDF", "JPG", "PNG" };
    public const int TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB, según criterio de aceptación de US-008
}
