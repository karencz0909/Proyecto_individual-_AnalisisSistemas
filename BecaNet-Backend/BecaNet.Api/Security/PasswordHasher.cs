using System.Security.Cryptography;

namespace BecaNet.Api.Security;

/// <summary>
/// Genera y verifica hashes de contraseña usando PBKDF2 (built-in en .NET,
/// no requiere paquetes NuGet adicionales). Nunca se guarda la contraseña
/// en texto plano, tal como exige el criterio de aceptación de US-001.
/// </summary>
public static class PasswordHasher
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int Iteraciones = 100_000;

    public static string Hashear(string contrasenaPlano)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasenaPlano, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string contrasenaPlano, string hashGuardado)
    {
        var partes = hashGuardado.Split('.');
        if (partes.Length != 2) return false;

        var sal = Convert.FromBase64String(partes[0]);
        var hashEsperado = Convert.FromBase64String(partes[1]);
        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(contrasenaPlano, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}