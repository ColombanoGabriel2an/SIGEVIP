namespace SIGEVIP.Application.Permisos
{
    public sealed class PermisoFiltro
    {
        public PermisoFiltro(
            string textoGeneral,
            bool? activo)
        {
            TextoGeneral =
                NormalizarTexto(
                    textoGeneral);

            Activo =
                activo;
        }

        public string TextoGeneral
        {
            get;
            private set;
        }

        public bool? Activo
        {
            get;
            private set;
        }

        public static PermisoFiltro
            CrearSinFiltros()
        {
            return new PermisoFiltro(
                string.Empty,
                null);
        }

        private static string NormalizarTexto(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}