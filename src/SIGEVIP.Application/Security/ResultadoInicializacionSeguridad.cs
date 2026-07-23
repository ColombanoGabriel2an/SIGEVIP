namespace SIGEVIP.Application.Security
{
    public sealed class ResultadoInicializacionSeguridad
    {
        private ResultadoInicializacionSeguridad(
            bool exitoso,
            bool usuarioExistente,
            string nombreUsuario,
            string mensaje)
        {
            Exitoso = exitoso;
            UsuarioExistente = usuarioExistente;
            NombreUsuario = nombreUsuario;
            Mensaje = mensaje;
        }

        public bool Exitoso { get; private set; }

        public bool UsuarioExistente { get; private set; }

        public string NombreUsuario { get; private set; }

        public string Mensaje { get; private set; }

        public static ResultadoInicializacionSeguridad CrearExitoso(
            string nombreUsuario)
        {
            return new ResultadoInicializacionSeguridad(
                true,
                false,
                nombreUsuario,
                "El administrador inicial fue creado correctamente.");
        }

        public static ResultadoInicializacionSeguridad CrearUsuarioExistente(
            string nombreUsuario)
        {
            return new ResultadoInicializacionSeguridad(
                false,
                true,
                nombreUsuario,
                "El nombre de usuario ya existe.");
        }

        public static ResultadoInicializacionSeguridad CrearFallido(
            string mensaje)
        {
            return new ResultadoInicializacionSeguridad(
                false,
                false,
                string.Empty,
                mensaje);
        }
    }
}
