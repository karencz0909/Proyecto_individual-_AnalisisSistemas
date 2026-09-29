using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _perfilService;

    public PerfilController(IPerfilService perfilService)
    {
        _perfilService = perfilService;
    }

    // tipoUsuario: ESTUDIANTE | COORDINADOR | EVALUADOR
    [HttpGet("{id}/{tipoUsuario}")]
    public async Task<ActionResult<PerfilDTO>> Obtener(int id, string tipoUsuario)
    {
        try
        {
            return Ok(await _perfilService.ObtenerAsync(id, tipoUsuario.ToUpperInvariant()));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/{tipoUsuario}")]
    public async Task<ActionResult<PerfilDTO>> Actualizar(int id, string tipoUsuario, [FromBody] ActualizarPerfilDTO dto)
    {
        try
        {
            return Ok(await _perfilService.ActualizarAsync(id, tipoUsuario.ToUpperInvariant(), dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}