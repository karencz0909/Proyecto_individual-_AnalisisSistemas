using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Security;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BecaNet.Api.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize(Roles = TiposUsuario.Estudiante)]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificaciones;
    public NotificacionesController(INotificacionService notificaciones) => _notificaciones = notificaciones;

    [HttpGet("mias")]
    public async Task<ActionResult<List<NotificacionSolicitudDTO>>> ObtenerMias()
    {
        var idEstudiante = User.ObtenerIdUsuario();
        if (idEstudiante is null) return Unauthorized();
        return Ok(await _notificaciones.ObtenerMiasAsync(idEstudiante.Value));
    }
}
