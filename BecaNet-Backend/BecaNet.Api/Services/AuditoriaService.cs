using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IAuditoriaService
{
    Task<List<RegistroAuditoriaDTO>> ObtenerPorSolicitudAsync(int idSolicitud);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly BecaNetDbContext _context;
    public AuditoriaService(BecaNetDbContext context) => _context = context;

    public async Task<List<RegistroAuditoriaDTO>> ObtenerPorSolicitudAsync(int idSolicitud) =>
        await _context.RegistrosAuditoria.AsNoTracking()
            .Where(r => r.IdSolicitud == idSolicitud)
            .OrderByDescending(r => r.FechaHoraUtc)
            .Select(r => new RegistroAuditoriaDTO
            {
                Id = r.Id,
                IdSolicitud = r.IdSolicitud,
                IdUsuario = r.IdUsuario,
                RolUsuario = r.RolUsuario,
                Operacion = r.Operacion,
                Detalle = r.Detalle,
                FechaHoraUtc = r.FechaHoraUtc
            })
            .ToListAsync();
}
