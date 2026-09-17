using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
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

    /// <summary>US-008: Como estudiante, quiero cargar los documentos requeridos a mi solicitud.</summary>
    [HttpPost("solicitud/{idSolicitud}")]
    [RequestSizeLimit(6 * 1024 * 1024)] // margen sobre el límite de negocio de 5 MB
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
        var documentos = await _documentoService.ObtenerPorSolicitudAsync(idSolicitud);
        return Ok(documentos);
    }
}
