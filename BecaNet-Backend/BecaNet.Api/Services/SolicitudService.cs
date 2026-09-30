using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;

namespace BecaNet.Api.Services;

public interface ISolicitudService
{
    Task<SolicitudDTO> CrearSolicitudAsync(CrearSolicitudDTO dto);
    Task<SolicitudDTO?> ObtenerPorIdAsync(int id);
    Task<List<SolicitudDTO>> ObtenerPorEstudianteAsync(int idEstudiante);
    Task CancelarSolicitudAsync(int id, CancelarSolicitudDTO dto);
    Task<SolicitudDTO> ResolverAsync(int id, ResolverSolicitudDTO dto);
}

public class SolicitudService : ISolicitudService
{
    private readonly ISolicitudRepository _solicitudRepo;
    private readonly BecaNetDbContext _context;
    private readonly SolicitudNotificador _notificador;

    public SolicitudService(ISolicitudRepository solicitudRepo, BecaNetDbContext context, SolicitudNotificador notificador)
    {
        _solicitudRepo = solicitudRepo;
        _context = context;
        _notificador = notificador;
    }

    public async Task<SolicitudDTO> CrearSolicitudAsync(CrearSolicitudDTO dto)
    {
        var estudiante = await _context.Estudiantes.FindAsync(dto.IdEstudiante)
            ?? throw new InvalidOperationException("El estudiante no existe.");

        var convocatoria = await _context.Convocatorias.FindAsync(dto.IdConvocatoria)
            ?? throw new InvalidOperationException("La convocatoria no existe.");

        if (convocatoria.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede postular a convocatorias en estado ABIERTA.");

        var yaExiste = await _solicitudRepo.ExisteSolicitudActivaAsync(dto.IdEstudiante, dto.IdConvocatoria);
        if (yaExiste)
            throw new InvalidOperationException("Ya existe una solicitud activa para esta convocatoria.");

        var solicitud = new Solicitud
        {
            IdEstudiante = dto.IdEstudiante,
            IdConvocatoria = dto.IdConvocatoria,
            Observaciones = dto.Observaciones,
            Estado = EstadoSolicitud.EnProceso,
            FechaCreacion = DateTime.Now
        };

        var creada = await _solicitudRepo.CrearAsync(solicitud);
        return MapearADto(creada, estudiante.Nombre, convocatoria.Titulo);
    }

    public async Task<SolicitudDTO?> ObtenerPorIdAsync(int id)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id);
        if (solicitud is null) return null;

        return MapearADto(solicitud, solicitud.Estudiante?.Nombre, solicitud.Convocatoria?.Titulo);
    }

    public async Task<List<SolicitudDTO>> ObtenerPorEstudianteAsync(int idEstudiante)
    {
        var solicitudes = await _solicitudRepo.ObtenerPorEstudianteAsync(idEstudiante);
        return solicitudes
            .Select(s => MapearADto(s, s.Estudiante?.Nombre, s.Convocatoria?.Titulo))
            .ToList();
    }

    public async Task CancelarSolicitudAsync(int id, CancelarSolicitudDTO dto)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (solicitud.Convocatoria?.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede cancelar la solicitud mientras la convocatoria esté ABIERTA.");

        var estadoAnterior = solicitud.Estado;
        solicitud.Estado = EstadoSolicitud.Cancelada;
        solicitud.Observaciones = dto.Motivo ?? solicitud.Observaciones;
        await _solicitudRepo.ActualizarAsync(solicitud);

        _notificador.NotificarCambioEstado(solicitud, estadoAnterior);
    }

    /// <summary>US-013: Aprobar o rechazar una solicitud ya evaluada por el comité.</summary>
    public async Task<SolicitudDTO> ResolverAsync(int id, ResolverSolicitudDTO dto)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (solicitud.Estado != EstadoSolicitud.Evaluada)
            throw new InvalidOperationException("Solo se pueden aprobar o rechazar solicitudes en estado EVALUADA.");

        if (!dto.Aprobar && string.IsNullOrWhiteSpace(dto.Motivo))
            throw new InvalidOperationException("Debes indicar un motivo para rechazar la solicitud.");

        var estadoAnterior = solicitud.Estado;
        solicitud.Estado = dto.Aprobar ? EstadoSolicitud.Aprobada : EstadoSolicitud.Rechazada;
        solicitud.MotivoResolucion = dto.Motivo;
        solicitud.FechaResolucion = DateTime.Now;

        await _solicitudRepo.ActualizarAsync(solicitud);

        _notificador.NotificarCambioEstado(solicitud, estadoAnterior);

        return MapearADto(solicitud, solicitud.Estudiante?.Nombre, solicitud.Convocatoria?.Titulo);
    }

    private static SolicitudDTO MapearADto(Solicitud s, string? nombreEstudiante, string? tituloConvocatoria)
    {
        return new SolicitudDTO
        {
            Id = s.Id,
            FechaCreacion = s.FechaCreacion,
            Estado = s.Estado,
            Observaciones = s.Observaciones,
            MotivoResolucion = s.MotivoResolucion,
            FechaResolucion = s.FechaResolucion,
            IdEstudiante = s.IdEstudiante,
            NombreEstudiante = nombreEstudiante,
            IdConvocatoria = s.IdConvocatoria,
            TituloConvocatoria = tituloConvocatoria,
            IdComite = s.IdComite,
            CantidadDocumentos = s.Documentos?.Count ?? 0
        };
    }
}