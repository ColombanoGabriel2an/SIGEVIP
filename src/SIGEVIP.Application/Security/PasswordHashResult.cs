using System;

namespace SIGEVIP.Application.Security
{
    public sealed class PasswordHashResult
    {
        private readonly byte[] _hash;
        private readonly byte[] _salt;

        public PasswordHashResult(
            byte[] hash,
            byte[] salt,
            int iteraciones)
        {
            if (hash == null || hash.Length == 0)
            {
                throw new ArgumentException(
                    "El hash no puede estar vacío.",
                    nameof(hash));
            }

            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException(
                    "El salt no puede estar vacío.",
                    nameof(salt));
            }

            if (iteraciones <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iteraciones),
                    "Las iteraciones deben ser mayores que cero.");
            }

            _hash = CopiarArreglo(hash);
            _salt = CopiarArreglo(salt);
            Iteraciones = iteraciones;
        }

        public byte[] Hash
        {
            get { return CopiarArreglo(_hash); }
        }

        public byte[] Salt
        {
            get { return CopiarArreglo(_salt); }
        }

        public int Iteraciones { get; private set; }

        private static byte[] CopiarArreglo(byte[] origen)
        {
            byte[] copia = new byte[origen.Length];

            Array.Copy(
                origen,
                copia,
                origen.Length);

            return copia;
        }
    }
}
