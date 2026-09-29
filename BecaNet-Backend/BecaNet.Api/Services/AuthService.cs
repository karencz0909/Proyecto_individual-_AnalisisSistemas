using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;
using BecaNet.Api.Security;

namespace BecaNet.Api.Services;

public interface IAuthService
{
    Task<AuthRespuestaDTO> RegistrarEstudianteAsync(RegistroEstudianteDTO dto);
    Task<AuthRespuestaDTO> LoginAsync(LoginDTO dto);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly JwtService _jwtService;

    public AuthService(IUsuarioRepository usuarioRepo, JwtService jwtService)
    {
        _usuarioRepo = usuarioRepo;
        _jwtService = jwtService;
    }

    public async Task<AuthRespuestaDTO> RegistrarEstudianteAsync(RegistroEstudianteDTO dto)
    {
        if (await _usuarioRepo.ExisteCorreoAsync(dto.Correo))
            throw new InvalidOperationException("Ya existe una cuenta registrada con ese correo.");

        var estudiante = new Estudiante
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            Contrasena = PasswordHasher.Hashear(dto.Contrasena),
            Telefono = dto.Telefono,
            NivelAcademico = dto.NivelAcademico,
            InstitucionProcedencia = dto.InstitucionProcedencia,
            FechaRegistro = DateTime.Now
        };

        var creado = await _usuarioRepo.CrearEstudianteAsync(estudiante);
        var token = _jwtService.GenerarToken(creado.Id, creado.Correo, TiposUsuario.Estudiante);

        return new AuthRespuestaDTO
        {
            Token = token,
            Id = creado.Id,
            Nombre = creado.Nombre,
            Correo = creado.Correo,
            TipoUsuario = TiposUsuario.Estudiante
        };
    }

    public async Task<AuthRespuestaDTO> LoginAsync(LoginDTO dto)
    {
        var usuario = await _usuarioRepo.BuscarPorCorreoAsync(dto.Correo)
            ?? throw new InvalidOperationException("Correo o contraseña incorrectos.");

        if (!PasswordHasher.Verificar(dto.Contrasena, usuario.Contrasena))
            throw new InvalidOperationException("Correo o contraseña incorrectos.");

        var tipoUsuario = usuario switch
        {
            Estudiante => TiposUsuario.Estudiante,
            CoordinadorBecas => TiposUsuario.Coordinador,
            EvaluadorComite => TiposUsuario.Evaluador,
            _ => throw new InvalidOperationException("Tipo de usuario no reconocido.")
        };

        var token = _jwtService.GenerarToken(usuario.Id, usuario.Correo, tipoUsuario);

        return new AuthRespuestaDTO
        {
            Token = token,
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            TipoUsuario = tipoUsuario
        };
    }
}