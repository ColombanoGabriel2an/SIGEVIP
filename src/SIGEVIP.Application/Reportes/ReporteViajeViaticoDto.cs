using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeViaticoDto
    {
        public ReporteViajeViaticoDto(
            int idViatico,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            string pagadoPor,
            decimal monto,
            string descripcion,
            EstadoViatico estado)
        {
            IdViatico = idViatico;
            Fecha = fecha.Date;
            Categoria = categoria;
            MetodoPago = metodoPago;
            PagadoPor =
                pagadoPor ?? string.Empty;
            Monto = monto;
            Descripcion =
                descripcion ?? string.Empty;
            Estado = estado;
        }

        public int IdViatico { get; private set; }

        public DateTime Fecha { get; private set; }

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

        public decimal Monto { get; private set; }

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
    }
}
