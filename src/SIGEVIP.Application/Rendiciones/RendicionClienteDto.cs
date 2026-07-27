namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionClienteDto
    {
        public RendicionClienteDto(
            int idCliente,
            string razonSocial,
            string cuit,
            string localidad,
            string provincia,
            bool activo)
        {
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