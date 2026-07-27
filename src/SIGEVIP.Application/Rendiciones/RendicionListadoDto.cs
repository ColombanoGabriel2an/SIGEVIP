using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionListadoDto
    {
        public RendicionListadoDto(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            string participantesResumen,
            decimal montoAnticipado,
            decimal totalGastado,
            decimal saldo,
            DateTime? fechaEnvioRendicion)
        {
            IdViaje = idViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion =
                descripcion ?? string.Empty;
            TipoViaje = tipoViaje;
            ParticipantesResumen =
                participantesResumen ?? string.Empty;
            MontoAnticipado = montoAnticipado;
            TotalGastado = totalGastado;
            Saldo = saldo;
            FechaEnvioRendicion =
                fechaEnvioRendicion;
        }

        public int IdViaje { get; private set; }

        public DateTime FechaInicio { get; private set; }

        public DateTime FechaFin { get; private set; }

        public string Descripcion { get; private set; }

        public TipoViaje TipoViaje { get; private set; }

        public string ParticipantesResumen
        {
            get;
            private set;
        }

        public decimal MontoAnticipado
        {
            get;
            private set;
        }

        public decimal TotalGastado
        {
            get;
            private set;
        }

        public decimal Saldo
        {
            get;
            private set;
        }

        public DateTime? FechaEnvioRendicion
        {
            get;
            private set;
        }
    }
}