using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Usuario
    {
        private readonly byte[] _passwordHash;
        private readonly byte[] _passwordSalt;

        public Usuario(
            int idUsuario,
            int idPersona,
            string nombreUsuario,
            byte[] passwordHash,
            byte[] passwordSalt,
            int iteracionesPassword)
        {
            if (idPersona <= 0)
            {
                throw new ReglaNegocioException(
                    "El usuario debe estar asociado a una persona válida.");
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ReglaNegocioException(
                    "El nombre de usuario es obligatorio.");
            }

            if (passwordHash == null || passwordHash.Length == 0)
            {
                throw new ReglaNegocioException(
                    "El hash de contraseña es obligatorio.");
            }

            if (passwordSalt == null || passwordSalt.Length == 0)
            {
                throw new ReglaNegocioException(
                    "El salt de contraseña es obligatorio.");
            }

            if (iteracionesPassword <= 0)
            {
                throw new ReglaNegocioException(
                    "La cantidad de iteraciones de contraseña debe ser mayor que cero.");
            }

            IdUsuario = idUsuario;
            IdPersona = idPersona;
            NombreUsuario = NormalizarNombreUsuario(nombreUsuario);
            IteracionesPassword = iteracionesPassword;
            Activo = true;

            _passwordHash = CopiarArreglo(passwordHash);
            _passwordSalt = CopiarArreglo(passwordSalt);
        }

        public int IdUsuario { get; private set; }

        public int IdPersona { get; private set; }

        public string NombreUsuario { get; private set; }

        public int IteracionesPassword { get; private set; }

        public bool Activo { get; private set; }

        public byte[] PasswordHash
        {
            get { return CopiarArreglo(_passwordHash); }
        }

        public byte[] PasswordSalt
        {
            get { return CopiarArreglo(_passwordSalt); }
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        internal static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return string.IsNullOrWhiteSpace(nombreUsuario)
                ? string.Empty
                : nombreUsuario.Trim().ToLowerInvariant();
        }

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
