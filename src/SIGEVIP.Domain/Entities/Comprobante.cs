using System;
using System.Linq;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Comprobante
    {
        public Comprobante(
            int idComprobante,
            TipoComprobante tipo,
            string cuitProveedor,
            string razonSocialProveedor,
            SituacionFiscal situacionFiscal,
            string sucursal,
            string numero,
            decimal montoGravado,
            decimal montoImpuestos)
        {
            ValidarId(
                idComprobante);

            ValidarDatos(
                tipo,
                cuitProveedor,
                razonSocialProveedor,
                situacionFiscal,
                sucursal,
                numero,
                montoGravado,
                montoImpuestos);

            IdComprobante = idComprobante;

            AsignarDatos(
                tipo,
                cuitProveedor,
                razonSocialProveedor,
                situacionFiscal,
                sucursal,
                numero,
                montoGravado,
                montoImpuestos);
        }

        public int IdComprobante
        {
            get;
            private set;
        }

        public TipoComprobante Tipo
        {
            get;
            private set;
        }

        public string CuitProveedor
        {
            get;
            private set;
        }

        public string RazonSocialProveedor
        {
            get;
            private set;
        }

        public SituacionFiscal SituacionFiscal
        {
            get;
            private set;
        }

        public string Sucursal
        {
            get;
            private set;
        }

        public string Numero
        {
            get;
            private set;
        }

        public decimal MontoGravado
        {
            get;
            private set;
        }

        public decimal MontoImpuestos
        {
            get;
            private set;
        }

        public decimal Total
        {
            get
            {
                return MontoGravado +
                       MontoImpuestos;
            }
        }

        public void Actualizar(
            TipoComprobante tipo,
            string cuitProveedor,
            string razonSocialProveedor,
            SituacionFiscal situacionFiscal,
            string sucursal,
            string numero,
            decimal montoGravado,
            decimal montoImpuestos)
        {
            ValidarDatos(
                tipo,
                cuitProveedor,
                razonSocialProveedor,
                situacionFiscal,
                sucursal,
                numero,
                montoGravado,
                montoImpuestos);

            AsignarDatos(
                tipo,
                cuitProveedor,
                razonSocialProveedor,
                situacionFiscal,
                sucursal,
                numero,
                montoGravado,
                montoImpuestos);
        }

        private void AsignarDatos(
            TipoComprobante tipo,
            string cuitProveedor,
            string razonSocialProveedor,
            SituacionFiscal situacionFiscal,
            string sucursal,
            string numero,
            decimal montoGravado,
            decimal montoImpuestos)
        {
            Tipo = tipo;
            CuitProveedor =
                cuitProveedor.Trim();
            RazonSocialProveedor =
                razonSocialProveedor.Trim();
            SituacionFiscal =
                situacionFiscal;
            Sucursal =
                sucursal.Trim();
            Numero =
                numero.Trim();
            MontoGravado =
                montoGravado;
            MontoImpuestos =
                montoImpuestos;
        }

        private static void ValidarId(
            int idComprobante)
        {
            if (idComprobante < 0)
            {
                throw new ReglaNegocioException(
                    "El identificador del comprobante no puede ser negativo.");
            }
        }

        private static void ValidarDatos(
            TipoComprobante tipo,
            string cuitProveedor,
            string razonSocialProveedor,
            SituacionFiscal situacionFiscal,
            string sucursal,
            string numero,
            decimal montoGravado,
            decimal montoImpuestos)
        {
            if (!Enum.IsDefined(
                typeof(TipoComprobante),
                tipo))
            {
                throw new ReglaNegocioException(
                    "El tipo de comprobante indicado no es válido.");
            }

            if (string.IsNullOrWhiteSpace(
                cuitProveedor))
            {
                throw new ReglaNegocioException(
                    "El CUIT del proveedor es obligatorio cuando existe comprobante.");
            }

            if (string.IsNullOrWhiteSpace(
                razonSocialProveedor))
            {
                throw new ReglaNegocioException(
                    "La razón social del proveedor es obligatoria cuando existe comprobante.");
            }

            if (!Enum.IsDefined(
                typeof(SituacionFiscal),
                situacionFiscal))
            {
                throw new ReglaNegocioException(
                    "La situación fiscal indicada no es válida.");
            }

            ValidarCodigoNumerico(
                sucursal,
                4,
                "La sucursal del comprobante debe contener exactamente cuatro dígitos.");

            ValidarCodigoNumerico(
                numero,
                8,
                "El número del comprobante debe contener exactamente ocho dígitos.");

            if (montoGravado < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto gravado del comprobante no puede ser negativo.");
            }

            if (montoImpuestos < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto de impuestos del comprobante no puede ser negativo.");
            }
        }

        private static void ValidarCodigoNumerico(
            string valor,
            int longitud,
            string mensaje)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                throw new ReglaNegocioException(
                    mensaje);
            }

            string normalizado =
                valor.Trim();

            if (normalizado.Length != longitud ||
                !normalizado.All(
                    char.IsDigit))
            {
                throw new ReglaNegocioException(
                    mensaje);
            }
        }
    }
}
