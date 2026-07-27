using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viaticos
{
    public sealed class RegistrarViaticoCommand
    {
        public RegistrarViaticoCommand(
            int idViaje,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            int? idPersonaPagadora,
            decimal monto,
            string descripcion,
            ComprobanteInput comprobante)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }

            ValidarCategoria(
                categoria);

            ValidarMetodoPago(
                metodoPago);

            if (idPersonaPagadora.HasValue &&
                idPersonaPagadora.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersonaPagadora),
                    "El identificador de la persona pagadora debe ser mayor que cero.");
            }

            IdViaje =
                idViaje;

            Fecha =
                fecha.Date;

            Categoria =
                categoria;

            MetodoPago =
                metodoPago;

            IdPersonaPagadora =
                idPersonaPagadora;

            Monto =
                monto;

            Descripcion =
                descripcion
                ?? string.Empty;

            Comprobante =
                comprobante;
        }

        public int IdViaje
        {
            get;
            private set;
        }

        public DateTime Fecha
        {
            get;
            private set;
        }

        public CategoriaGasto Categoria
        {
            get;
            private set;
        }

        public MetodoPago MetodoPago
        {
            get;
            private set;
        }

        public int? IdPersonaPagadora
        {
            get;
            private set;
        }

        public decimal Monto
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get;
            private set;
        }

        public ComprobanteInput Comprobante
        {
            get;
            private set;
        }

        private static void ValidarCategoria(
            CategoriaGasto categoria)
        {
            if (!Enum.IsDefined(
                typeof(CategoriaGasto),
                categoria))
            {
                throw new ReglaNegocioException(
                    "La categoría de gasto indicada no es válida.");
            }
        }

        private static void ValidarMetodoPago(
            MetodoPago metodoPago)
        {
            if (!Enum.IsDefined(
                typeof(MetodoPago),
                metodoPago))
            {
                throw new ReglaNegocioException(
                    "El método de pago indicado no es válido.");
            }
        }
    }
}