using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Services;
using BecaNet.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = TiposUsuario.Coordinador)]
public class UsuariosController : ControllerBase
{
    private readonly IGestionUsuariosService _usuarios;

    public UsuariosController(IGestionUsuariosService usuarios) => _usuarios = usuarios;

    [HttpGet]
    public async Task<ActionResult<List<UsuarioGestionDTO>>> Listar()
        => Ok(await _usuarios.ListarAsync());

    [HttpPost]
    public async Task<ActionResult<UsuarioGestionDTO>> Crear([FromBody] CrearUsuarioGestionDTO dto)
    {
        var idActor = User.ObtenerIdUsuario();
        if (idActor is null) return Unauthorized();
        try { return Ok(await _usuarios.CrearAsync(dto, idActor.Value)); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPut("{id:int}/{tipoUsuario}")]
    public async Task<ActionResult<UsuarioGestionDTO>> Actualizar(int id, string tipoUsuario, [FromBody] ActualizarUsuarioGestionDTO dto)
    {
        var idActor = User.ObtenerIdUsuario();
        if (idActor is null) return Unauthorized();
        try { return Ok(await _usuarios.ActualizarAsync(id, tipoUsuario, dto, idActor.Value)); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }
}
