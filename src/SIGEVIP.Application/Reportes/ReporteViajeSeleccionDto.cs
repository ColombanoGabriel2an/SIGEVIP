using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeSeleccionDto
    {
        public ReporteViajeSeleccionDto(
            int idViaje,
            string descripcion,
            DateTime fechaInicio,
            DateTime fechaFin,
            EstadoViaje estado)
        {
            IdViaje = idViaje;
            Descripcion =
                descripcion ?? string.Empty;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Estado = estado;
        }

        public int IdViaje { get; private set; }

        public string Descripcion
        {
            get;
            private set;
        }

        public DateTime FechaInicio
        {
            get;
            private set;
        }

        public DateTime FechaFin
        {
            get;
            private set;
        }

        public EstadoViaje Estado
        {
            get;
            private set;
        }

        public string Presentacion
        {
            get
            {
                return "#" +
                    IdViaje +
                    " - " +
                    Descripcion +
                    " (" +
                    FechaInicio.ToString("dd/MM/yyyy") +
                    " - " +
                    FechaFin.ToString("dd/MM/yyyy") +
                    ")";
            }
        }
    }
}
