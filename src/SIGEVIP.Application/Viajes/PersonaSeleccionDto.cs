namespace SIGEVIP.Application.Viajes
{
    public sealed class PersonaSeleccionDto
    {
        public PersonaSeleccionDto(
            int idPersona,
            string nombreCompleto,
            bool activo)
            : this(
                idPersona,
                nombreCompleto,
                string.Empty,
                activo)
        {
        }

        public PersonaSeleccionDto(
            int idPersona,
            string nombreCompleto,
            string email,
            bool activo)
        {
            IdPersona = idPersona;
            NombreCompleto =
                nombreCompleto ?? string.Empty;
            Email =
                email ?? string.Empty;
            Activo = activo;
        }

        public int IdPersona { get; private set; }

        public string NombreCompleto { get; private set; }

        public string Email { get; private set; }

        public bool Activo { get; private set; }
    }
}
