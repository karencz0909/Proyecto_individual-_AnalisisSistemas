using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = TiposUsuario.Coordinador)]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoria;
    public AuditoriaController(IAuditoriaService auditoria) => _auditoria = auditoria;

    [HttpGet("solicitud/{idSolicitud:int}")]
    public async Task<ActionResult<List<RegistroAuditoriaDTO>>> ObtenerPorSolicitud(int idSolicitud)
    {
        if (idSolicitud <= 0) return BadRequest();
        return Ok(await _auditoria.ObtenerPorSolicitudAsync(idSolicitud));
    }
}
