using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface INotificacionService
{
    Task<List<NotificacionSolicitudDTO>> ObtenerMiasAsync(int idEstudiante);
}

public class NotificacionService : INotificacionService
{
    private readonly BecaNetDbContext _context;
    public NotificacionService(BecaNetDbContext context) => _context = context;

    public async Task<List<NotificacionSolicitudDTO>> ObtenerMiasAsync(int idEstudiante)
    {
        var filas = await _context.Notificaciones.AsNoTracking()
            .Where(n => n.IdEstudiante == idEstudiante)
            .OrderByDescending(n => n.FechaHoraUtc)
            .Take(20)
            .Select(n => new NotificacionSolicitudDTO
            {
                Id = n.Id,
                IdSolicitud = n.IdSolicitud,
                EstadoNuevo = n.EstadoNuevo,
                FechaHoraUtc = n.FechaHoraUtc
            })
            .ToListAsync();

        return filas.Select(n =>
        {
            n.Mensaje = $"Tu solicitud #{n.IdSolicitud} cambió a {n.EstadoNuevo}.";
            return n;
        }).ToList();
    }
}
