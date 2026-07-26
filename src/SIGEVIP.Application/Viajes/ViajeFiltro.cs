using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viajes
{
    public sealed class ViajeFiltro
    {
        public ViajeFiltro(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            EstadoViaje? estado,
            int? idParticipante)
        {
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                    fechaHasta.Value.Date)
            {
                throw new ReglaNegocioException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }

            if (estado.HasValue &&
                !Enum.IsDefined(
                    typeof(EstadoViaje),
                    estado.Value))
            {
                throw new ReglaNegocioException(
                    "El estado utilizado como filtro no es válido.");
            }

            if (idParticipante.HasValue &&
                idParticipante.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idParticipante),
                    "El identificador del participante debe ser mayor que cero.");
            }

            FechaDesde =
                fechaDesde.HasValue
                    ? fechaDesde.Value.Date
                    : (DateTime?)null;

            FechaHasta =
                fechaHasta.HasValue
                    ? fechaHasta.Value.Date
                    : (DateTime?)null;

            Estado = estado;
            IdParticipante = idParticipante;
        }

        public DateTime? FechaDesde { get; private set; }

        public DateTime? FechaHasta { get; private set; }

        public EstadoViaje? Estado { get; private set; }

        public int? IdParticipante { get; private set; }

        public static ViajeFiltro CrearSinFiltros()
        {
            return new ViajeFiltro(
                null,
                null,
                null,
                null);
        }
    }
}
