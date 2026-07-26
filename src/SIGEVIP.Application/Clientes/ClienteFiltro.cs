namespace SIGEVIP.Application.Clientes
{
    public sealed class ClienteFiltro
    {
        public ClienteFiltro(
            string textoGeneral,
            string cuit,
            string localidad,
            string provincia,
            bool? activo)
        {
            TextoGeneral =
                NormalizarTextoOpcional(
                    textoGeneral);

            Cuit =
                NormalizarTextoOpcional(
                    cuit);

            Localidad =
                NormalizarTextoOpcional(
                    localidad);

            Provincia =
                NormalizarTextoOpcional(
                    provincia);

            Activo = activo;
        }

        public string TextoGeneral { get; private set; }

        public string Cuit { get; private set; }

        public string Localidad { get; private set; }

        public string Provincia { get; private set; }

        public bool? Activo { get; private set; }

        public static ClienteFiltro CrearSinFiltros()
        {
            return new ClienteFiltro(
                string.Empty,
                string.Empty,
                string.Empty,
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
