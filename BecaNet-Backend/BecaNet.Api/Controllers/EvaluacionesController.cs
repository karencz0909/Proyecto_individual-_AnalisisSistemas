using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluacionesController : ControllerBase
{
    private readonly IEvaluacionService _evaluacionService;

    public EvaluacionesController(IEvaluacionService evaluacionService)
    {
        _evaluacionService = evaluacionService;
    }

    /// <summary>US-012: Como miembro de un comité, quiero registrar mi evaluación de una solicitud.</summary>
    [HttpPost]
    public async Task<ActionResult<EvaluacionDTO>> Registrar([FromBody] RegistrarEvaluacionDTO dto)
    {
        try
        {
            var creada = await _evaluacionService.RegistrarEvaluacionAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorSolicitud), new { idSolicitud = creada.IdSolicitud }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("solicitud/{idSolicitud}")]
    public async Task<ActionResult<List<EvaluacionDTO>>> ObtenerPorSolicitud(int idSolicitud)
    {
        var evaluaciones = await _evaluacionService.ObtenerPorSolicitudAsync(idSolicitud);
        return Ok(evaluaciones);
    }
}
