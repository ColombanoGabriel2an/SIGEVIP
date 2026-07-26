using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Cliente
    {
        public Cliente(
            int idCliente,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia)
        {
            IdCliente = idCliente;
            Activo = true;

            ActualizarDatos(
                razonSocial,
                cuit,
                email,
                telefono,
                localidad,
                provincia);
        }

        public int IdCliente { get; private set; }

        public string RazonSocial { get; private set; }

        public string Cuit { get; private set; }

        public string Email { get; private set; }

        public string Telefono { get; private set; }

        public string Localidad { get; private set; }

        public string Provincia { get; private set; }

        public bool Activo { get; private set; }

        public static Cliente Reconstruir(
            int idCliente,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia,
            bool activo)
        {
            if (idCliente <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido del cliente debe ser válido.");
            }

            Cliente cliente =
                new Cliente(
                    idCliente,
                    razonSocial,
                    cuit,
                    email,
                    telefono,
                    localidad,
                    provincia);

            if (!activo)
            {
                cliente.Desactivar();
            }

            return cliente;
        }

        public void ActualizarDatos(
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
            {
                throw new ReglaNegocioException(
                    "La razón social del cliente es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(cuit))
            {
                throw new ReglaNegocioException(
                    "El CUIT del cliente es obligatorio.");
            }

            string cuitNormalizado =
                NormalizarCuit(cuit);

            if (string.IsNullOrWhiteSpace(cuitNormalizado))
            {
                throw new ReglaNegocioException(
                    "El CUIT del cliente es obligatorio.");
            }

            RazonSocial = razonSocial.Trim();
            Cuit = cuitNormalizado;
            Email = NormalizarTextoOpcional(email);
            Telefono = NormalizarTextoOpcional(telefono);
            Localidad = NormalizarTextoOpcional(localidad);
            Provincia = NormalizarTextoOpcional(provincia);
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        internal static string NormalizarCuit(
            string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit))
            {
                return string.Empty;
            }

            return cuit
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Trim();
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
