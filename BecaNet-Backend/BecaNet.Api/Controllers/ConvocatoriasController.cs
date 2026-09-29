using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConvocatoriasController : ControllerBase
{
    private readonly IConvocatoriaService _convocatoriaService;

    public ConvocatoriasController(IConvocatoriaService convocatoriaService)
    {
        _convocatoriaService = convocatoriaService;
    }

    /// <summary>US-004: crear convocatoria.</summary>
    [HttpPost]
    public async Task<ActionResult<ConvocatoriaDTO>> Crear([FromBody] CrearConvocatoriaDTO dto)
    {
        try
        {
            var creada = await _convocatoriaService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerTodas), new { }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-005: publicar convocatoria.</summary>
    [HttpPut("{id}/publicar")]
    public async Task<ActionResult<ConvocatoriaDTO>> Publicar(int id)
    {
        try
        {
            return Ok(await _convocatoriaService.PublicarAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-005: cerrar convocatoria.</summary>
    [HttpPut("{id}/cerrar")]
    public async Task<ActionResult<ConvocatoriaDTO>> Cerrar(int id)
    {
        try
        {
            return Ok(await _convocatoriaService.CerrarAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-006: listado de convocatorias abiertas (vista del estudiante).</summary>
    [HttpGet("abiertas")]
    public async Task<ActionResult<List<ConvocatoriaDTO>>> ObtenerAbiertas()
    {
        return Ok(await _convocatoriaService.ObtenerAbiertasAsync());
    }

    /// <summary>Listado completo (vista del coordinador).</summary>
    [HttpGet]
    public async Task<ActionResult<List<ConvocatoriaDTO>>> ObtenerTodas()
    {
        return Ok(await _convocatoriaService.ObtenerTodasAsync());
    }
}