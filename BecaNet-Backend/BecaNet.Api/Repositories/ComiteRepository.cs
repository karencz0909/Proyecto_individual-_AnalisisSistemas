using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface IComiteRepository
{
    Task<ComiteEvaluador?> ObtenerPorIdAsync(int id);
    Task<List<ComiteEvaluador>> ObtenerTodosAsync();
    Task<List<EvaluadorComite>> ObtenerEvaluadoresPorIdsAsync(List<int> ids);
    Task<ComiteEvaluador> CrearAsync(ComiteEvaluador comite);
    Task<bool> EsMiembroAsync(int idComite, int idEvaluador);
}

public class ComiteRepository : IComiteRepository
{
    private readonly BecaNetDbContext _context;

    public ComiteRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<ComiteEvaluador?> ObtenerPorIdAsync(int id)
    {
        return await _context.Comites
            .Include(c => c.Miembros).ThenInclude(m => m.Evaluador)
            .Include(c => c.SolicitudesAsignadas)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<ComiteEvaluador>> ObtenerTodosAsync()
    {
        return await _context.Comites
            .Include(c => c.Miembros).ThenInclude(m => m.Evaluador)
            .Include(c => c.SolicitudesAsignadas)
            .ToListAsync();
    }

    public async Task<List<EvaluadorComite>> ObtenerEvaluadoresPorIdsAsync(List<int> ids)
    {
        return await _context.Evaluadores.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public async Task<ComiteEvaluador> CrearAsync(ComiteEvaluador comite)
    {
        _context.Comites.Add(comite);
        await _context.SaveChangesAsync();
        return comite;
    }

    public async Task<bool> EsMiembroAsync(int idComite, int idEvaluador)
    {
        return await _context.ComiteMiembros.AnyAsync(m =>
            m.IdComite == idComite && m.IdEvaluador == idEvaluador);
    }
}
