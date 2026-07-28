namespace SIGEVIP.Application.Usuarios
{
    public sealed class PersonaSeleccionUsuarioDto
    {
        public PersonaSeleccionUsuarioDto(
            int idPersona,
            string nombreCompleto,
            string email,
            bool activo)
        {
            IdPersona = idPersona;
            NombreCompleto = nombreCompleto;
            Email = email;
            Activo = activo;
        }

        public int IdPersona
        {
            get;
            private set;
        }

        public string NombreCompleto
        {
            get;
            private set;
        }

        public string Email
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get
            {
                return string.IsNullOrWhiteSpace(Email)
                    ? NombreCompleto
                    : NombreCompleto + " — " + Email;
            }
        }
    }
}
