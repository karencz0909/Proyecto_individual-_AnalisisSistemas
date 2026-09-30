using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using BecaNet.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;

    public SolicitudesController(ISolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    /// <summary>US-007: crear una solicitud de beca.</summary>
    [HttpPost]
    public async Task<ActionResult<SolicitudDTO>> Crear([FromBody] CrearSolicitudDTO dto)
    {
        try
        {
            var creada = await _solicitudService.CrearSolicitudAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SolicitudDTO>> ObtenerPorId(int id)
    {
        var solicitud = await _solicitudService.ObtenerPorIdAsync(id);
        return solicitud is null ? NotFound() : Ok(solicitud);
    }

    [HttpGet("estudiante/{idEstudiante}")]
    public async Task<ActionResult<List<SolicitudDTO>>> ObtenerPorEstudiante(int idEstudiante)
    {
        return Ok(await _solicitudService.ObtenerPorEstudianteAsync(idEstudiante));
    }

    /// <summary>US-009: cancelar una solicitud.</summary>
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarSolicitudDTO dto)
    {
        try
        {
            await _solicitudService.CancelarSolicitudAsync(id, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-013: aprobar o rechazar una solicitud ya evaluada (Fase 3).</summary>
    [HttpPut("{id}/resolver")]
    public async Task<ActionResult<SolicitudDTO>> Resolver(int id, [FromBody] ResolverSolicitudDTO dto)
    {
        try
        {
            return Ok(await _solicitudService.ResolverAsync(id, dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}