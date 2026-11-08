// PasswordHasher guarda las contraseñas de forma segura: nunca se guarda la contraseña,
// solo un hash que no se puede revertir. Usa PBKDF2, que ya viene con .NET (sin paquetes extra).
using System.Security.Cryptography;

namespace Users.API.Services
{
    /// <summary>Genera y verifica hashes de contraseña con PBKDF2 (SHA-256).</summary>
    public static class PasswordHasher
    {
        private const int TamanoSal = 16;       // bytes de sal aleatoria
        private const int TamanoHash = 32;      // bytes del hash resultante
        private const int Iteraciones = 100_000; // cuántas veces se repite el cálculo (lo hace lento a propósito)

        /// <summary>
        /// Devuelve el hash en el formato "iteraciones.sal.hash" (sal y hash en Base64).
        /// Dos llamadas con la misma contraseña dan resultados distintos, porque la sal es aleatoria.
        /// </summary>
        public static string Hash(string password)
        {
            var sal = RandomNumberGenerator.GetBytes(TamanoSal);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

            return $"{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>Indica si la contraseña coincide con un hash guardado. Si el formato del hash es inválido, devuelve false.</summary>
        public static bool Verificar(string password, string hashGuardado)
        {
            var partes = hashGuardado.Split('.');
            if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
                return false;

            try
            {
                var sal = Convert.FromBase64String(partes[1]);
                var hashEsperado = Convert.FromBase64String(partes[2]);

                // se recalcula con la misma sal y las mismas iteraciones que se usaron al guardar
                var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                    password, sal, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

                // comparación en tiempo constante: no filtra información por cuánto tarda en fallar
                return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}