namespace SIGEVIP.Application.Clientes
{
    public sealed class ClienteListadoDto
    {
        public ClienteListadoDto(
            int idCliente,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia,
            bool activo)
        {
            IdCliente = idCliente;
            RazonSocial = razonSocial;
            Cuit = cuit;
            Email = email;
            Telefono = telefono;
            Localidad = localidad;
            Provincia = provincia;
            Activo = activo;
        }

        public int IdCliente { get; private set; }

        public string RazonSocial { get; private set; }

        public string Cuit { get; private set; }

        public string Email { get; private set; }

        public string Telefono { get; private set; }

        public string Localidad { get; private set; }

        public string Provincia { get; private set; }

        public bool Activo { get; private set; }

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
