using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionComprobanteDto
    {
        public RendicionComprobanteDto(
            int idComprobante,
            TipoComprobante tipo,
            string cuitProveedor,
            string razonSocialProveedor,
            SituacionFiscal situacionFiscal,
            string sucursal,
            string numero,
            decimal montoGravado,
            decimal montoImpuestos,
            decimal total)
        {
            IdComprobante = idComprobante;
            Tipo = tipo;
            CuitProveedor =
                cuitProveedor ?? string.Empty;
            RazonSocialProveedor =
                razonSocialProveedor ?? string.Empty;
            SituacionFiscal = situacionFiscal;
            Sucursal =
                sucursal ?? string.Empty;
            Numero =
                numero ?? string.Empty;
            MontoGravado = montoGravado;
            MontoImpuestos = montoImpuestos;
            Total = total;
        }

        public int IdComprobante { get; private set; }

        public TipoComprobante Tipo { get; private set; }

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

        public string Sucursal { get; private set; }

        public string Numero { get; private set; }

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

        public decimal Total { get; private set; }
    }
}