using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IAuditoriaService
{
    Task<List<RegistroAuditoriaDTO>> ObtenerPorSolicitudAsync(int idSolicitud);
    Task<List<RegistroAuditoriaDTO>> ObtenerRecientesAsync();
}

public class AuditoriaService : IAuditoriaService
{
    private readonly BecaNetDbContext _context;
    public AuditoriaService(BecaNetDbContext context) => _context = context;

    public async Task<List<RegistroAuditoriaDTO>> ObtenerPorSolicitudAsync(int idSolicitud) =>
        await MapearAsync(_context.RegistrosAuditoria.AsNoTracking()
            .Where(r => r.IdSolicitud == idSolicitud)
            .OrderByDescending(r => r.FechaHoraUtc)
            .Take(500));

    public async Task<List<RegistroAuditoriaDTO>> ObtenerRecientesAsync() =>
        await MapearAsync(_context.RegistrosAuditoria.AsNoTracking()
            .OrderByDescending(r => r.FechaHoraUtc)
            .Take(500));

    private async Task<List<RegistroAuditoriaDTO>> MapearAsync(IQueryable<RegistroAuditoria> query)
    {
        var rows = await query.ToListAsync();
        var idsEstudiante = rows.Where(r => r.RolUsuario == TiposUsuario.Estudiante).Select(r => r.IdUsuario).Distinct().ToArray();
        var idsCoordinador = rows.Where(r => r.RolUsuario == TiposUsuario.Coordinador).Select(r => r.IdUsuario).Distinct().ToArray();
        var idsEvaluador = rows.Where(r => r.RolUsuario == TiposUsuario.Evaluador).Select(r => r.IdUsuario).Distinct().ToArray();
        var actores = new Dictionary<(string Rol, int Id), (string Nombre, string Correo)>();
        foreach (var u in await _context.Estudiantes.AsNoTracking().Where(u => idsEstudiante.Contains(u.Id)).Select(u => new { u.Id, u.Nombre, u.Correo }).ToListAsync()) actores[(TiposUsuario.Estudiante, u.Id)] = (u.Nombre, u.Correo);
        foreach (var u in await _context.Coordinadores.AsNoTracking().Where(u => idsCoordinador.Contains(u.Id)).Select(u => new { u.Id, u.Nombre, u.Correo }).ToListAsync()) actores[(TiposUsuario.Coordinador, u.Id)] = (u.Nombre, u.Correo);
        foreach (var u in await _context.Evaluadores.AsNoTracking().Where(u => idsEvaluador.Contains(u.Id)).Select(u => new { u.Id, u.Nombre, u.Correo }).ToListAsync()) actores[(TiposUsuario.Evaluador, u.Id)] = (u.Nombre, u.Correo);

        return rows.Select(r =>
        {
            actores.TryGetValue((r.RolUsuario, r.IdUsuario), out var actor);
            return new RegistroAuditoriaDTO
            {
                Id = r.Id, IdSolicitud = r.IdSolicitud, Entidad = r.Entidad, IdEntidad = r.IdEntidad,
                IdUsuario = r.IdUsuario, RolUsuario = r.RolUsuario,
                NombreUsuario = actor.Nombre ?? $"Usuario #{r.IdUsuario}", CorreoUsuario = actor.Correo ?? string.Empty,
                Operacion = r.Operacion, Detalle = r.Detalle, FechaHoraUtc = r.FechaHoraUtc
            };
        }).ToList();
    }
}
