using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface IConvocatoriaRepository
{
    Task<Convocatoria?> ObtenerPorIdAsync(int id);
    Task<List<Convocatoria>> ObtenerAbiertasAsync();
    Task<List<Convocatoria>> ObtenerTodasAsync();
    Task<Convocatoria> CrearAsync(Convocatoria convocatoria);
    Task ActualizarAsync(Convocatoria convocatoria);
}

public class ConvocatoriaRepository : IConvocatoriaRepository
{
    private readonly BecaNetDbContext _context;

    public ConvocatoriaRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<Convocatoria?> ObtenerPorIdAsync(int id)
    {
        return await _context.Convocatorias
            .Include(c => c.Solicitudes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Convocatoria>> ObtenerAbiertasAsync()
    {
        return await _context.Convocatorias
            .Where(c => c.Estado == EstadoConvocatoria.Abierta)
            .OrderBy(c => c.FechaCierre)
            .ToListAsync();
    }

    public async Task<List<Convocatoria>> ObtenerTodasAsync()
    {
        return await _context.Convocatorias
            .Include(c => c.Solicitudes)
            .OrderByDescending(c => c.FechaApertura)
            .ToListAsync();
    }

    public async Task<Convocatoria> CrearAsync(Convocatoria convocatoria)
    {
        _context.Convocatorias.Add(convocatoria);
        await _context.SaveChangesAsync();
        return convocatoria;
    }

    public async Task ActualizarAsync(Convocatoria convocatoria)
    {
        _context.Convocatorias.Update(convocatoria);
        await _context.SaveChangesAsync();
    }
}