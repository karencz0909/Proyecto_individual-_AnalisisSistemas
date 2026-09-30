using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>US-004: Datos para crear una convocatoria.</summary>
public class CrearConvocatoriaDTO
{
    [Required] public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Requisitos { get; set; }

    [Required] public DateTime FechaApertura { get; set; }
    [Required] public DateTime FechaCierre { get; set; }

    [Required] public int IdCoordinador { get; set; }
}

public class ConvocatoriaDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Requisitos { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadSolicitudes { get; set; }
}