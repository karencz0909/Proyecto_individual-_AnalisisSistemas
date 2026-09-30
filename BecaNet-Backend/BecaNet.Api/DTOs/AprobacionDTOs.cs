using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>US-013: Datos para aprobar o rechazar una solicitud evaluada.</summary>
public class ResolverSolicitudDTO
{
    /// <summary>true = aprobar, false = rechazar.</summary>
    [Required]
    public bool Aprobar { get; set; }

    /// <summary>Obligatorio cuando Aprobar = false, según el criterio de aceptación de US-013.</summary>
    public string? Motivo { get; set; }
}