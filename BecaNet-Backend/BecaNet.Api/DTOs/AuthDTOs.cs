using System.ComponentModel.DataAnnotations;

namespace BecaNet.Api.DTOs;

/// <summary>US-001: Registro de un nuevo estudiante.</summary>
public class RegistroEstudianteDTO
{
    [Required] public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [Required] public string NivelAcademico { get; set; } = string.Empty;
    public string? InstitucionProcedencia { get; set; }
}

/// <summary>US-002: Inicio de sesión (válido para cualquier tipo de usuario).</summary>
public class LoginDTO
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}

/// <summary>Respuesta tras un login o registro exitoso.</summary>
public class AuthRespuestaDTO
{
    public string Token { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = string.Empty;
}