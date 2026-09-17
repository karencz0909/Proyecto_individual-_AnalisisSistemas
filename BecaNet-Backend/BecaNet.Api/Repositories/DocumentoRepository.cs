using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface IDocumentoRepository
{
    Task<List<Documento>> ObtenerPorSolicitudAsync(int idSolicitud);
    Task<Documento> CrearAsync(Documento documento);
}

public class DocumentoRepository : IDocumentoRepository
{
    private readonly BecaNetDbContext _context;

    public DocumentoRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<List<Documento>> ObtenerPorSolicitudAsync(int idSolicitud)
    {
        return await _context.Documentos
            .Where(d => d.IdSolicitud == idSolicitud)
            .OrderByDescending(d => d.FechaCarga)
            .ToListAsync();
    }

    public async Task<Documento> CrearAsync(Documento documento)
    {
        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync();
        return documento;
    }
}
