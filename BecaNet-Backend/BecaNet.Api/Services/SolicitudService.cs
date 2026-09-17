using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Services;

public interface ISolicitudService
{
    Task<SolicitudDTO> CrearSolicitudAsync(CrearSolicitudDTO dto);
    Task<SolicitudDTO?> ObtenerPorIdAsync(int id);
    Task<List<SolicitudDTO>> ObtenerPorEstudianteAsync(int idEstudiante);
    Task CancelarSolicitudAsync(int id, CancelarSolicitudDTO dto);
}

/// <summary>
/// Reglas de negocio del Módulo de gestión de solicitudes de beca (US-007).
/// </summary>
public class SolicitudService : ISolicitudService
{
    private readonly ISolicitudRepository _solicitudRepo;
    private readonly BecaNetDbContext _context; // para validar convocatoria/estudiante

    public SolicitudService(ISolicitudRepository solicitudRepo, BecaNetDbContext context)
    {
        _solicitudRepo = solicitudRepo;
        _context = context;
    }

    public async Task<SolicitudDTO> CrearSolicitudAsync(CrearSolicitudDTO dto)
    {
        var estudiante = await _context.Estudiantes.FindAsync(dto.IdEstudiante)
            ?? throw new InvalidOperationException("El estudiante no existe.");

        var convocatoria = await _context.Convocatorias.FindAsync(dto.IdConvocatoria)
            ?? throw new InvalidOperationException("La convocatoria no existe.");

        // Criterio de aceptación: solo se puede postular a convocatorias abiertas
        if (convocatoria.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede postular a convocatorias en estado ABIERTA.");

        // Criterio de aceptación: solo puedo tener una solicitud activa por convocatoria
        var yaExiste = await _solicitudRepo.ExisteSolicitudActivaAsync(dto.IdEstudiante, dto.IdConvocatoria);
        if (yaExiste)
            throw new InvalidOperationException("Ya existe una solicitud activa para esta convocatoria.");

        var solicitud = new Solicitud
        {
            IdEstudiante = dto.IdEstudiante,
            IdConvocatoria = dto.IdConvocatoria,
            Observaciones = dto.Observaciones,
            Estado = EstadoSolicitud.EnProceso, // criterio: estado inicial "En proceso"
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

        // Criterio de aceptación: solo se puede cancelar mientras la convocatoria esté abierta
        if (solicitud.Convocatoria?.Estado != EstadoConvocatoria.Abierta)
            throw new InvalidOperationException("Solo se puede cancelar la solicitud mientras la convocatoria esté ABIERTA.");

        solicitud.Estado = EstadoSolicitud.Cancelada;
        solicitud.Observaciones = dto.Motivo ?? solicitud.Observaciones;
        await _solicitudRepo.ActualizarAsync(solicitud);
    }

    private static SolicitudDTO MapearADto(Solicitud s, string? nombreEstudiante, string? tituloConvocatoria)
    {
        return new SolicitudDTO
        {
            Id = s.Id,
            FechaCreacion = s.FechaCreacion,
            Estado = s.Estado,
            Observaciones = s.Observaciones,
            IdEstudiante = s.IdEstudiante,
            NombreEstudiante = nombreEstudiante,
            IdConvocatoria = s.IdConvocatoria,
            TituloConvocatoria = tituloConvocatoria,
            IdComite = s.IdComite,
            CantidadDocumentos = s.Documentos?.Count ?? 0
        };
    }
}
