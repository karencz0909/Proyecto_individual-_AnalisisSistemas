using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>US-007: crear una solicitud de beca (versión con id explícito, para pruebas en Swagger).</summary>
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

    /// <summary>El estudiante autenticado crea SU solicitud: el id sale del token, no se escribe.</summary>
    [HttpPost("mia")]
    [Authorize(Roles = TiposUsuario.Estudiante)]
    public async Task<ActionResult<SolicitudDTO>> CrearMia([FromBody] CrearMiSolicitudDTO dto)
    {
        var idEstudiante = User.ObtenerIdUsuario();
        if (idEstudiante is null) return Unauthorized();

        try
        {
            var creada = await _solicitudService.CrearSolicitudAsync(new CrearSolicitudDTO
            {
                IdEstudiante = idEstudiante.Value,
                IdConvocatoria = dto.IdConvocatoria,
                Motivo = dto.Motivo.Trim()
            });
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Las solicitudes del estudiante autenticado, sin tener que escribir su id.</summary>
    [HttpGet("mias")]
    [Authorize(Roles = TiposUsuario.Estudiante)]
    public async Task<ActionResult<List<SolicitudDTO>>> ObtenerMias()
    {
        var idEstudiante = User.ObtenerIdUsuario();
        if (idEstudiante is null) return Unauthorized();

        return Ok(await _solicitudService.ObtenerPorEstudianteAsync(idEstudiante.Value));
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

    /// <summary>US-009: el estudiante cancela una solicitud suya.</summary>
    [HttpPut("{id}/cancelar")]
    [Authorize(Roles = TiposUsuario.Estudiante)]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarSolicitudDTO dto)
    {
        var idEstudiante = User.ObtenerIdUsuario();
        var solicitud = await _solicitudService.ObtenerPorIdAsync(id);
        if (solicitud is null) return NotFound(new { mensaje = "La solicitud no existe." });
        if (solicitud.IdEstudiante != idEstudiante) return Forbid();

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

    /// <summary>US-013: el coordinador aprueba o rechaza una solicitud ya evaluada.</summary>
    [HttpPut("{id}/resolver")]
    [Authorize(Roles = TiposUsuario.Coordinador)]
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
