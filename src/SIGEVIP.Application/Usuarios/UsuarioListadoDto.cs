namespace SIGEVIP.Application.Usuarios
{
    public sealed class UsuarioListadoDto
    {
        public UsuarioListadoDto(
            int idUsuario,
            string nombreUsuario,
            string nombreCompleto,
            string email,
            string gruposResumen,
            bool activo)
        {
            IdUsuario = idUsuario;
            NombreUsuario = nombreUsuario;
            NombreCompleto = nombreCompleto;
            Email = email;
            GruposResumen = gruposResumen;
            Activo = activo;
        }

        public int IdUsuario
        {
            get;
            private set;
        }

        public string NombreUsuario
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

        public string GruposResumen
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public string Estado
        {
            get
            {
                return Activo
                    ? "Activo"
                    : "Inactivo";
            }
        }
    }
}
