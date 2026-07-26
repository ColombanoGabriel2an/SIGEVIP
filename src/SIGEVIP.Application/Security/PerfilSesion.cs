using System;

namespace SIGEVIP.Application.Security
{
    public sealed class PerfilSesion
    {
        public PerfilSesion(
            int idPersona,
            string nombre,
            string apellido,
            string email)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona),
                    "El identificador de persona debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.",
                    nameof(nombre));
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                throw new ArgumentException(
                    "El apellido es obligatorio.",
                    nameof(apellido));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "El email es obligatorio.",
                    nameof(email));
            }

            IdPersona = idPersona;
            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Email = email.Trim();
        }

        public int IdPersona { get; private set; }

        public string Nombre { get; private set; }

        public string Apellido { get; private set; }

        public string Email { get; private set; }

        public string NombreCompleto
        {
            get
            {
                return Nombre + " " + Apellido;
            }
        }
    }
}
