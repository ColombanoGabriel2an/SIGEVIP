using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Domain.States;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Viaje
    {
        private readonly List<Viatico> _viaticos;
        private IEstadoViaje _estadoActual;

        public Viaje(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado)
            : this(
                idViaje,
                fechaInicio,
                fechaFin,
                descripcion,
                tipoViaje,
                montoAnticipado,
                EstadoViaje.Abierto)
        {
        }

        public Viaje(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            EstadoViaje estado)
        {
            if (fechaInicio.Date > fechaFin.Date)
            {
                throw new ReglaNegocioException(
                    "La fecha de inicio del viaje no puede ser posterior a la fecha de fin.");
            }

            if (montoAnticipado < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto anticipado del viaje no puede ser negativo.");
            }

            if (!Enum.IsDefined(typeof(TipoViaje), tipoViaje))
            {
                throw new ReglaNegocioException(
                    "El tipo de viaje indicado no es válido.");
            }

            IdViaje = idViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion = descripcion ?? string.Empty;
            TipoViaje = tipoViaje;
            MontoAnticipado = montoAnticipado;

            _viaticos = new List<Viatico>();
            _estadoActual = EstadoViajeFactory.Crear(estado);
        }

        public int IdViaje { get; private set; }

        public DateTime FechaInicio { get; private set; }

        public DateTime FechaFin { get; private set; }

        public string Descripcion { get; private set; }

        public TipoViaje TipoViaje { get; private set; }

        public decimal MontoAnticipado { get; private set; }

        public EstadoViaje EstadoActual
        {
            get { return _estadoActual.Estado; }
        }

        public IReadOnlyCollection<Viatico> Viaticos
        {
            get { return new ReadOnlyCollection<Viatico>(_viaticos); }
        }

        public decimal TotalGastado
        {
            get
            {
                return _viaticos
                    .Where(viatico => viatico.Estado == EstadoViatico.Vigente)
                    .Sum(viatico => viatico.Monto);
            }
        }

        public decimal SaldoPendiente
        {
            get { return TotalGastado - MontoAnticipado; }
        }

        public void AgregarViatico(Viatico viatico)
        {
            if (viatico == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un viático válido.");
            }

            _estadoActual.ValidarModificacion();
            ValidarFechaViatico(viatico.Fecha);

            if (_viaticos.Any(item => ReferenceEquals(item, viatico)))
            {
                throw new ReglaNegocioException(
                    "El viático ya fue agregado al viaje.");
            }

            viatico.AsociarAViaje(IdViaje);
            _viaticos.Add(viatico);
        }

        public void ExcluirViatico(Viatico viatico)
        {
            ValidarViaticoPerteneciente(viatico);

            if (EstadoActual != EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "Los viáticos solo pueden excluirse mientras el viaje está EnRendicion.");
            }

            viatico.Excluir();
        }

        public void ReactivarViatico(Viatico viatico)
        {
            ValidarViaticoPerteneciente(viatico);

            if (EstadoActual != EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "Los viáticos solo pueden reactivarse mientras el viaje está EnRendicion.");
            }

            viatico.Reactivar();
        }

        public decimal CalcularSaldo()
        {
            return SaldoPendiente;
        }

        public void EnviarARendicion()
        {
            _estadoActual = _estadoActual.EnviarARendicion();
        }

        public void Aprobar()
        {
            _estadoActual = _estadoActual.Aprobar();
        }

        public void Cancelar()
        {
            _estadoActual = _estadoActual.Cancelar();
        }

        private void ValidarFechaViatico(DateTime fechaViatico)
        {
            DateTime fecha = fechaViatico.Date;

            if (fecha < FechaInicio || fecha > FechaFin)
            {
                throw new ReglaNegocioException(
                    "La fecha del viático debe encontrarse dentro del período del viaje.");
            }
        }

        private void ValidarViaticoPerteneciente(Viatico viatico)
        {
            if (viatico == null || !_viaticos.Contains(viatico))
            {
                throw new ReglaNegocioException(
                    "El viático indicado no pertenece al viaje.");
            }
        }
    }
}
