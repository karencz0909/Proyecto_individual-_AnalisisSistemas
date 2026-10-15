using Microsoft.AspNetCore.Mvc;
using BecaNet.Api.DTOs;
using BecaNet.Api.Services;

namespace BecaNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registro")]
        [HttpPost("register")]
        public async Task<ActionResult<AuthRespuestaDTO>> Registrar([FromBody] RegistroEstudianteDTO dto)
        {
            try
            {
                var resultado = await _authService.RegistrarEstudianteAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthRespuestaDTO>> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var resultado = await _authService.LoginAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }
    }
}