using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface IEvaluacionRepository
{
    Task<List<Evaluacion>> ObtenerPorSolicitudAsync(int idSolicitud);
    Task<bool> YaEvaluoAsync(int idSolicitud, int idEvaluador);
    Task<Evaluacion> CrearAsync(Evaluacion evaluacion);
}

public class EvaluacionRepository : IEvaluacionRepository
{
    private readonly BecaNetDbContext _context;

    public EvaluacionRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<List<Evaluacion>> ObtenerPorSolicitudAsync(int idSolicitud)
    {
        return await _context.Evaluaciones
            .Include(e => e.Evaluador)
            .Where(e => e.IdSolicitud == idSolicitud)
            .ToListAsync();
    }

    public async Task<bool> YaEvaluoAsync(int idSolicitud, int idEvaluador)
    {
        return await _context.Evaluaciones.AnyAsync(e =>
            e.IdSolicitud == idSolicitud && e.IdEvaluador == idEvaluador);
    }

    public async Task<Evaluacion> CrearAsync(Evaluacion evaluacion)
    {
        _context.Evaluaciones.Add(evaluacion);
        await _context.SaveChangesAsync();
        return evaluacion;
    }
}
