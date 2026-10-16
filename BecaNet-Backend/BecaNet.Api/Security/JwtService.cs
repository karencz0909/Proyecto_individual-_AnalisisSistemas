using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BecaNet.Api.Security;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerarToken(int idUsuario, string correo, string tipoUsuario)
    {
        // Mismas claves y valores por defecto que usa Program.cs para VALIDAR el token
        var seccion = _config.GetSection("Jwt");
        var secreto = seccion["Secret"] ?? seccion["Key"] ?? "TuClaveSecretaSuperSeguraBecaNet2026_JWT_Token_Key_Long_String";
        var emisor = seccion["Issuer"] ?? "BecaNet.Api";
        var audiencia = seccion["Audience"] ?? "BecaNet.Client";
        var minutos = int.TryParse(seccion["MinutosExpiracion"], out var m) ? m : 120;

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, idUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, correo),
            new Claim(ClaimTypes.Role, tipoUsuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutos),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}