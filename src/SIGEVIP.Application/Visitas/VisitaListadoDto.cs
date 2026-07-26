using System;

namespace SIGEVIP.Application.Visitas
{
    public sealed class VisitaListadoDto
    {
        public VisitaListadoDto(
            int idVisita,
            int idViaje,
            DateTime fecha,
            string observacion,
            string localidadEncuentro,
            string clientesResumen)
        {
            IdVisita = idVisita;
            IdViaje = idViaje;
            Fecha = fecha.Date;
            Observacion =
                observacion ?? string.Empty;
            LocalidadEncuentro =
                localidadEncuentro ?? string.Empty;
            ClientesResumen =
                clientesResumen ?? string.Empty;
        }

        public int IdVisita { get; private set; }

        public int IdViaje { get; private set; }

        public DateTime Fecha { get; private set; }

        public string Observacion { get; private set; }

        public string LocalidadEncuentro
        {
            get;
            private set;
        }

        public string ClientesResumen
        {
            get;
            private set;
        }
    }
}