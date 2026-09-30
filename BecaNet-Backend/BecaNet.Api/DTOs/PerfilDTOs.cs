using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>US-003: Datos editables del perfil de un usuario.</summary>
public class ActualizarPerfilDTO
{
    [Required] public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    public string? Telefono { get; set; }
}

public class PerfilDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string TipoUsuario { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}