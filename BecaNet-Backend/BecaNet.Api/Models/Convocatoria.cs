namespace BecaNet.Api.Models;

/// <summary>
/// Convocatoria de becas. Entidad de soporte del Sprint 2, incluida aquí
/// completa porque Solicitud depende de ella con una llave foránea.
/// </summary>
public class Convocatoria
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Requisitos { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = EstadoConvocatoria.Borrador;

    public int IdCoordinador { get; set; }
    public CoordinadorBecas? Coordinador { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}
