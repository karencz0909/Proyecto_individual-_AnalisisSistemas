namespace BecaNet.Api.Models;

/// <summary>Estado del trabajo del evaluador sobre una solicitud asignada.</summary>
public static class EstadosRevision
{
    public const string SinAsignar = "SIN_ASIGNAR";
    public const string EnRevision = "EN_REVISION";
    public const string Revisado = "REVISADO";
}

/// <summary>Tipos de documento que puede adjuntar un estudiante.</summary>
public static class TiposDocumento
{
    public const string CertificacionEstudios = "CERTIFICACION_ESTUDIOS";
    public const string Otro = "OTRO";
}
