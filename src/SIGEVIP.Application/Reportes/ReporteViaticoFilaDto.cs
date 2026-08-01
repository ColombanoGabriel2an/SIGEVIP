using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViaticoFilaDto
    {
        public ReporteViaticoFilaDto(
            int idViatico,
            int idViaje,
            string viaje,
            EstadoViaje estadoViaje,
            DateTime fecha,
            int? idPersonaPagadora,
            string personaPagadora,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            decimal monto,
            EstadoViatico estadoViatico,
            string descripcion)
        {
            IdViatico = idViatico;
            IdViaje = idViaje;
            Viaje =
                viaje ?? string.Empty;
            EstadoViaje = estadoViaje;
            Fecha = fecha.Date;
            IdPersonaPagadora =
                idPersonaPagadora;
            PersonaPagadora =
                string.IsNullOrWhiteSpace(
                    personaPagadora)
                    ? "Sin persona pagadora"
                    : personaPagadora;
            Categoria = categoria;
            MetodoPago = metodoPago;
            Monto = monto;
            EstadoViatico = estadoViatico;
            Descripcion =
                descripcion ?? string.Empty;
        }

        public int IdViatico { get; private set; }
        public int IdViaje { get; private set; }
        public string Viaje { get; private set; }
        public EstadoViaje EstadoViaje { get; private set; }
        public DateTime Fecha { get; private set; }
        public int? IdPersonaPagadora { get; private set; }
        public string PersonaPagadora { get; private set; }
        public CategoriaGasto Categoria { get; private set; }
        public MetodoPago MetodoPago { get; private set; }
        public decimal Monto { get; private set; }
        public EstadoViatico EstadoViatico { get; private set; }
        public string Descripcion { get; private set; }
    }
}
