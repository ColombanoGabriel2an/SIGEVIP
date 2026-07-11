using System;
using System.Security.Cryptography;
using SIGEVIP.Application.Security;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        public const int IteracionesPredeterminadas = 100000;
        public const int LongitudSaltBytes = 32;
        public const int LongitudHashBytes = 32;

        public PasswordHashResult CrearHash(string password)
        {
            ValidarPassword(password);

            byte[] salt = GenerarSalt();

            byte[] hash = DerivarHash(
                password,
                salt,
                IteracionesPredeterminadas,
                LongitudHashBytes);

            return new PasswordHashResult(
                hash,
                salt,
                IteracionesPredeterminadas);
        }

        public bool Verificar(
            string password,
            byte[] hashEsperado,
            byte[] salt,
            int iteraciones)
        {
            ValidarPassword(password);
            ValidarHashEsperado(hashEsperado);
            ValidarSalt(salt);
            ValidarIteraciones(iteraciones);

            byte[] hashCalculado = DerivarHash(
                password,
                salt,
                iteraciones,
                hashEsperado.Length);

            return CompararTiempoConstante(
                hashEsperado,
                hashCalculado);
        }

        private static byte[] GenerarSalt()
        {
            byte[] salt =
                new byte[LongitudSaltBytes];

            using (
                RandomNumberGenerator generador =
                    RandomNumberGenerator.Create())
            {
                generador.GetBytes(salt);
            }

            return salt;
        }

        private static byte[] DerivarHash(
            string password,
            byte[] salt,
            int iteraciones,
            int longitudHash)
        {
            using (
                Rfc2898DeriveBytes derivador =
                    new Rfc2898DeriveBytes(
                        password,
                        salt,
                        iteraciones,
                        HashAlgorithmName.SHA256))
            {
                return derivador.GetBytes(longitudHash);
            }
        }

        private static bool CompararTiempoConstante(
            byte[] esperado,
            byte[] calculado)
        {
            if (esperado == null || calculado == null)
            {
                return false;
            }

            int diferencia =
                esperado.Length ^ calculado.Length;

            int longitud =
                Math.Min(
                    esperado.Length,
                    calculado.Length);

            for (int indice = 0;
                 indice < longitud;
                 indice++)
            {
                diferencia |=
                    esperado[indice] ^
                    calculado[indice];
            }

            return diferencia == 0;
        }

        private static void ValidarPassword(
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "La contraseña es obligatoria.",
                    nameof(password));
            }
        }

        private static void ValidarHashEsperado(
            byte[] hashEsperado)
        {
            if (hashEsperado == null ||
                hashEsperado.Length == 0)
            {
                throw new ArgumentException(
                    "El hash esperado es obligatorio.",
                    nameof(hashEsperado));
            }
        }

        private static void ValidarSalt(
            byte[] salt)
        {
            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException(
                    "El salt es obligatorio.",
                    nameof(salt));
            }
        }

        private static void ValidarIteraciones(
            int iteraciones)
        {
            if (iteraciones <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iteraciones),
                    "Las iteraciones deben ser mayores que cero.");
            }
        }
    }
}
