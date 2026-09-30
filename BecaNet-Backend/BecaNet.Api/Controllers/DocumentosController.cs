using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using BecaNet.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;

    public DocumentosController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }

    /// <summary>US-008: cargar un documento a una solicitud.</summary>
    [HttpPost("solicitud/{idSolicitud}")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoDTO>> Cargar(int idSolicitud, IFormFile archivo)
    {
        try
        {
            var documento = await _documentoService.CargarDocumentoAsync(idSolicitud, archivo);
            return CreatedAtAction(nameof(ObtenerPorSolicitud), new { idSolicitud }, documento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("solicitud/{idSolicitud}")]
    public async Task<ActionResult<List<DocumentoDTO>>> ObtenerPorSolicitud(int idSolicitud)
    {
        return Ok(await _documentoService.ObtenerPorSolicitudAsync(idSolicitud));
    }
}