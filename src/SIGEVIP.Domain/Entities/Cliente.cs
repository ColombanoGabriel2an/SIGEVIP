using System;
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

            string cuitNormalizado = NormalizarCuit(cuit);

            if (string.IsNullOrWhiteSpace(cuitNormalizado))
            {
                throw new ReglaNegocioException(
                    "El CUIT del cliente es obligatorio.");
            }

            IdCliente = idCliente;
            RazonSocial = razonSocial.Trim();
            Cuit = cuitNormalizado;
            Email = NormalizarTextoOpcional(email);
            Telefono = NormalizarTextoOpcional(telefono);
            Localidad = NormalizarTextoOpcional(localidad);
            Provincia = NormalizarTextoOpcional(provincia);
            Activo = true;
        }

        public int IdCliente { get; private set; }

        public string RazonSocial { get; private set; }

        public string Cuit { get; private set; }

        public string Email { get; private set; }

        public string Telefono { get; private set; }

        public string Localidad { get; private set; }

        public string Provincia { get; private set; }

        public bool Activo { get; private set; }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        internal static string NormalizarCuit(string cuit)
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

        private static string NormalizarTextoOpcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}
