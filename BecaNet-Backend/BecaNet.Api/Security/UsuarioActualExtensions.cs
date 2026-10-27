using System.Security.Claims;

namespace BecaNet.Api.Security;

public static class UsuarioActualExtensions
{
    /// <summary>Lee el id del usuario autenticado desde el token JWT (claim "sub").</summary>
    public static int? ObtenerIdUsuario(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? usuario.FindFirst("sub")?.Value;
        return int.TryParse(valor, out var id) ? id : null;
    }
}
