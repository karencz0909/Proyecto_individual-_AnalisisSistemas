using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IAsignacionService
{
    // Evaluador
    Task<List<AsignacionEvaluadorDTO>> ObtenerMisAsignacionesAsync(int idEvaluador);
    Task<AsignacionEvaluadorDTO> RevisarAsync(int idEvaluador, int idSolicitud, RevisarSolicitudDTO dto);
    Task<AsignacionEvaluadorDTO> ReabrirAsync(int idEvaluador, int idSolicitud);

    // Coordinador
    Task<List<SolicitudCoordinadorDTO>> ObtenerSolicitudesCoordinadorAsync();
    Task<List<EvaluadorOpcionDTO>> ObtenerEvaluadoresAsync();
    Task<ResultadoDistribucionDTO> DistribuirAsync(DistribuirDTO dto);
    Task<SolicitudCoordinadorDTO> AsignarManualAsync(int idSolicitud, int idEvaluador);
}

/// <summary>
/// Reparte las solicitudes entre los evaluadores y controla el avance de cada revisión
/// (EN_REVISION -> REVISADO). Cuando el evaluador revisa, la solicitud pasa a EVALUADA
/// y el patrón Observer avisa del cambio de estado.
/// </summary>
public class AsignacionService : IAsignacionService
{
    private readonly BecaNetDbContext _context;
    private readonly SolicitudNotificador _notificador;

    public AsignacionService(BecaNetDbContext context, SolicitudNotificador notificador)
    {
        _context = context;
        _notificador = notificador;
    }

    // ------------------------------------------------------------------
    // EVALUADOR
    // ------------------------------------------------------------------
    public async Task<List<AsignacionEvaluadorDTO>> ObtenerMisAsignacionesAsync(int idEvaluador)
    {
        var solicitudes = await _context.Solicitudes
            .Include(s => s.Estudiante)
            .Include(s => s.Convocatoria)
            .Include(s => s.Documentos)
            .Include(s => s.Evaluaciones)
            .Where(s => s.IdEvaluadorAsignado == idEvaluador && s.Estado != EstadoSolicitud.Cancelada)
            .OrderBy(s => s.FechaAsignacion)
            .ToListAsync();

        return solicitudes.Select(s => MapearAsignacion(s, idEvaluador)).ToList();
    }

    public async Task<AsignacionEvaluadorDTO> RevisarAsync(int idEvaluador, int idSolicitud, RevisarSolicitudDTO dto)
    {
        var solicitud = await CargarSolicitudAsync(idSolicitud);
        ValidarAsignada(solicitud, idEvaluador);

        if (solicitud.Estado != EstadoSolicitud.EnProceso && solicitud.Estado != EstadoSolicitud.Evaluada)
            throw new InvalidOperationException("Esta solicitud ya fue resuelta; no se puede modificar la evaluación.");

        var evaluacion = solicitud.Evaluaciones.FirstOrDefault(e => e.IdEvaluador == idEvaluador);
        if (evaluacion is null)
        {
            evaluacion = new Evaluacion { IdSolicitud = solicitud.Id, IdEvaluador = idEvaluador };
            _context.Evaluaciones.Add(evaluacion);
        }

        evaluacion.Puntaje = dto.Puntaje;
        evaluacion.Observaciones = dto.Observaciones.Trim();
        evaluacion.FechaEvaluacion = DateTime.Now;

        var estadoAnterior = solicitud.Estado;
        solicitud.EstadoRevision = EstadosRevision.Revisado;
        solicitud.Estado = EstadoSolicitud.Evaluada;

        await _context.SaveChangesAsync();

        if (estadoAnterior != solicitud.Estado)
            _notificador.NotificarCambioEstado(solicitud, estadoAnterior);

        return MapearAsignacion(solicitud, idEvaluador);
    }

    public async Task<AsignacionEvaluadorDTO> ReabrirAsync(int idEvaluador, int idSolicitud)
    {
        var solicitud = await CargarSolicitudAsync(idSolicitud);
        ValidarAsignada(solicitud, idEvaluador);

        if (solicitud.Estado == EstadoSolicitud.Aprobada
            || solicitud.Estado == EstadoSolicitud.Rechazada
            || solicitud.Estado == EstadoSolicitud.Cancelada)
            throw new InvalidOperationException("La solicitud ya fue resuelta; no se puede volver a revisión.");

        var estadoAnterior = solicitud.Estado;
        solicitud.EstadoRevision = EstadosRevision.EnRevision;
        solicitud.Estado = EstadoSolicitud.EnProceso;

        await _context.SaveChangesAsync();

        if (estadoAnterior != solicitud.Estado)
            _notificador.NotificarCambioEstado(solicitud, estadoAnterior);

        return MapearAsignacion(solicitud, idEvaluador);
    }

    // ------------------------------------------------------------------
    // COORDINADOR
    // ------------------------------------------------------------------
    public async Task<List<SolicitudCoordinadorDTO>> ObtenerSolicitudesCoordinadorAsync()
    {
        var solicitudes = await _context.Solicitudes
            .Include(s => s.Estudiante)
            .Include(s => s.Convocatoria)
            .Include(s => s.Documentos)
            .Include(s => s.Evaluaciones)
            .Include(s => s.EvaluadorAsignado)
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        return solicitudes.Select(MapearCoordinador).ToList();
    }

    public async Task<List<EvaluadorOpcionDTO>> ObtenerEvaluadoresAsync()
    {
        var cargas = await CargasPorEvaluadorAsync();
        var evaluadores = await _context.Evaluadores.OrderBy(e => e.Nombre).ToListAsync();

        return evaluadores.Select(e => new EvaluadorOpcionDTO
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Correo = e.Correo,
            Asignadas = cargas.GetValueOrDefault(e.Id)
        }).ToList();
    }

    /// <summary>
    /// Reparte las solicitudes pendientes (con certificación de estudios) entre los evaluadores,
    /// dándole siempre la siguiente al que tenga menos carga. Con 10 solicitudes y 5 evaluadores
    /// queda 2 para cada uno.
    /// </summary>
    public async Task<ResultadoDistribucionDTO> DistribuirAsync(DistribuirDTO dto)
    {
        List<EvaluadorComite> pool;

        if (dto.IdComite.HasValue)
        {
            var idComite = dto.IdComite.Value;
            var existe = await _context.Comites.AnyAsync(c => c.Id == idComite);
            if (!existe)
                throw new InvalidOperationException("El comité indicado no existe.");

            pool = await _context.Evaluadores
                .Where(e => e.Comites.Any(m => m.IdComite == idComite))
                .OrderBy(e => e.Id)
                .ToListAsync();
        }
        else
        {
            pool = await _context.Evaluadores.OrderBy(e => e.Id).ToListAsync();
        }

        if (pool.Count == 0)
            throw new InvalidOperationException("No hay evaluadores disponibles para recibir solicitudes.");

        var pendientes = await _context.Solicitudes
            .Include(s => s.Documentos)
            .Where(s => s.Estado == EstadoSolicitud.EnProceso && s.IdEvaluadorAsignado == null)
            .OrderBy(s => s.FechaCreacion)
            .ToListAsync();

        var aptas = pendientes
            .Where(s => s.Documentos.Any(d => d.TipoDocumento == TiposDocumento.CertificacionEstudios))
            .ToList();

        var cargas = await CargasPorEvaluadorAsync();
        var cargaActual = pool.ToDictionary(e => e.Id, e => cargas.GetValueOrDefault(e.Id));

        foreach (var solicitud in aptas)
        {
            var elegido = pool.OrderBy(e => cargaActual[e.Id]).ThenBy(e => e.Id).First();

            solicitud.IdEvaluadorAsignado = elegido.Id;
            solicitud.EstadoRevision = EstadosRevision.EnRevision;
            solicitud.FechaAsignacion = DateTime.Now;
            if (dto.IdComite.HasValue)
                solicitud.IdComite = dto.IdComite;

            cargaActual[elegido.Id]++;
        }

        await _context.SaveChangesAsync();

        return new ResultadoDistribucionDTO
        {
            TotalAsignadas = aptas.Count,
            OmitidasSinCertificacion = pendientes.Count - aptas.Count,
            PorEvaluador = pool.Select(e => new EvaluadorOpcionDTO
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Correo = e.Correo,
                Asignadas = cargaActual[e.Id]
            }).ToList()
        };
    }

    public async Task<SolicitudCoordinadorDTO> AsignarManualAsync(int idSolicitud, int idEvaluador)
    {
        var solicitud = await CargarSolicitudAsync(idSolicitud);

        if (solicitud.Estado != EstadoSolicitud.EnProceso)
            throw new InvalidOperationException("Solo se puede asignar o reasignar una solicitud que siga en proceso.");

        if (!solicitud.Documentos.Any(d => d.TipoDocumento == TiposDocumento.CertificacionEstudios))
            throw new InvalidOperationException("El estudiante todavía no ha subido su certificación de estudios.");

        if (solicitud.EstadoRevision == EstadosRevision.Revisado)
            throw new InvalidOperationException("Esta solicitud ya fue revisada; el evaluador debe devolverla a revisión antes de reasignarla.");

        var evaluador = await _context.Evaluadores.FindAsync(idEvaluador)
            ?? throw new InvalidOperationException("El evaluador no existe.");

        solicitud.IdEvaluadorAsignado = evaluador.Id;
        solicitud.EvaluadorAsignado = evaluador;
        solicitud.EstadoRevision = EstadosRevision.EnRevision;
        solicitud.FechaAsignacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return MapearCoordinador(solicitud);
    }

    // ------------------------------------------------------------------
    // Utilidades
    // ------------------------------------------------------------------
    private async Task<Solicitud> CargarSolicitudAsync(int idSolicitud)
    {
        var solicitud = await _context.Solicitudes
            .Include(s => s.Estudiante)
            .Include(s => s.Convocatoria)
            .Include(s => s.Documentos)
            .Include(s => s.Evaluaciones)
            .Include(s => s.EvaluadorAsignado)
            .FirstOrDefaultAsync(s => s.Id == idSolicitud);

        return solicitud ?? throw new InvalidOperationException("La solicitud no existe.");
    }

    private static void ValidarAsignada(Solicitud solicitud, int idEvaluador)
    {
        if (solicitud.IdEvaluadorAsignado != idEvaluador)
            throw new InvalidOperationException("Esta solicitud no está asignada a ti.");
    }

    private async Task<Dictionary<int, int>> CargasPorEvaluadorAsync()
    {
        var asignados = await _context.Solicitudes
            .Where(s => s.IdEvaluadorAsignado != null && s.Estado != EstadoSolicitud.Cancelada)
            .Select(s => s.IdEvaluadorAsignado!.Value)
            .ToListAsync();

        return asignados.GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());
    }

    private static DocumentoResumenDTO MapearDocumento(Documento d) => new()
    {
        Id = d.Id,
        NombreArchivo = d.NombreArchivo,
        TipoDocumento = d.TipoDocumento,
        UrlArchivo = d.UrlArchivo
    };

    private static AsignacionEvaluadorDTO MapearAsignacion(Solicitud s, int idEvaluador)
    {
        var evaluacion = s.Evaluaciones.FirstOrDefault(e => e.IdEvaluador == idEvaluador);

        return new AsignacionEvaluadorDTO
        {
            IdSolicitud = s.Id,
            NombreEstudiante = s.Estudiante?.Nombre ?? "Estudiante",
            TituloConvocatoria = s.Convocatoria?.Titulo ?? "Convocatoria",
            Motivo = s.Motivo,
            EstadoSolicitud = s.Estado,
            EstadoRevision = s.EstadoRevision,
            FechaAsignacion = s.FechaAsignacion,
            Puntaje = evaluacion?.Puntaje,
            ObservacionesEvaluacion = evaluacion?.Observaciones,
            Documentos = s.Documentos.Select(MapearDocumento).ToList()
        };
    }

    private static SolicitudCoordinadorDTO MapearCoordinador(Solicitud s)
    {
        var evaluacion = s.IdEvaluadorAsignado.HasValue
            ? s.Evaluaciones.FirstOrDefault(e => e.IdEvaluador == s.IdEvaluadorAsignado.Value)
            : null;

        return new SolicitudCoordinadorDTO
        {
            IdSolicitud = s.Id,
            FechaCreacion = s.FechaCreacion,
            NombreEstudiante = s.Estudiante?.Nombre ?? "Estudiante",
            CorreoEstudiante = s.Estudiante?.Correo ?? string.Empty,
            TituloConvocatoria = s.Convocatoria?.Titulo ?? "Convocatoria",
            Motivo = s.Motivo,
            Estado = s.Estado,
            EstadoRevision = s.EstadoRevision,
            IdEvaluadorAsignado = s.IdEvaluadorAsignado,
            NombreEvaluador = s.EvaluadorAsignado?.Nombre,
            CorreoEvaluador = s.EvaluadorAsignado?.Correo,
            Puntaje = evaluacion?.Puntaje,
            ObservacionesEvaluacion = evaluacion?.Observaciones,
            MotivoResolucion = s.MotivoResolucion,
            TieneCertificacionEstudios = s.Documentos.Any(d => d.TipoDocumento == TiposDocumento.CertificacionEstudios),
            Documentos = s.Documentos.Select(MapearDocumento).ToList()
        };
    }
}
