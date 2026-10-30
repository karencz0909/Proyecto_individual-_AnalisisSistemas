using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;
    private readonly ISolicitudService _solicitudService;

    public DocumentosController(IDocumentoService documentoService, ISolicitudService solicitudService)
    {
        _documentoService = documentoService;
        _solicitudService = solicitudService;
    }

    /// <summary>
    /// US-008: el estudiante adjunta un documento a SU solicitud.
    /// tipoDocumento: CERTIFICACION_ESTUDIOS (obligatorio para la beca) u OTRO.
    /// </summary>
    [HttpPost("solicitud/{idSolicitud}")]
    [Authorize(Roles = TiposUsuario.Estudiante)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoDTO>> Cargar(
        int idSolicitud, IFormFile archivo, [FromForm] string? tipoDocumento)
    {
        var idEstudiante = User.ObtenerIdUsuario();
        var solicitud = await _solicitudService.ObtenerPorIdAsync(idSolicitud);

        if (solicitud is null) return BadRequest(new { mensaje = "La solicitud no existe." });
        if (solicitud.IdEstudiante != idEstudiante) return Forbid();

        try
        {
            var documento = await _documentoService.CargarDocumentoAsync(idSolicitud, archivo, idEstudiante!.Value, tipoDocumento);
            return CreatedAtAction(nameof(ObtenerPorSolicitud), new { idSolicitud }, documento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("solicitud/{idSolicitud}")]
    [Authorize]
    public async Task<ActionResult<List<DocumentoDTO>>> ObtenerPorSolicitud(int idSolicitud)
    {
        return Ok(await _documentoService.ObtenerPorSolicitudAsync(idSolicitud));
    }
}
