using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

public class CrearUsuarioGestionDTO
{
    [Required, StringLength(120, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(180)] public string Correo { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Contrasena { get; set; } = string.Empty;
    [Required] public string TipoUsuario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? NivelAcademico { get; set; }
    public string? InstitucionProcedencia { get; set; }
    public string? Cargo { get; set; }
    public string? Especialidad { get; set; }
}

public class ActualizarUsuarioGestionDTO
{
    [Required, StringLength(120, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(180)] public string Correo { get; set; } = string.Empty;
    public string? Contrasena { get; set; }
    public string? Telefono { get; set; }
    public string? NivelAcademico { get; set; }
    public string? InstitucionProcedencia { get; set; }
    public string? Cargo { get; set; }
    public string? Especialidad { get; set; }
}

public class UsuarioGestionDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string TipoUsuario { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string? NivelAcademico { get; set; }
    public string? InstitucionProcedencia { get; set; }
    public string? Cargo { get; set; }
    public string? Especialidad { get; set; }
}
