using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface IEvaluacionService
{
    Task<EvaluacionDTO> RegistrarEvaluacionAsync(RegistrarEvaluacionDTO dto);
    Task<List<EvaluacionDTO>> ObtenerPorSolicitudAsync(int idSolicitud);
}

public class EvaluacionService : IEvaluacionService
{
    private readonly IEvaluacionRepository _evaluacionRepo;
    private readonly IComiteRepository _comiteRepo;
    private readonly ISolicitudRepository _solicitudRepo;
    private readonly BecaNetDbContext _context;
    private readonly SolicitudNotificador _notificador;

    public EvaluacionService(
        IEvaluacionRepository evaluacionRepo,
        IComiteRepository comiteRepo,
        ISolicitudRepository solicitudRepo,
        BecaNetDbContext context,
        SolicitudNotificador notificador)
    {
        _evaluacionRepo = evaluacionRepo;
        _comiteRepo = comiteRepo;
        _solicitudRepo = solicitudRepo;
        _context = context;
        _notificador = notificador;
    }

    public async Task<EvaluacionDTO> RegistrarEvaluacionAsync(RegistrarEvaluacionDTO dto)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(dto.IdSolicitud)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (solicitud.IdComite is null)
            throw new InvalidOperationException("La solicitud todavía no tiene un comité evaluador asignado.");

        var esMiembro = await _comiteRepo.EsMiembroAsync(solicitud.IdComite.Value, dto.IdEvaluador);
        if (!esMiembro)
            throw new InvalidOperationException("El evaluador no pertenece al comité asignado a esta solicitud.");

        var yaEvaluo = await _evaluacionRepo.YaEvaluoAsync(dto.IdSolicitud, dto.IdEvaluador);
        if (yaEvaluo)
            throw new InvalidOperationException("Este evaluador ya registró una evaluación para esta solicitud.");

        var evaluacion = new Evaluacion
        {
            IdSolicitud = dto.IdSolicitud,
            IdEvaluador = dto.IdEvaluador,
            Puntaje = dto.Puntaje,
            Observaciones = dto.Observaciones,
            FechaEvaluacion = DateTime.Now
        };

        var creada = await _evaluacionRepo.CrearAsync(evaluacion);

        // Cuando todos los miembros del comité evaluaron, la solicitud pasa a "Evaluada"
        // y el patrón Observer notifica automáticamente (estudiante + auditoría).
        await ActualizarEstadoSiCorrespondeAsync(solicitud.Id, solicitud.IdComite.Value);

        var evaluador = await _context.Evaluadores.FindAsync(dto.IdEvaluador);

        return new EvaluacionDTO
        {
            Id = creada.Id,
            Puntaje = creada.Puntaje,
            Observaciones = creada.Observaciones,
            FechaEvaluacion = creada.FechaEvaluacion,
            IdSolicitud = creada.IdSolicitud,
            IdEvaluador = creada.IdEvaluador,
            NombreEvaluador = evaluador?.Nombre
        };
    }

    public async Task<List<EvaluacionDTO>> ObtenerPorSolicitudAsync(int idSolicitud)
    {
        var evaluaciones = await _evaluacionRepo.ObtenerPorSolicitudAsync(idSolicitud);
        return evaluaciones.Select(e => new EvaluacionDTO
        {
            Id = e.Id,
            Puntaje = e.Puntaje,
            Observaciones = e.Observaciones,
            FechaEvaluacion = e.FechaEvaluacion,
            IdSolicitud = e.IdSolicitud,
            IdEvaluador = e.IdEvaluador,
            NombreEvaluador = e.Evaluador?.Nombre
        }).ToList();
    }

    private async Task ActualizarEstadoSiCorrespondeAsync(int idSolicitud, int idComite)
    {
        var comite = await _comiteRepo.ObtenerPorIdAsync(idComite);
        var totalMiembros = comite?.Miembros.Count ?? 0;

        var evaluacionesRegistradas = await _context.Evaluaciones
            .CountAsync(e => e.IdSolicitud == idSolicitud);

        if (totalMiembros > 0 && evaluacionesRegistradas >= totalMiembros)
        {
            var solicitud = await _context.Solicitudes.FindAsync(idSolicitud);
            if (solicitud is not null && solicitud.Estado == EstadoSolicitud.EnProceso)
            {
                var estadoAnterior = solicitud.Estado;
                solicitud.Estado = EstadoSolicitud.Evaluada;
                await _context.SaveChangesAsync();

                _notificador.NotificarCambioEstado(solicitud, estadoAnterior);
            }
        }
    }
}