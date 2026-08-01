using System;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeVisitaDto
    {
        public ReporteViajeVisitaDto(
            int idVisita,
            DateTime fecha,
            string observacion,
            string localidadEncuentro)
        {
            IdVisita = idVisita;
            Fecha = fecha.Date;
            Observacion =
                observacion ?? string.Empty;
            LocalidadEncuentro =
                localidadEncuentro ?? string.Empty;
        }

        public int IdVisita { get; private set; }

        public DateTime Fecha { get; private set; }

        public string Observacion
        {
            get;
            private set;
        }

        public string LocalidadEncuentro
        {
            get;
            private set;
        }
    }
}
