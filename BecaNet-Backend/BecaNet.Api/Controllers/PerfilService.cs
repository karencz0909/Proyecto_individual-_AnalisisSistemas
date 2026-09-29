using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;

namespace BecaNet.Api.Services;

public interface IPerfilService
{
    Task<PerfilDTO> ObtenerAsync(int id, string tipoUsuario);
    Task<PerfilDTO> ActualizarAsync(int id, string tipoUsuario, ActualizarPerfilDTO dto);
}

/// <summary>US-003: Como usuario registrado, quiero editar mi perfil.</summary>
public class PerfilService : IPerfilService
{
    private readonly IUsuarioRepository _usuarioRepo;

    public PerfilService(IUsuarioRepository usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    public async Task<PerfilDTO> ObtenerAsync(int id, string tipoUsuario)
    {
        var usuario = await _usuarioRepo.BuscarPorIdAsync(id, tipoUsuario)
            ?? throw new InvalidOperationException("El usuario no existe.");

        return MapearADto(usuario, tipoUsuario);
    }

    public async Task<PerfilDTO> ActualizarAsync(int id, string tipoUsuario, ActualizarPerfilDTO dto)
    {
        var usuario = await _usuarioRepo.BuscarPorIdAsync(id, tipoUsuario)
            ?? throw new InvalidOperationException("El usuario no existe.");

        // Criterio de aceptación: los cambios se guardan solo si los campos obligatorios son válidos
        // (la validación de formato ya la hace [Required]/[EmailAddress] en el DTO)
        usuario.Nombre = dto.Nombre;
        usuario.Correo = dto.Correo;
        usuario.Telefono = dto.Telefono;

        await _usuarioRepo.ActualizarAsync(usuario);
        return MapearADto(usuario, tipoUsuario);
    }

    private static PerfilDTO MapearADto(Usuario usuario, string tipoUsuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Correo = usuario.Correo,
        Telefono = usuario.Telefono,
        TipoUsuario = tipoUsuario,
        FechaRegistro = usuario.FechaRegistro
    };
}