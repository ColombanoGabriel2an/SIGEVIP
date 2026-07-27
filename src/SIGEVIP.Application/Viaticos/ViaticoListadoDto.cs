using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Viaticos
{
    public sealed class ViaticoListadoDto
    {
        public ViaticoListadoDto(
            int idViatico,
            int idViaje,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            string pagadoPor,
            decimal monto,
            string descripcion,
            EstadoViatico estado,
            bool tieneComprobante)
        {
            IdViatico =
                idViatico;

            IdViaje =
                idViaje;

            Fecha =
                fecha.Date;

            Categoria =
                categoria;

            MetodoPago =
                metodoPago;

            PagadoPor =
                pagadoPor
                ?? string.Empty;

            Monto =
                monto;

            Descripcion =
                descripcion
                ?? string.Empty;

            Estado =
                estado;

            TieneComprobante =
                tieneComprobante;
        }

        public int IdViatico
        {
            get;
            private set;
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

        public string PagadoPor
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

        public EstadoViatico Estado
        {
            get;
            private set;
        }

        public bool TieneComprobante
        {
            get;
            private set;
        }
    }
}