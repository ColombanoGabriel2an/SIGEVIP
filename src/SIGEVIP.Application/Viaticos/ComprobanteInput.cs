using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viaticos
{
    public sealed class ComprobanteInput
    {
        public ComprobanteInput(
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
            if (idComprobante < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idComprobante),
                    "El identificador del comprobante no puede ser negativo.");
            }

            if (!Enum.IsDefined(
                typeof(TipoComprobante),
                tipo))
            {
                throw new ReglaNegocioException(
                    "El tipo de comprobante indicado no es válido.");
            }

            if (!Enum.IsDefined(
                typeof(SituacionFiscal),
                situacionFiscal))
            {
                throw new ReglaNegocioException(
                    "La situación fiscal indicada no es válida.");
            }

            IdComprobante =
                idComprobante;

            Tipo =
                tipo;

            CuitProveedor =
                cuitProveedor
                ?? string.Empty;

            RazonSocialProveedor =
                razonSocialProveedor
                ?? string.Empty;

            SituacionFiscal =
                situacionFiscal;

            Sucursal =
                sucursal
                ?? string.Empty;

            Numero =
                numero
                ?? string.Empty;

            MontoGravado =
                montoGravado;

            MontoImpuestos =
                montoImpuestos;
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
    }
}