using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface ISolicitudRepository
{
    Task<Solicitud?> ObtenerPorIdAsync(int id);
    Task<List<Solicitud>> ObtenerPorEstudianteAsync(int idEstudiante);
    Task<List<Solicitud>> ObtenerPorIdsAsync(List<int> ids);
    Task<bool> ExisteSolicitudActivaAsync(int idEstudiante, int idConvocatoria);
    Task<Solicitud> CrearAsync(Solicitud solicitud);
    Task ActualizarAsync(Solicitud solicitud);
}

public class SolicitudRepository : ISolicitudRepository
{
    private readonly BecaNetDbContext _context;

    public SolicitudRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<Solicitud?> ObtenerPorIdAsync(int id)
    {
        return await _context.Solicitudes
            .Include(s => s.Estudiante)
            .Include(s => s.Convocatoria)
            .Include(s => s.Documentos)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Solicitud>> ObtenerPorEstudianteAsync(int idEstudiante)
    {
        return await _context.Solicitudes
            .Include(s => s.Convocatoria)
            .Include(s => s.Documentos)
            .Where(s => s.IdEstudiante == idEstudiante)
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Solicitud>> ObtenerPorIdsAsync(List<int> ids)
    {
        return await _context.Solicitudes
            .Include(s => s.Documentos)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<bool> ExisteSolicitudActivaAsync(int idEstudiante, int idConvocatoria)
    {
        return await _context.Solicitudes.AnyAsync(s =>
            s.IdEstudiante == idEstudiante &&
            s.IdConvocatoria == idConvocatoria &&
            s.Estado != EstadoSolicitud.Cancelada);
    }

    public async Task<Solicitud> CrearAsync(Solicitud solicitud)
    {
        _context.Solicitudes.Add(solicitud);
        await _context.SaveChangesAsync();
        return solicitud;
    }

    public async Task ActualizarAsync(Solicitud solicitud)
    {
        _context.Solicitudes.Update(solicitud);
        await _context.SaveChangesAsync();
    }
}
