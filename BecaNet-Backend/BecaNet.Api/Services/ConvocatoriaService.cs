using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;

namespace BecaNet.Api.Services;

public interface IConvocatoriaService
{
    Task<ConvocatoriaDTO> CrearAsync(CrearConvocatoriaDTO dto);
    Task<ConvocatoriaDTO> PublicarAsync(int id);
    Task<ConvocatoriaDTO> CerrarAsync(int id);
    Task<List<ConvocatoriaDTO>> ObtenerAbiertasAsync();
    Task<List<ConvocatoriaDTO>> ObtenerTodasAsync();
}

public class ConvocatoriaService : IConvocatoriaService
{
    private readonly IConvocatoriaRepository _convocatoriaRepo;

    public ConvocatoriaService(IConvocatoriaRepository convocatoriaRepo)
    {
        _convocatoriaRepo = convocatoriaRepo;
    }

    /// <summary>US-004: crear convocatoria con requisitos y fechas.</summary>
    public async Task<ConvocatoriaDTO> CrearAsync(CrearConvocatoriaDTO dto)
    {
        // Criterio de aceptación: no se permite fecha de cierre anterior a la de apertura
        if (dto.FechaCierre <= dto.FechaApertura)
            throw new InvalidOperationException("La fecha de cierre debe ser posterior a la fecha de apertura.");

        var convocatoria = new Convocatoria
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Requisitos = dto.Requisitos,
            FechaApertura = dto.FechaApertura,
            FechaCierre = dto.FechaCierre,
            IdCoordinador = dto.IdCoordinador,
            Estado = EstadoConvocatoria.Borrador // criterio: se guarda como Borrador hasta publicarse
        };

        var creada = await _convocatoriaRepo.CrearAsync(convocatoria);
        return MapearADto(creada);
    }

    /// <summary>US-005: publicar (Borrador -> Abierta).</summary>
    public async Task<ConvocatoriaDTO> PublicarAsync(int id)
    {
        var convocatoria = await _convocatoriaRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La convocatoria no existe.");

        if (convocatoria.Estado != EstadoConvocatoria.Borrador)
            throw new InvalidOperationException("Solo se puede publicar una convocatoria que esté en Borrador.");

        convocatoria.Estado = EstadoConvocatoria.Abierta;
        await _convocatoriaRepo.ActualizarAsync(convocatoria);
        return MapearADto(convocatoria);
    }

    /// <summary>US-005: cerrar (Abierta -> Cerrada). Ya no se reciben nuevas solicitudes.</summary>
    public async Task<ConvocatoriaDTO> CerrarAsync(int id)
    {
        var convocatoria = await _convocatoriaRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La convocatoria no existe.");

        if (convocatoria.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede cerrar una convocatoria que esté Abierta.");

        convocatoria.Estado = EstadoConvocatoria.Cerrada;
        await _convocatoriaRepo.ActualizarAsync(convocatoria);
        return MapearADto(convocatoria);
    }

    /// <summary>US-006: listado de convocatorias abiertas para el estudiante.</summary>
    public async Task<List<ConvocatoriaDTO>> ObtenerAbiertasAsync()
    {
        var convocatorias = await _convocatoriaRepo.ObtenerAbiertasAsync();
        return convocatorias.Select(MapearADto).ToList();
    }

    public async Task<List<ConvocatoriaDTO>> ObtenerTodasAsync()
    {
        var convocatorias = await _convocatoriaRepo.ObtenerTodasAsync();
        return convocatorias.Select(MapearADto).ToList();
    }

    private static ConvocatoriaDTO MapearADto(Convocatoria c) => new()
    {
        Id = c.Id,
        Titulo = c.Titulo,
        Descripcion = c.Descripcion,
        Requisitos = c.Requisitos,
        FechaApertura = c.FechaApertura,
        FechaCierre = c.FechaCierre,
        Estado = c.Estado,
        CantidadSolicitudes = c.Solicitudes?.Count ?? 0
    };
}