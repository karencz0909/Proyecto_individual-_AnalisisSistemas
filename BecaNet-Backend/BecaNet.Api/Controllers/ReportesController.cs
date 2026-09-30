using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using BecaNet.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    /// <summary>US-016: reporte de resultados de una convocatoria.</summary>
    [HttpGet("convocatoria/{idConvocatoria}")]
    public async Task<ActionResult<ReporteConvocatoriaDTO>> ReporteConvocatoria(int idConvocatoria)
    {
        try
        {
            return Ok(await _reporteService.ReporteConvocatoriaAsync(idConvocatoria));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-017: estadísticas generales del sistema.</summary>
    [HttpGet("estadisticas-generales")]
    public async Task<ActionResult<EstadisticasGeneralesDTO>> EstadisticasGenerales()
    {
        return Ok(await _reporteService.EstadisticasGeneralesAsync());
    }
}