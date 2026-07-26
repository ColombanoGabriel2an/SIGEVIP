using System;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Viajes
{
    public sealed class ViajeListadoDto
    {
        public ViajeListadoDto(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            EstadoViaje estado,
            decimal montoAnticipado,
            string participantesResumen)
        {
            IdViaje = idViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion =
                descripcion ?? string.Empty;
            TipoViaje = tipoViaje;
            Estado = estado;
            MontoAnticipado = montoAnticipado;
            ParticipantesResumen =
                participantesResumen ?? string.Empty;
        }

        public int IdViaje { get; private set; }

        public DateTime FechaInicio { get; private set; }

        public DateTime FechaFin { get; private set; }

        public string Descripcion { get; private set; }

        public TipoViaje TipoViaje { get; private set; }

        public EstadoViaje Estado { get; private set; }

        public decimal MontoAnticipado { get; private set; }

        public string ParticipantesResumen { get; private set; }
    }
}
