using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeDatosDto
    {
        public ReporteViajeDatosDto(
            int idViaje,
            string descripcion,
            TipoViaje tipoViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            EstadoViaje estado,
            decimal montoAnticipado,
            string responsableEnvio,
            DateTime? fechaEnvio,
            string aprobador,
            DateTime? fechaAprobacion,
            string cancelador,
            DateTime? fechaCancelacion,
            string motivoCancelacion,
            IEnumerable<ReporteViajeParticipanteDto>
                participantes,
            IEnumerable<ReporteViajeVisitaDto>
                visitas,
            IEnumerable<ReporteViajeClienteDto>
                clientes,
            IEnumerable<ReporteViajeViaticoDto>
                viaticos)
        {
            IdViaje = idViaje;
            Descripcion =
                descripcion ?? string.Empty;
            TipoViaje = tipoViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Estado = estado;
            MontoAnticipado = montoAnticipado;
            ResponsableEnvio =
                responsableEnvio ?? string.Empty;
            FechaEnvio = fechaEnvio;
            Aprobador =
                aprobador ?? string.Empty;
            FechaAprobacion = fechaAprobacion;
            Cancelador =
                cancelador ?? string.Empty;
            FechaCancelacion = fechaCancelacion;
            MotivoCancelacion =
                motivoCancelacion ?? string.Empty;

            Participantes =
                (participantes ??
                    Enumerable.Empty
                        <ReporteViajeParticipanteDto>())
                .ToList()
                .AsReadOnly();

            Visitas =
                (visitas ??
                    Enumerable.Empty
                        <ReporteViajeVisitaDto>())
                .ToList()
                .AsReadOnly();

            Clientes =
                (clientes ??
                    Enumerable.Empty
                        <ReporteViajeClienteDto>())
                .ToList()
                .AsReadOnly();

            Viaticos =
                (viaticos ??
                    Enumerable.Empty
                        <ReporteViajeViaticoDto>())
                .ToList()
                .AsReadOnly();
        }

        public int IdViaje { get; private set; }

        public string Descripcion
        {
            get;
            private set;
        }

        public TipoViaje TipoViaje
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

        public decimal MontoAnticipado
        {
            get;
            private set;
        }

        public string ResponsableEnvio
        {
            get;
            private set;
        }

        public DateTime? FechaEnvio
        {
            get;
            private set;
        }

        public string Aprobador
        {
            get;
            private set;
        }

        public DateTime? FechaAprobacion
        {
            get;
            private set;
        }

        public string Cancelador
        {
            get;
            private set;
        }

        public DateTime? FechaCancelacion
        {
            get;
            private set;
        }

        public string MotivoCancelacion
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteViajeParticipanteDto>
            Participantes
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteViajeVisitaDto>
            Visitas
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteViajeClienteDto>
            Clientes
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteViajeViaticoDto>
            Viaticos
        {
            get;
            private set;
        }
    }
}
