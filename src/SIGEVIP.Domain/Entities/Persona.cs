using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Persona
    {
        public Persona(
            int idPersona,
            string nombre,
            string apellido,
            string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre de la persona es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                throw new ReglaNegocioException(
                    "El apellido de la persona es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ReglaNegocioException(
                    "El email de la persona es obligatorio.");
            }

            IdPersona = idPersona;
            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Email = email.Trim();
            Activo = true;
        }

        public int IdPersona { get; private set; }

        public string Nombre { get; private set; }

        public string Apellido { get; private set; }

        public string Email { get; private set; }

        public bool Activo { get; private set; }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }
    }
}
