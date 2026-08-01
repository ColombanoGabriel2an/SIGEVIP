using System;
using System.Collections.Generic;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViajeResumenDto
    {
        public ReporteViajeResumenDto(
            ReporteViajeDatosDto datos,
            int cantidadParticipantes,
            int cantidadVisitas,
            int cantidadClientesDistintos,
            int cantidadViaticos,
            decimal totalRegistrado,
            decimal totalVigente,
            decimal totalExcluido,
            decimal gastosComputablesSaldo,
            decimal diferenciaAnticipo,
            string tipoDiferencia)
        {
            if (datos == null)
            {
                throw new ArgumentNullException(
                    nameof(datos));
            }

            IdViaje = datos.IdViaje;
            Descripcion = datos.Descripcion;
            TipoViaje = datos.TipoViaje;
            FechaInicio = datos.FechaInicio;
            FechaFin = datos.FechaFin;
            Estado = datos.Estado;
            MontoAnticipado =
                datos.MontoAnticipado;
            ResponsableEnvio =
                datos.ResponsableEnvio;
            FechaEnvio = datos.FechaEnvio;
            Aprobador = datos.Aprobador;
            FechaAprobacion =
                datos.FechaAprobacion;
            Cancelador = datos.Cancelador;
            FechaCancelacion =
                datos.FechaCancelacion;
            MotivoCancelacion =
                datos.MotivoCancelacion;
            Participantes = datos.Participantes;
            Visitas = datos.Visitas;
            Clientes = datos.Clientes;
            Viaticos = datos.Viaticos;
            CantidadParticipantes =
                cantidadParticipantes;
            CantidadVisitas = cantidadVisitas;
            CantidadClientesDistintos =
                cantidadClientesDistintos;
            CantidadViaticos = cantidadViaticos;
            TotalRegistrado = totalRegistrado;
            TotalVigente = totalVigente;
            TotalExcluido = totalExcluido;
            TotalGastos = totalVigente;
            GastosComputablesSaldo =
                gastosComputablesSaldo;
            DiferenciaAnticipo =
                diferenciaAnticipo;
            Saldo = diferenciaAnticipo;
            ImporteDiferencia =
                Math.Abs(
                    diferenciaAnticipo);
            TipoDiferencia =
                tipoDiferencia ?? string.Empty;
            TipoSaldo = TipoDiferencia;
        }

        public int IdViaje { get; private set; }
        public string Descripcion { get; private set; }
        public TipoViaje TipoViaje { get; private set; }
        public DateTime FechaInicio { get; private set; }
        public DateTime FechaFin { get; private set; }
        public EstadoViaje Estado { get; private set; }
        public decimal MontoAnticipado { get; private set; }
        public string ResponsableEnvio { get; private set; }
        public DateTime? FechaEnvio { get; private set; }
        public string Aprobador { get; private set; }
        public DateTime? FechaAprobacion { get; private set; }
        public string Cancelador { get; private set; }
        public DateTime? FechaCancelacion { get; private set; }
        public string MotivoCancelacion { get; private set; }

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

        public int CantidadParticipantes
        {
            get;
            private set;
        }

        public int CantidadVisitas
        {
            get;
            private set;
        }

        public int CantidadClientesDistintos
        {
            get;
            private set;
        }

        public int CantidadViaticos
        {
            get;
            private set;
        }

        public decimal TotalRegistrado
        {
            get;
            private set;
        }

        public decimal TotalVigente
        {
            get;
            private set;
        }

        public decimal TotalExcluido
        {
            get;
            private set;
        }

        public decimal TotalGastos
        {
            get;
            private set;
        }

        public decimal GastosComputablesSaldo
        {
            get;
            private set;
        }

        public decimal DiferenciaAnticipo
        {
            get;
            private set;
        }

        public decimal Saldo
        {
            get;
            private set;
        }

        public decimal ImporteDiferencia
        {
            get;
            private set;
        }

        public string TipoDiferencia
        {
            get;
            private set;
        }

        public string TipoSaldo
        {
            get;
            private set;
        }
    }
}
