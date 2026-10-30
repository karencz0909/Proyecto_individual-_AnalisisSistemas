using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IGestionUsuariosService
{
    Task<List<UsuarioGestionDTO>> ListarAsync();
    Task<UsuarioGestionDTO> CrearAsync(CrearUsuarioGestionDTO dto, int idActor);
    Task<UsuarioGestionDTO> ActualizarAsync(int id, string tipoUsuario, ActualizarUsuarioGestionDTO dto, int idActor);
}

/// <summary>Gestión de perfiles disponible únicamente para coordinadores.</summary>
public class GestionUsuariosService : IGestionUsuariosService
{
    private readonly BecaNetDbContext _context;

    public GestionUsuariosService(BecaNetDbContext context) => _context = context;

    public async Task<List<UsuarioGestionDTO>> ListarAsync()
    {
        var estudiantes = await _context.Estudiantes.AsNoTracking().ToListAsync();
        var coordinadores = await _context.Coordinadores.AsNoTracking().ToListAsync();
        var evaluadores = await _context.Evaluadores.AsNoTracking().ToListAsync();

        return estudiantes.Select(u => Mapear(u, TiposUsuario.Estudiante))
            .Concat(coordinadores.Select(u => Mapear(u, TiposUsuario.Coordinador)))
            .Concat(evaluadores.Select(u => Mapear(u, TiposUsuario.Evaluador)))
            .OrderBy(u => u.Nombre)
            .ToList();
    }

    public async Task<UsuarioGestionDTO> CrearAsync(CrearUsuarioGestionDTO dto, int idActor)
    {
        var tipo = NormalizarTipo(dto.TipoUsuario);
        var correo = dto.Correo.Trim();
        if (await CorreoExisteAsync(correo))
            throw new InvalidOperationException("Ya existe un usuario con ese correo electrónico.");

        Usuario creado = tipo switch
        {
            TiposUsuario.Estudiante => CrearEstudiante(dto, correo),
            TiposUsuario.Coordinador => new CoordinadorBecas
            {
                Nombre = dto.Nombre.Trim(), Correo = correo,
                Contrasena = PasswordHasher.Hashear(dto.Contrasena), Telefono = Limpiar(dto.Telefono),
                Cargo = Limpiar(dto.Cargo)
            },
            TiposUsuario.Evaluador => new EvaluadorComite
            {
                Nombre = dto.Nombre.Trim(), Correo = correo,
                Contrasena = PasswordHasher.Hashear(dto.Contrasena), Telefono = Limpiar(dto.Telefono),
                Especialidad = Limpiar(dto.Especialidad)
            },
            _ => throw new InvalidOperationException("Selecciona un rol válido.")
        };

        switch (creado)
        {
            case Estudiante estudiante: _context.Estudiantes.Add(estudiante); break;
            case CoordinadorBecas coordinador: _context.Coordinadores.Add(coordinador); break;
            case EvaluadorComite evaluador: _context.Evaluadores.Add(evaluador); break;
        }

        await _context.SaveChangesAsync();
        _context.RegistrosAuditoria.Add(new RegistroAuditoria
        {
            IdUsuario = idActor, RolUsuario = TiposUsuario.Coordinador, Entidad = "USUARIO", IdEntidad = $"{tipo}:{((Usuario)creado).Id}",
            Operacion = "CREACION_USUARIO", Detalle = $"Se creó el perfil {tipo}. Nombre: {creado.Nombre}; correo: {creado.Correo}."
        });
        await _context.SaveChangesAsync();
        return Mapear(creado, tipo);
    }

    public async Task<UsuarioGestionDTO> ActualizarAsync(int id, string tipoUsuario, ActualizarUsuarioGestionDTO dto, int idActor)
    {
        var tipo = NormalizarTipo(tipoUsuario);
        var usuario = await ObtenerAsync(id, tipo)
            ?? throw new InvalidOperationException("No se encontró el perfil que quieres editar.");
        var correo = dto.Correo.Trim();

        if (await CorreoExisteAsync(correo, id))
            throw new InvalidOperationException("Ya existe otro usuario con ese correo electrónico.");

        var cambios = new List<string>();
        RegistrarCambio(cambios, "Nombre", usuario.Nombre, dto.Nombre.Trim());
        RegistrarCambio(cambios, "Correo", usuario.Correo, correo);
        RegistrarCambio(cambios, "Teléfono", usuario.Telefono, Limpiar(dto.Telefono));
        usuario.Nombre = dto.Nombre.Trim();
        usuario.Correo = correo;
        usuario.Telefono = Limpiar(dto.Telefono);
        if (!string.IsNullOrWhiteSpace(dto.Contrasena))
        {
            if (dto.Contrasena.Length < 8)
                throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");
            usuario.Contrasena = PasswordHasher.Hashear(dto.Contrasena);
            cambios.Add("Contraseña: se restableció (valor omitido por seguridad)");
        }

        switch (usuario)
        {
            case Estudiante estudiante:
                if (!string.IsNullOrWhiteSpace(dto.NivelAcademico))
                {
                    RegistrarCambio(cambios, "Nivel académico", estudiante.NivelAcademico, dto.NivelAcademico.Trim());
                    estudiante.NivelAcademico = dto.NivelAcademico.Trim();
                }
                RegistrarCambio(cambios, "Institución", estudiante.InstitucionProcedencia, Limpiar(dto.InstitucionProcedencia));
                estudiante.InstitucionProcedencia = Limpiar(dto.InstitucionProcedencia);
                break;
            case CoordinadorBecas coordinador:
                RegistrarCambio(cambios, "Cargo", coordinador.Cargo, Limpiar(dto.Cargo));
                coordinador.Cargo = Limpiar(dto.Cargo);
                break;
            case EvaluadorComite evaluador:
                RegistrarCambio(cambios, "Especialidad", evaluador.Especialidad, Limpiar(dto.Especialidad));
                evaluador.Especialidad = Limpiar(dto.Especialidad);
                break;
        }

        if (cambios.Count > 0)
            _context.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                IdUsuario = idActor, RolUsuario = TiposUsuario.Coordinador, Entidad = "USUARIO", IdEntidad = $"{tipo}:{id}",
                Operacion = "MODIFICACION_USUARIO", Detalle = string.Join("; ", cambios)
            });
        await _context.SaveChangesAsync();
        return Mapear(usuario, tipo);
    }

    private async Task<bool> CorreoExisteAsync(string correo, int? excluirId = null)
    {
        var normalizado = correo.Trim().ToUpperInvariant();
        return await _context.Estudiantes.AnyAsync(u => u.Correo.ToUpper() == normalizado && (!excluirId.HasValue || u.Id != excluirId.Value))
            || await _context.Coordinadores.AnyAsync(u => u.Correo.ToUpper() == normalizado && (!excluirId.HasValue || u.Id != excluirId.Value))
            || await _context.Evaluadores.AnyAsync(u => u.Correo.ToUpper() == normalizado && (!excluirId.HasValue || u.Id != excluirId.Value));
    }

    private async Task<Usuario?> ObtenerAsync(int id, string tipo) => tipo switch
    {
        TiposUsuario.Estudiante => await _context.Estudiantes.SingleOrDefaultAsync(u => u.Id == id),
        TiposUsuario.Coordinador => await _context.Coordinadores.SingleOrDefaultAsync(u => u.Id == id),
        TiposUsuario.Evaluador => await _context.Evaluadores.SingleOrDefaultAsync(u => u.Id == id),
        _ => null
    };

    private static Estudiante CrearEstudiante(CrearUsuarioGestionDTO dto, string correo)
    {
        if (string.IsNullOrWhiteSpace(dto.NivelAcademico))
            throw new InvalidOperationException("Indica el nivel académico del estudiante.");

        return new Estudiante
        {
            Nombre = dto.Nombre.Trim(), Correo = correo,
            Contrasena = PasswordHasher.Hashear(dto.Contrasena), Telefono = Limpiar(dto.Telefono),
            NivelAcademico = dto.NivelAcademico.Trim(),
            InstitucionProcedencia = Limpiar(dto.InstitucionProcedencia)
        };
    }

    private static string NormalizarTipo(string tipo) => tipo.Trim().ToUpperInvariant() switch
    {
        TiposUsuario.Estudiante => TiposUsuario.Estudiante,
        TiposUsuario.Coordinador => TiposUsuario.Coordinador,
        TiposUsuario.Evaluador => TiposUsuario.Evaluador,
        _ => throw new InvalidOperationException("Selecciona un rol válido.")
    };

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static void RegistrarCambio(List<string> cambios, string campo, string? anterior, string? nuevo)
    {
        if (string.Equals(anterior, nuevo, StringComparison.Ordinal)) return;
        cambios.Add($"{campo}: [{Limitar(anterior)}] -> [{Limitar(nuevo)}]");
    }

    private static string Limitar(string? texto) => string.IsNullOrEmpty(texto) ? "(vacío)" : texto.Length <= 250 ? texto : texto[..250] + "…";

    private static UsuarioGestionDTO Mapear(Usuario usuario, string tipo) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Correo = usuario.Correo,
        Telefono = usuario.Telefono,
        TipoUsuario = tipo,
        FechaRegistro = usuario.FechaRegistro,
        NivelAcademico = (usuario as Estudiante)?.NivelAcademico,
        InstitucionProcedencia = (usuario as Estudiante)?.InstitucionProcedencia,
        Cargo = (usuario as CoordinadorBecas)?.Cargo,
        Especialidad = (usuario as EvaluadorComite)?.Especialidad
    };
}
