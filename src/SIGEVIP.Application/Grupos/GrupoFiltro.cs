namespace SIGEVIP.Application.Grupos
{
    public sealed class GrupoFiltro
    {
        public GrupoFiltro(
            string textoGeneral,
            bool? activo)
        {
            TextoGeneral =
                NormalizarTextoOpcional(
                    textoGeneral);

            Activo = activo;
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

        public static GrupoFiltro CrearSinFiltros()
        {
            return new GrupoFiltro(
                string.Empty,
                null);
        }

        private static string NormalizarTextoOpcional(
            string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}
