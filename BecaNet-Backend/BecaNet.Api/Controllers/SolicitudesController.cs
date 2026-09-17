using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
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

    /// <summary>US-007: Como estudiante, quiero crear una solicitud de beca.</summary>
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
        var solicitudes = await _solicitudService.ObtenerPorEstudianteAsync(idEstudiante);
        return Ok(solicitudes);
    }

    /// <summary>US-009: Como estudiante, quiero cancelar mi solicitud.</summary>
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
}
