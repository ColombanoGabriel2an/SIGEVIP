namespace SIGEVIP.Application.Visitas
{
    public sealed class ClienteSeleccionVisitaDto
    {
        public ClienteSeleccionVisitaDto(
            int idCliente,
            string razonSocial,
            string cuit,
            bool activo)
        {
            IdCliente = idCliente;
            RazonSocial =
                razonSocial ?? string.Empty;
            Cuit =
                cuit ?? string.Empty;
            Activo = activo;
        }

        public int IdCliente { get; private set; }

        public string RazonSocial { get; private set; }

        public string Cuit { get; private set; }

        public bool Activo { get; private set; }

        public string Descripcion
        {
            get
            {
                return RazonSocial +
                       " - " +
                       Cuit;
            }
        }
    }
}