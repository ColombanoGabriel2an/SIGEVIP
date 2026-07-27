using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionDetalleDto
    {
        public RendicionDetalleDto(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            EstadoViaje estado,
            decimal montoAnticipado,
            decimal totalGastado,
            decimal saldo,
            IEnumerable<RendicionParticipanteDto> participantes,
            IEnumerable<RendicionVisitaDto> visitas,
            IEnumerable<RendicionViaticoDto> viaticos,
            int? idUsuarioEnvioRendicion,
            DateTime? fechaEnvioRendicion)
        {
            IdViaje = idViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion =
                descripcion ?? string.Empty;
            TipoViaje = tipoViaje;
            Estado = estado;
            MontoAnticipado = montoAnticipado;
            TotalGastado = totalGastado;
            Saldo = saldo;

            Participantes =
                (participantes ??
                    Enumerable.Empty<RendicionParticipanteDto>())
                .ToList()
                .AsReadOnly();

            Visitas =
                (visitas ??
                    Enumerable.Empty<RendicionVisitaDto>())
                .ToList()
                .AsReadOnly();

            Viaticos =
                (viaticos ??
                    Enumerable.Empty<RendicionViaticoDto>())
                .ToList()
                .AsReadOnly();

            IdUsuarioEnvioRendicion =
                idUsuarioEnvioRendicion;

            FechaEnvioRendicion =
                fechaEnvioRendicion;
        }

        public int IdViaje { get; private set; }

        public DateTime FechaInicio { get; private set; }

        public DateTime FechaFin { get; private set; }

        public string Descripcion { get; private set; }

        public TipoViaje TipoViaje { get; private set; }

        public EstadoViaje Estado { get; private set; }

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

        public decimal Saldo { get; private set; }

        public IReadOnlyCollection<RendicionParticipanteDto>
            Participantes
        {
            get;
            private set;
        }

        public IReadOnlyCollection<RendicionVisitaDto>
            Visitas
        {
            get;
            private set;
        }

        public IReadOnlyCollection<RendicionViaticoDto>
            Viaticos
        {
            get;
            private set;
        }

        public int? IdUsuarioEnvioRendicion
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