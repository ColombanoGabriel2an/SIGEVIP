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
        private readonly List<Visita> _visitas;
        private readonly List<Persona> _participantes;
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
            ValidarDatosGenerales(
                fechaInicio,
                fechaFin,
                descripcion,
                tipoViaje,
                montoAnticipado);

            IdViaje = idViaje;
            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion = descripcion.Trim();
            TipoViaje = tipoViaje;
            MontoAnticipado = montoAnticipado;

            _viaticos =
                new List<Viatico>();

            _visitas =
                new List<Visita>();

            _participantes =
                new List<Persona>();

            _estadoActual =
                EstadoViajeFactory.Crear(
                    estado);
        }

        public int IdViaje { get; private set; }

        public DateTime FechaInicio { get; private set; }

        public DateTime FechaFin { get; private set; }

        public string Descripcion { get; private set; }

        public TipoViaje TipoViaje { get; private set; }

        public decimal MontoAnticipado { get; private set; }

        public EstadoViaje EstadoActual
        {
            get
            {
                return _estadoActual.Estado;
            }
        }

        public IReadOnlyCollection<Viatico> Viaticos
        {
            get
            {
                return new ReadOnlyCollection<Viatico>(
                    _viaticos);
            }
        }

        public IReadOnlyCollection<Visita> Visitas
        {
            get
            {
                return new ReadOnlyCollection<Visita>(
                    _visitas);
            }
        }

        public IReadOnlyCollection<Persona> Participantes
        {
            get
            {
                return new ReadOnlyCollection<Persona>(
                    _participantes);
            }
        }

        public decimal TotalGastado
        {
            get
            {
                return _viaticos
                    .Where(
                        viatico =>
                            viatico.Estado ==
                            EstadoViatico.Vigente)
                    .Sum(
                        viatico =>
                            viatico.Monto);
            }
        }

        public decimal SaldoPendiente
        {
            get
            {
                return TotalGastado -
                       MontoAnticipado;
            }
        }

        public static Viaje Reconstruir(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            EstadoViaje estado,
            IEnumerable<Persona> participantes)
        {
            return Reconstruir(
                idViaje,
                fechaInicio,
                fechaFin,
                descripcion,
                tipoViaje,
                montoAnticipado,
                estado,
                participantes,
                Enumerable.Empty<Visita>());
        }

        public static Viaje Reconstruir(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            EstadoViaje estado,
            IEnumerable<Persona> participantes,
            IEnumerable<Visita> visitas)
        {
            if (idViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido del viaje debe ser válido.");
            }

            var viaje =
                new Viaje(
                    idViaje,
                    fechaInicio,
                    fechaFin,
                    descripcion,
                    tipoViaje,
                    montoAnticipado,
                    estado);

            viaje.CargarParticipantes(
                participantes,
                true);

            viaje.CargarVisitasReconstruidas(
                visitas);

            return viaje;
        }

        public void AgregarParticipante(
            Persona participante)
        {
            _estadoActual.ValidarModificacion();

            ValidarParticipante(
                participante);

            if (_participantes.Any(
                existente =>
                    SonLaMismaPersona(
                        existente,
                        participante)))
            {
                throw new ReglaNegocioException(
                    "La persona ya participa del viaje.");
            }

            _participantes.Add(
                participante);
        }

        public void QuitarParticipante(
            Persona participante)
        {
            _estadoActual.ValidarModificacion();

            if (participante == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un participante válido.");
            }

            Persona existente =
                _participantes.FirstOrDefault(
                    item =>
                        SonLaMismaPersona(
                            item,
                            participante));

            if (existente == null)
            {
                throw new ReglaNegocioException(
                    "La persona indicada no participa del viaje.");
            }

            if (_participantes.Count == 1)
            {
                throw new ReglaNegocioException(
                    "El viaje debe conservar al menos un participante.");
            }

            _participantes.Remove(
                existente);
        }

        public void ReemplazarParticipantes(
            IEnumerable<Persona> participantes)
        {
            _estadoActual.ValidarModificacion();

            CargarParticipantes(
                participantes,
                true);
        }

        public void ActualizarDatos(
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            IEnumerable<Persona> participantes)
        {
            _estadoActual.ValidarModificacion();

            ValidarDatosGenerales(
                fechaInicio,
                fechaFin,
                descripcion,
                tipoViaje,
                montoAnticipado);

            ValidarElementosDentroDelPeriodo(
                fechaInicio.Date,
                fechaFin.Date);

            List<Persona> participantesValidados =
                ValidarYMaterializarParticipantes(
                    participantes,
                    true);

            FechaInicio = fechaInicio.Date;
            FechaFin = fechaFin.Date;
            Descripcion = descripcion.Trim();
            TipoViaje = tipoViaje;
            MontoAnticipado = montoAnticipado;

            _participantes.Clear();
            _participantes.AddRange(
                participantesValidados);
        }

        public void AgregarViatico(
            Viatico viatico)
        {
            if (viatico == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un viático válido.");
            }

            _estadoActual.ValidarModificacion();

            ValidarFechaViatico(
                viatico.Fecha);

            if (_viaticos.Any(
                item =>
                    ReferenceEquals(
                        item,
                        viatico)))
            {
                throw new ReglaNegocioException(
                    "El viático ya fue agregado al viaje.");
            }

            viatico.AsociarAViaje(
                IdViaje);

            _viaticos.Add(
                viatico);
        }

        public void AgregarVisita(
            Visita visita)
        {
            if (visita == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar una visita válida.");
            }

            _estadoActual.ValidarModificacion();

            ValidarFechaVisita(
                visita.Fecha);

            if (!visita.TieneClientes)
            {
                throw new ReglaNegocioException(
                    "La visita debe tener al menos un cliente asociado.");
            }

            if (_visitas.Any(
                item =>
                    SonLaMismaVisita(
                        item,
                        visita)))
            {
                throw new ReglaNegocioException(
                    "La visita ya fue agregada al viaje.");
            }

            visita.AsociarAViaje(
                IdViaje);

            _visitas.Add(
                visita);
        }

        public void ExcluirViatico(
            Viatico viatico)
        {
            ValidarViaticoPerteneciente(
                viatico);

            if (EstadoActual !=
                EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "Los viáticos solo pueden excluirse mientras el viaje está EnRendicion.");
            }

            viatico.Excluir();
        }

        public void ReactivarViatico(
            Viatico viatico)
        {
            ValidarViaticoPerteneciente(
                viatico);

            if (EstadoActual !=
                EstadoViaje.EnRendicion)
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
            _estadoActual =
                _estadoActual
                    .EnviarARendicion();
        }

        public void Aprobar()
        {
            _estadoActual =
                _estadoActual.Aprobar();
        }

        public void Cancelar()
        {
            if (_visitas.Count > 0)
            {
                throw new ReglaNegocioException(
                    "El viaje no puede cancelarse porque posee visitas registradas.");
            }

            _estadoActual =
                _estadoActual.Cancelar();
        }

        private void CargarParticipantes(
            IEnumerable<Persona> participantes,
            bool exigirAlMenosUno)
        {
            List<Persona> materializados =
                ValidarYMaterializarParticipantes(
                    participantes,
                    exigirAlMenosUno);

            _participantes.Clear();
            _participantes.AddRange(
                materializados);
        }

        private void CargarVisitasReconstruidas(
            IEnumerable<Visita> visitas)
        {
            if (visitas == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar las visitas persistidas del viaje.");
            }

            List<Visita> materializadas =
                visitas.ToList();

            var resultado =
                new List<Visita>();

            foreach (
                Visita visita
                in materializadas)
            {
                if (visita == null)
                {
                    throw new ReglaNegocioException(
                        "Las visitas persistidas del viaje deben ser válidas.");
                }

                if (visita.IdVisita <= 0)
                {
                    throw new ReglaNegocioException(
                        "Las visitas persistidas deben tener un identificador válido.");
                }

                ValidarFechaVisita(
                    visita.Fecha);

                if (!visita.TieneClientes)
                {
                    throw new ReglaNegocioException(
                        "La visita persistida debe tener al menos un cliente asociado.");
                }

                if (resultado.Any(
                    existente =>
                        SonLaMismaVisita(
                            existente,
                            visita)))
                {
                    throw new ReglaNegocioException(
                        "No se permiten visitas persistidas duplicadas en el viaje.");
                }

                visita.AsociarAViaje(
                    IdViaje);

                resultado.Add(
                    visita);
            }

            _visitas.Clear();
            _visitas.AddRange(
                resultado);
        }

        private static List<Persona>
            ValidarYMaterializarParticipantes(
                IEnumerable<Persona> participantes,
                bool exigirAlMenosUno)
        {
            if (participantes == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar los participantes del viaje.");
            }

            List<Persona> materializados =
                participantes.ToList();

            if (exigirAlMenosUno &&
                materializados.Count == 0)
            {
                throw new ReglaNegocioException(
                    "El viaje debe tener al menos un participante.");
            }

            var resultado =
                new List<Persona>();

            foreach (
                Persona participante
                in materializados)
            {
                ValidarParticipante(
                    participante);

                if (resultado.Any(
                    existente =>
                        SonLaMismaPersona(
                            existente,
                            participante)))
                {
                    throw new ReglaNegocioException(
                        "No se permiten participantes duplicados en el viaje.");
                }

                resultado.Add(
                    participante);
            }

            return resultado;
        }

        private static void ValidarParticipante(
            Persona participante)
        {
            if (participante == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un participante válido.");
            }
        }

        private static bool SonLaMismaPersona(
            Persona existente,
            Persona candidata)
        {
            if (ReferenceEquals(
                existente,
                candidata))
            {
                return true;
            }

            return existente.IdPersona > 0 &&
                   candidata.IdPersona > 0 &&
                   existente.IdPersona ==
                   candidata.IdPersona;
        }

        private static void ValidarDatosGenerales(
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado)
        {
            if (fechaInicio.Date >
                fechaFin.Date)
            {
                throw new ReglaNegocioException(
                    "La fecha de inicio del viaje no puede ser posterior a la fecha de fin.");
            }

            if (string.IsNullOrWhiteSpace(
                descripcion))
            {
                throw new ReglaNegocioException(
                    "La descripción del viaje es obligatoria.");
            }

            if (!Enum.IsDefined(
                typeof(TipoViaje),
                tipoViaje))
            {
                throw new ReglaNegocioException(
                    "El tipo de viaje indicado no es válido.");
            }

            if (montoAnticipado < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto anticipado del viaje no puede ser negativo.");
            }
        }

        private void ValidarElementosDentroDelPeriodo(
            DateTime nuevaFechaInicio,
            DateTime nuevaFechaFin)
        {
            if (_visitas.Any(
                visita =>
                    visita.Fecha.Date <
                        nuevaFechaInicio ||
                    visita.Fecha.Date >
                        nuevaFechaFin))
            {
                throw new ReglaNegocioException(
                    "El nuevo período dejaría visitas fuera de las fechas del viaje.");
            }

            if (_viaticos.Any(
                viatico =>
                    viatico.Fecha.Date <
                        nuevaFechaInicio ||
                    viatico.Fecha.Date >
                        nuevaFechaFin))
            {
                throw new ReglaNegocioException(
                    "El nuevo período dejaría viáticos fuera de las fechas del viaje.");
            }
        }

        private void ValidarFechaViatico(
            DateTime fechaViatico)
        {
            DateTime fecha =
                fechaViatico.Date;

            if (fecha < FechaInicio ||
                fecha > FechaFin)
            {
                throw new ReglaNegocioException(
                    "La fecha del viático debe encontrarse dentro del período del viaje.");
            }
        }

        private void ValidarFechaVisita(
            DateTime fechaVisita)
        {
            DateTime fecha =
                fechaVisita.Date;

            if (fecha < FechaInicio ||
                fecha > FechaFin)
            {
                throw new ReglaNegocioException(
                    "La fecha de la visita debe encontrarse dentro del período del viaje.");
            }
        }

        private void ValidarViaticoPerteneciente(
            Viatico viatico)
        {
            if (viatico == null ||
                !_viaticos.Contains(
                    viatico))
            {
                throw new ReglaNegocioException(
                    "El viático indicado no pertenece al viaje.");
            }
        }

        private static bool SonLaMismaVisita(
            Visita existente,
            Visita candidata)
        {
            if (ReferenceEquals(
                existente,
                candidata))
            {
                return true;
            }

            return existente.IdVisita > 0 &&
                   candidata.IdVisita > 0 &&
                   existente.IdVisita ==
                   candidata.IdVisita;
        }
    }
}