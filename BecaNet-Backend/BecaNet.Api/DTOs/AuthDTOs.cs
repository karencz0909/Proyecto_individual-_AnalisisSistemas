namespace BecaNet.Api.DTOs
{
    public class LoginDTO
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }

    // Compatibilidad para controladores que usan LoginDto
    public class LoginDto : LoginDTO { }

    public class RegistroEstudianteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string NivelAcademico { get; set; } = string.Empty;
        public string InstitucionProcedencia { get; set; } = string.Empty;
    }

    // Compatibilidad para controladores que usan RegisterDto
    public class RegisterDto : RegistroEstudianteDTO { }

    public class AuthRespuestaDTO
    {
        public string Token { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
    }
}