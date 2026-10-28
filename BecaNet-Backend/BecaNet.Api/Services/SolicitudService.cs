using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface ISolicitudService
{
    Task<SolicitudDTO> CrearSolicitudAsync(CrearSolicitudDTO dto);
    Task<SolicitudDTO?> ObtenerPorIdAsync(int id);
    Task<List<SolicitudDTO>> ObtenerPorEstudianteAsync(int idEstudiante);
    Task CancelarSolicitudAsync(int id, CancelarSolicitudDTO dto, int idUsuario);
    Task<SolicitudDTO> ResolverAsync(int id, ResolverSolicitudDTO dto, int idUsuario);
    Task<SolicitudDTO> AprobarComoEvaluadorAsync(int id, RegistrarAprobacionDTO dto, int idEvaluador);
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
            Motivo = dto.Motivo,
            MontoSolicitado = dto.MontoSolicitado,
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

    public async Task CancelarSolicitudAsync(int id, CancelarSolicitudDTO dto, int idUsuario)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (solicitud.Convocatoria?.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede cancelar la solicitud mientras la convocatoria esté ABIERTA.");

        var estadoAnterior = solicitud.Estado;
        var observacionesAnteriores = solicitud.Observaciones;
        solicitud.Estado = EstadoSolicitud.Cancelada;
        solicitud.Observaciones = dto.Motivo ?? solicitud.Observaciones;
        _context.Notificaciones.Add(new NotificacionSolicitud { IdSolicitud = id, IdEstudiante = solicitud.IdEstudiante, EstadoAnterior = estadoAnterior, EstadoNuevo = solicitud.Estado });
        _context.RegistrosAuditoria.Add(new RegistroAuditoria { IdSolicitud = id, IdUsuario = idUsuario, RolUsuario = TiposUsuario.Estudiante, Operacion = "CANCELACION_SOLICITUD", Detalle = $"Estado: {estadoAnterior} -> {solicitud.Estado}; Observaciones: [{Recortar(observacionesAnteriores)}] -> [{Recortar(solicitud.Observaciones)}]", FechaHoraUtc = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        try { _notificador.NotificarCambioEstado(solicitud, estadoAnterior); } catch { /* No se revierte el estado si falla un canal. */ }
    }

    /// <summary>US-013: Aprobar o rechazar una solicitud ya evaluada por el comité.</summary>
    public async Task<SolicitudDTO> ResolverAsync(int id, ResolverSolicitudDTO dto, int idUsuario)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (solicitud.Estado != EstadoSolicitud.Evaluada)
            throw new InvalidOperationException("Solo se pueden aprobar o rechazar solicitudes en estado EVALUADA.");

        if (!dto.Aprobar && string.IsNullOrWhiteSpace(dto.Motivo))
            throw new InvalidOperationException("Debes indicar un motivo para rechazar la solicitud.");

        if (solicitud.MontoSolicitado > 10_000)
        {
            var decisionEvaluador = await _context.Aprobaciones.SingleOrDefaultAsync(a =>
                a.IdSolicitud == id && a.Tipo == TipoAprobacionSolicitud.Evaluador);
            if (dto.Aprobar && decisionEvaluador is null)
                throw new InvalidOperationException("El evaluador asignado debe registrar su decisión antes de la autorización del coordinador.");
            if (decisionEvaluador is not null && !decisionEvaluador.Aprobada && dto.Aprobar)
                throw new InvalidOperationException("La solicitud fue rechazada por el evaluador y no puede autorizarse.");
            if (decisionEvaluador?.IdUsuario == idUsuario)
                throw new InvalidOperationException("Una misma persona no puede realizar ambas aprobaciones.");
        }

        var estadoAnterior = solicitud.Estado;
        solicitud.Estado = dto.Aprobar ? EstadoSolicitud.Aprobada : EstadoSolicitud.Rechazada;
        solicitud.MotivoResolucion = dto.Motivo;
        solicitud.FechaResolucion = DateTime.Now;
        var ahora = DateTime.UtcNow;
        _context.Aprobaciones.Add(new AprobacionSolicitud { IdSolicitud = id, Tipo = TipoAprobacionSolicitud.Coordinador, IdUsuario = idUsuario, RolUsuario = TiposUsuario.Coordinador, Aprobada = dto.Aprobar, Motivo = dto.Motivo, FechaHoraUtc = ahora });
        _context.RegistrosAuditoria.Add(new RegistroAuditoria { IdSolicitud = id, IdUsuario = idUsuario, RolUsuario = TiposUsuario.Coordinador, Operacion = dto.Aprobar ? "APROBACION_COORDINADOR" : "RECHAZO_COORDINADOR", Detalle = $"Resultado: {solicitud.Estado}; estado: {estadoAnterior} -> {solicitud.Estado}; monto: Q{solicitud.MontoSolicitado:N2}; motivo: {Recortar(dto.Motivo)}", FechaHoraUtc = ahora });
        _context.Notificaciones.Add(new NotificacionSolicitud { IdSolicitud = id, IdEstudiante = solicitud.IdEstudiante, EstadoAnterior = estadoAnterior, EstadoNuevo = solicitud.Estado, FechaHoraUtc = ahora });
        await _context.SaveChangesAsync();
        try { _notificador.NotificarCambioEstado(solicitud, estadoAnterior); } catch { /* El cambio y el evento quedan persistidos. */ }

        return MapearADto(solicitud, solicitud.Estudiante?.Nombre, solicitud.Convocatoria?.Titulo);
    }

    public async Task<SolicitudDTO> AprobarComoEvaluadorAsync(int id, RegistrarAprobacionDTO dto, int idEvaluador)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id) ?? throw new InvalidOperationException("La solicitud no existe.");
        if (solicitud.MontoSolicitado <= 10_000) throw new InvalidOperationException("Esta solicitud no requiere aprobación adicional del evaluador.");
        if (solicitud.IdEvaluadorAsignado != idEvaluador) throw new UnauthorizedAccessException("Solo el evaluador asignado puede registrar esta decisión.");
        if (solicitud.Estado != EstadoSolicitud.Evaluada || solicitud.EstadoRevision != EstadosRevision.Revisado)
            throw new InvalidOperationException("La solicitud debe estar revisada antes de registrar la decisión del evaluador.");
        if (!dto.Aprobar && string.IsNullOrWhiteSpace(dto.Motivo)) throw new InvalidOperationException("Debes indicar el motivo del rechazo.");
        if (await _context.Aprobaciones.AnyAsync(a => a.IdSolicitud == id && a.Tipo == TipoAprobacionSolicitud.Evaluador))
            throw new InvalidOperationException("El evaluador ya registró su decisión para esta solicitud.");

        var anterior = solicitud.Estado;
        var ahora = DateTime.UtcNow;
        _context.Aprobaciones.Add(new AprobacionSolicitud { IdSolicitud = id, Tipo = TipoAprobacionSolicitud.Evaluador, IdUsuario = idEvaluador, RolUsuario = TiposUsuario.Evaluador, Aprobada = dto.Aprobar, Motivo = dto.Motivo, FechaHoraUtc = ahora });
        _context.RegistrosAuditoria.Add(new RegistroAuditoria { IdSolicitud = id, IdUsuario = idEvaluador, RolUsuario = TiposUsuario.Evaluador, Operacion = dto.Aprobar ? "APROBACION_EVALUADOR" : "RECHAZO_EVALUADOR", Detalle = $"Resultado: {(dto.Aprobar ? "APROBADA" : "RECHAZADA")}; monto: Q{solicitud.MontoSolicitado:N2}; motivo: {Recortar(dto.Motivo)}", FechaHoraUtc = ahora });
        if (!dto.Aprobar)
        {
            solicitud.Estado = EstadoSolicitud.Rechazada;
            solicitud.MotivoResolucion = dto.Motivo;
            solicitud.FechaResolucion = DateTime.Now;
            _context.Notificaciones.Add(new NotificacionSolicitud { IdSolicitud = id, IdEstudiante = solicitud.IdEstudiante, EstadoAnterior = anterior, EstadoNuevo = solicitud.Estado, FechaHoraUtc = ahora });
        }
        await _context.SaveChangesAsync();
        if (anterior != solicitud.Estado)
            try { _notificador.NotificarCambioEstado(solicitud, anterior); } catch { /* No se revierte el rechazo si falla un canal. */ }
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
            EstadoRevision = s.EstadoRevision,
            Motivo = s.Motivo,
            MontoSolicitado = s.MontoSolicitado,
            MotivoResolucion = s.MotivoResolucion,
            FechaResolucion = s.FechaResolucion,
            IdEstudiante = s.IdEstudiante,
            NombreEstudiante = nombreEstudiante,
            IdConvocatoria = s.IdConvocatoria,
            TituloConvocatoria = tituloConvocatoria,
            IdComite = s.IdComite,
            CantidadDocumentos = s.Documentos?.Count ?? 0,
            TieneCertificacionEstudios = s.Documentos?.Any(d => d.TipoDocumento == TiposDocumento.CertificacionEstudios) ?? false,
            Aprobaciones = s.Aprobaciones?.Select(a => new AprobacionSolicitudDTO { Tipo = a.Tipo, IdUsuario = a.IdUsuario, RolUsuario = a.RolUsuario, Aprobada = a.Aprobada, Motivo = a.Motivo, FechaHoraUtc = a.FechaHoraUtc }).ToList() ?? new()
        };
    }

    private static string Recortar(string? texto) =>
        string.IsNullOrEmpty(texto) ? "(vacío)" : texto.Length <= 650 ? texto : texto[..650] + "…";
}
