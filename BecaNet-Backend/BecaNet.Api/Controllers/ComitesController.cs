using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComitesController : ControllerBase
{
    private readonly IComiteService _comiteService;

    public ComitesController(IComiteService comiteService)
    {
        _comiteService = comiteService;
    }

    /// <summary>US-010: Como coordinador, quiero crear comités evaluadores y asignarles miembros.</summary>
    [HttpPost]
    public async Task<ActionResult<ComiteDTO>> Crear([FromBody] CrearComiteDTO dto)
    {
        try
        {
            var creado = await _comiteService.CrearComiteAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ComiteDTO>>> ObtenerTodos()
    {
        return Ok(await _comiteService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComiteDTO>> ObtenerPorId(int id)
    {
        var comite = await _comiteService.ObtenerPorIdAsync(id);
        return comite is null ? NotFound() : Ok(comite);
    }

    /// <summary>US-011: Como coordinador, quiero asignar solicitudes recibidas a un comité evaluador.</summary>
    [HttpPost("asignar-solicitudes")]
    public async Task<IActionResult> AsignarSolicitudes([FromBody] AsignarSolicitudesDTO dto)
    {
        try
        {
            await _comiteService.AsignarSolicitudesAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
