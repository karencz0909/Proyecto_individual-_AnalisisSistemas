using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionService _asignacionService;

    public AsignacionesController(IAsignacionService asignacionService)
    {
        _asignacionService = asignacionService;
    }

    // ------------------------- EVALUADOR -------------------------

    /// <summary>Las solicitudes asignadas al evaluador autenticado.</summary>
    [HttpGet("mias")]
    [Authorize(Roles = TiposUsuario.Evaluador)]
    public async Task<ActionResult<List<AsignacionEvaluadorDTO>>> ObtenerMias()
    {
        var idEvaluador = User.ObtenerIdUsuario();
        if (idEvaluador is null) return Unauthorized();

        return Ok(await _asignacionService.ObtenerMisAsignacionesAsync(idEvaluador.Value));
    }

    /// <summary>El evaluador califica su solicitud (puntaje + observaciones) y la pasa a REVISADO.</summary>
    [HttpPut("{idSolicitud}/revisar")]
    [Authorize(Roles = TiposUsuario.Evaluador)]
    public async Task<ActionResult<AsignacionEvaluadorDTO>> Revisar(int idSolicitud, [FromBody] RevisarSolicitudDTO dto)
    {
        var idEvaluador = User.ObtenerIdUsuario();
        if (idEvaluador is null) return Unauthorized();

        try
        {
            return Ok(await _asignacionService.RevisarAsync(idEvaluador.Value, idSolicitud, dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>El evaluador devuelve su solicitud a EN_REVISION (deshacer).</summary>
    [HttpPut("{idSolicitud}/reabrir")]
    [Authorize(Roles = TiposUsuario.Evaluador)]
    public async Task<ActionResult<AsignacionEvaluadorDTO>> Reabrir(int idSolicitud)
    {
        var idEvaluador = User.ObtenerIdUsuario();
        if (idEvaluador is null) return Unauthorized();

        try
        {
            return Ok(await _asignacionService.ReabrirAsync(idEvaluador.Value, idSolicitud));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ------------------------- COORDINADOR -------------------------

    /// <summary>Todas las solicitudes con estudiante, evaluador (correo) y estado.</summary>
    [HttpGet("solicitudes")]
    [Authorize(Roles = TiposUsuario.Coordinador)]
    public async Task<ActionResult<List<SolicitudCoordinadorDTO>>> ObtenerSolicitudes()
    {
        return Ok(await _asignacionService.ObtenerSolicitudesCoordinadorAsync());
    }

    /// <summary>Evaluadores con su carga actual de solicitudes.</summary>
    [HttpGet("evaluadores")]
    [Authorize(Roles = TiposUsuario.Coordinador)]
    public async Task<ActionResult<List<EvaluadorOpcionDTO>>> ObtenerEvaluadores()
    {
        return Ok(await _asignacionService.ObtenerEvaluadoresAsync());
    }

    /// <summary>Reparte automáticamente las solicitudes pendientes entre los evaluadores.</summary>
    [HttpPost("distribuir")]
    [Authorize(Roles = TiposUsuario.Coordinador)]
    public async Task<ActionResult<ResultadoDistribucionDTO>> Distribuir([FromBody] DistribuirDTO dto)
    {
        try
        {
            return Ok(await _asignacionService.DistribuirAsync(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Asigna o reasigna manualmente una solicitud a un evaluador.</summary>
    [HttpPut("{idSolicitud}/asignar")]
    [Authorize(Roles = TiposUsuario.Coordinador)]
    public async Task<ActionResult<SolicitudCoordinadorDTO>> Asignar(int idSolicitud, [FromBody] AsignarManualDTO dto)
    {
        try
        {
            return Ok(await _asignacionService.AsignarManualAsync(idSolicitud, dto.IdEvaluador));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
