using BecaNet.Api.DTOs;
using BecaNet.Api.Services;
using BecaNet.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>US-001: Registro de un nuevo estudiante.</summary>
    [HttpPost("registro")]
    public async Task<ActionResult<AuthRespuestaDTO>> Registro([FromBody] RegistroEstudianteDTO dto)
    {
        try
        {
            var respuesta = await _authService.RegistrarEstudianteAsync(dto);
            return Ok(respuesta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>US-002: Inicio de sesión para cualquier tipo de usuario.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthRespuestaDTO>> Login([FromBody] LoginDTO dto)
    {
        try
        {
            var respuesta = await _authService.LoginAsync(dto);
            return Ok(respuesta);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }
}