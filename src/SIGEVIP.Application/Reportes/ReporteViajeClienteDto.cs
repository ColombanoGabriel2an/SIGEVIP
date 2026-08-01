namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeClienteDto
    {
        public ReporteViajeClienteDto(
            int idVisita,
            int idCliente,
            string razonSocial,
            string cuit,
            string localidad,
            string provincia,
            bool activo)
        {
            IdVisita = idVisita;
            IdCliente = idCliente;
            RazonSocial =
                razonSocial ?? string.Empty;
            Cuit =
                cuit ?? string.Empty;
            Localidad =
                localidad ?? string.Empty;
            Provincia =
                provincia ?? string.Empty;
            Activo = activo;
        }

        public int IdVisita { get; private set; }

        public int IdCliente { get; private set; }

        public string RazonSocial
        {
            get;
            private set;
        }

        public string Cuit { get; private set; }

        public string Localidad
        {
            get;
            private set;
        }

        public string Provincia
        {
            get;
            private set;
        }

        public bool Activo { get; private set; }
    }
}
