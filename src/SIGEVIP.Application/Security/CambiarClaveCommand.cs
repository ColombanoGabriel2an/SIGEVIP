namespace SIGEVIP.Application.Security
{
    public sealed class CambiarClaveCommand
    {
        public CambiarClaveCommand(
            string claveActual,
            string claveNueva,
            string confirmacionClaveNueva)
        {
            ClaveActual =
                claveActual;

            ClaveNueva =
                claveNueva;

            ConfirmacionClaveNueva =
                confirmacionClaveNueva;
        }

        public string ClaveActual
        {
            get;
            private set;
        }

        public string ClaveNueva
        {
            get;
            private set;
        }

        public string ConfirmacionClaveNueva
        {
            get;
            private set;
        }
    }
}
