using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public sealed class ResultadoAutenticacion
    {
        private ResultadoAutenticacion(
            bool exitoso,
            Usuario usuario,
            string mensaje)
        {
            Exitoso = exitoso;
            Usuario = usuario;
            Mensaje = mensaje;
        }

        public bool Exitoso { get; private set; }

        public Usuario Usuario { get; private set; }

        public string Mensaje { get; private set; }

        public static ResultadoAutenticacion CrearExitoso(
            Usuario usuario)
        {
            return new ResultadoAutenticacion(
                true,
                usuario,
                "Autenticación correcta.");
        }

        public static ResultadoAutenticacion CrearFallido(
            string mensaje)
        {
            return new ResultadoAutenticacion(
                false,
                null,
                mensaje);
        }
    }
}
