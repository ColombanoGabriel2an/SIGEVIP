using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionViaticoDto
    {
        public RendicionViaticoDto(
            int idViatico,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            string pagadoPor,
            decimal monto,
            string descripcion,
            EstadoViatico estado,
            RendicionComprobanteDto comprobante,
            string motivoExclusion,
            int? idUsuarioExclusion,
            DateTime? fechaExclusion,
            int? idUsuarioReactivacion,
            DateTime? fechaReactivacion)
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
            Comprobante = comprobante;
            MotivoExclusion =
                motivoExclusion ?? string.Empty;
            IdUsuarioExclusion =
                idUsuarioExclusion;
            FechaExclusion =
                fechaExclusion;
            IdUsuarioReactivacion =
                idUsuarioReactivacion;
            FechaReactivacion =
                fechaReactivacion;
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

        public string PagadoPor { get; private set; }

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

        public RendicionComprobanteDto Comprobante
        {
            get;
            private set;
        }

        public bool TieneComprobante
        {
            get
            {
                return Comprobante != null;
            }
        }

        public string MotivoExclusion
        {
            get;
            private set;
        }

        public int? IdUsuarioExclusion
        {
            get;
            private set;
        }

        public DateTime? FechaExclusion
        {
            get;
            private set;
        }

        public int? IdUsuarioReactivacion
        {
            get;
            private set;
        }

        public DateTime? FechaReactivacion
        {
            get;
            private set;
        }
    }
}