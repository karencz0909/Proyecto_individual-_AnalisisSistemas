namespace BecaNet.Api.Models;

public static class EstadoSolicitud
{
    public const string EnProceso = "EN_PROCESO";
    public const string Evaluada = "EVALUADA";
    public const string Aprobada = "APROBADA";
    public const string Rechazada = "RECHAZADA";
    public const string Cancelada = "CANCELADA";
}

public static class EstadoConvocatoria
{
    public const string Borrador = "BORRADOR";
    public const string Abierta = "ABIERTA";
    public const string Cerrada = "CERRADA";
}

public static class TiposArchivoPermitidos
{
    public static readonly string[] Extensiones = { ".pdf", ".jpg", ".jpeg", ".png" };
    public static readonly string[] TiposValidos = { "PDF", "JPG", "PNG" };
    public const int TamanoMaximoBytes = 5 * 1024 * 1024;
}

public static class TiposUsuario
{
    public const string Estudiante = "ESTUDIANTE";
    public const string Coordinador = "COORDINADOR";
    public const string Evaluador = "EVALUADOR";
}