using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;

namespace BecaNet.Api.Services;

public interface IComiteService
{
    Task<ComiteDTO> CrearComiteAsync(CrearComiteDTO dto);
    Task<List<ComiteDTO>> ObtenerTodosAsync();
    Task<ComiteDTO?> ObtenerPorIdAsync(int id);
    Task AsignarSolicitudesAsync(AsignarSolicitudesDTO dto);
}

/// <summary>
/// Reglas de negocio del Módulo de gestión de comités evaluadores (US-010, US-011).
/// </summary>
public class ComiteService : IComiteService
{
    private readonly IComiteRepository _comiteRepo;
    private readonly ISolicitudRepository _solicitudRepo;
    private readonly BecaNetDbContext _context;

    public ComiteService(IComiteRepository comiteRepo, ISolicitudRepository solicitudRepo, BecaNetDbContext context)
    {
        _comiteRepo = comiteRepo;
        _solicitudRepo = solicitudRepo;
        _context = context;
    }

    public async Task<ComiteDTO> CrearComiteAsync(CrearComiteDTO dto)
    {
        // Criterio de aceptación: un comité debe tener al menos 2 miembros
        if (dto.IdsEvaluadores.Distinct().Count() < 2)
            throw new InvalidOperationException("Un comité debe tener al menos 2 miembros asignados.");

        var evaluadores = await _comiteRepo.ObtenerEvaluadoresPorIdsAsync(dto.IdsEvaluadores);
        if (evaluadores.Count != dto.IdsEvaluadores.Distinct().Count())
            throw new InvalidOperationException("Uno o más evaluadores indicados no existen.");

        var comite = new ComiteEvaluador
        {
            Nombre = dto.Nombre,
            FechaCreacion = DateTime.Now,
            Miembros = dto.IdsEvaluadores.Distinct()
                .Select(idEval => new ComiteMiembro { IdEvaluador = idEval })
                .ToList()
        };

        var creado = await _comiteRepo.CrearAsync(comite);
        return await MapearADtoAsync(creado.Id);
    }

    public async Task<List<ComiteDTO>> ObtenerTodosAsync()
    {
        var comites = await _comiteRepo.ObtenerTodosAsync();
        return comites.Select(MapearADto).ToList();
    }

    public async Task<ComiteDTO?> ObtenerPorIdAsync(int id)
    {
        var comite = await _comiteRepo.ObtenerPorIdAsync(id);
        return comite is null ? null : MapearADto(comite);
    }

    public async Task AsignarSolicitudesAsync(AsignarSolicitudesDTO dto)
    {
        var comite = await _comiteRepo.ObtenerPorIdAsync(dto.IdComite)
            ?? throw new InvalidOperationException("El comité no existe.");

        var solicitudes = await _solicitudRepo.ObtenerPorIdsAsync(dto.IdsSolicitudes);

        if (solicitudes.Count != dto.IdsSolicitudes.Distinct().Count())
            throw new InvalidOperationException("Una o más solicitudes indicadas no existen.");

        foreach (var solicitud in solicitudes)
        {
            // Criterio de aceptación: solo se asignan solicitudes en proceso con documentación completa
            if (solicitud.Estado != EstadoSolicitud.EnProceso)
                throw new InvalidOperationException($"La solicitud {solicitud.Id} no está en estado EN_PROCESO.");

            if (solicitud.Documentos is null || solicitud.Documentos.Count == 0)
                throw new InvalidOperationException($"La solicitud {solicitud.Id} no tiene documentación cargada.");

            solicitud.IdComite = dto.IdComite;
            await _solicitudRepo.ActualizarAsync(solicitud);
        }
    }

    private async Task<ComiteDTO> MapearADtoAsync(int id)
    {
        var comite = await _comiteRepo.ObtenerPorIdAsync(id);
        return MapearADto(comite!);
    }

    private static ComiteDTO MapearADto(ComiteEvaluador c)
    {
        return new ComiteDTO
        {
            Id = c.Id,
            Nombre = c.Nombre,
            FechaCreacion = c.FechaCreacion,
            Miembros = c.Miembros?.Select(m => m.Evaluador?.Nombre ?? $"Evaluador #{m.IdEvaluador}").ToList() ?? new(),
            SolicitudesAsignadas = c.SolicitudesAsignadas?.Count ?? 0
        };
    }
}
