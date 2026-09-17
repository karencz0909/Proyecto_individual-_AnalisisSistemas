namespace BecaNet.Api.Models;

/// <summary>
/// Clase base de la jerarquía de usuarios (mapeada como Table-Per-Type en EF Core).
/// Corresponde a la clase abstracta "Usuario" del Diagrama de Clases (modelo de dominio).
/// </summary>
public abstract class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty; // se almacena como hash
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>
/// Estudiante que puede postular a becas. Hereda de Usuario.
/// </summary>
public class Estudiante : Usuario
{
    public string NivelAcademico { get; set; } = string.Empty;
    public string? InstitucionProcedencia { get; set; }

    // Navegación: un estudiante puede tener muchas solicitudes
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

/// <summary>
/// Coordinador de becas del Ministerio de Educación. Hereda de Usuario.
/// (Entidad de soporte: ya se creó en un sprint anterior; se incluye aquí
/// completa para que el modelo de datos compile de forma consistente).
/// </summary>
public class CoordinadorBecas : Usuario
{
    public string? Cargo { get; set; }

    public ICollection<Convocatoria> Convocatorias { get; set; } = new List<Convocatoria>();
}

/// <summary>
/// Miembro de un comité evaluador. Hereda de Usuario.
/// </summary>
public class EvaluadorComite : Usuario
{
    public string? Especialidad { get; set; }

    // Navegación: relación N:M con ComiteEvaluador a través de ComiteMiembro
    public ICollection<ComiteMiembro> Comites { get; set; } = new List<ComiteMiembro>();
    public ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
}
