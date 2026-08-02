using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viajes
{
    public sealed class ViajeService
    {
        public const string PermisoConsultar =
            "VIAJE_CONSULTAR";

        public const string PermisoCrear =
            "VIAJE_CREAR";

        public const string PermisoCancelar =
            "VIAJE_CANCELAR";

        private const string ModuloAuditoria =
            "Viajes";

        private const string EntidadAuditoria =
            "Viaje";

        private readonly IViajeRepository
            _viajeRepository;

        private readonly IPersonaConsultaRepository
            _personaRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public ViajeService(
            IViajeRepository viajeRepository,
            IPersonaConsultaRepository personaRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _viajeRepository =
                viajeRepository
                ?? throw new ArgumentNullException(
                    nameof(viajeRepository));

            _personaRepository =
                personaRepository
                ?? throw new ArgumentNullException(
                    nameof(personaRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<ViajeListadoDto> Listar(
            ViajeFiltro filtro)
        {
            ExigirPermiso(
                PermisoConsultar);

            return _viajeRepository.Listar(
                filtro
                ?? ViajeFiltro.CrearSinFiltros());
        }

        public Viaje Obtener(
            int idViaje)
        {
            ExigirPermiso(
                PermisoConsultar);

            ValidarIdViaje(
                idViaje);

            Viaje viaje =
                _viajeRepository.ObtenerPorId(
                    idViaje);

            if (viaje == null)
            {
                throw new ReglaNegocioException(
                    "El viaje indicado no existe.");
            }

            return viaje;
        }

        public IReadOnlyCollection<PersonaSeleccionDto>
            ListarParticipantesDisponibles()
        {
            ExigirPermiso(
                PermisoCrear);

            return _personaRepository
                .ListarActivas();
        }

        public int Registrar(
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            IEnumerable<int> idsParticipantes)
        {
            ExigirPermiso(
                PermisoCrear);

            IReadOnlyCollection<Persona> participantes =
                ObtenerParticipantesActivos(
                    idsParticipantes);

            var viaje =
                new Viaje(
                    0,
                    fechaInicio,
                    fechaFin,
                    descripcion,
                    tipoViaje,
                    montoAnticipado);

            viaje.ReemplazarParticipantes(
                participantes);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Alta",
                    null,
                    "Se registró un viaje y sus participantes.");

            return _viajeRepository.Insertar(
                viaje,
                auditoria);
        }

        public void Modificar(
            int idViaje,
            DateTime fechaInicio,
            DateTime fechaFin,
            string descripcion,
            TipoViaje tipoViaje,
            decimal montoAnticipado,
            IEnumerable<int> idsParticipantes)
        {
            ExigirPermiso(
                PermisoCrear);

            ValidarIdViaje(
                idViaje);

            Viaje viaje =
                _viajeRepository.ObtenerPorId(
                    idViaje);

            if (viaje == null)
            {
                throw new ReglaNegocioException(
                    "El viaje indicado no existe.");
            }

            IReadOnlyCollection<Persona> participantes =
                ObtenerParticipantesParaModificacion(
                    viaje,
                    idsParticipantes);

            viaje.ActualizarDatos(
                fechaInicio,
                fechaFin,
                descripcion,
                tipoViaje,
                montoAnticipado,
                participantes);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Modificacion",
                    viaje.IdViaje,
                    "Se modificaron los datos y participantes del viaje.");

            _viajeRepository.Actualizar(
                viaje,
                auditoria);
        }

        public void Cancelar(
            int idViaje)
        {
            ExigirPermiso(
                PermisoCancelar);

            ValidarIdViaje(
                idViaje);

            Viaje viaje =
                _viajeRepository.ObtenerPorId(
                    idViaje);

            if (viaje == null)
            {
                throw new ReglaNegocioException(
                    "El viaje indicado no existe.");
            }

            viaje.Cancelar();

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Cancelacion",
                    viaje.IdViaje,
                    "Se canceló el viaje.");

            _viajeRepository.Cancelar(
                viaje,
                auditoria);
        }

        private AuditoriaRegistro CrearAuditoria(
            string accion,
            int? idEntidad,
            string descripcion)
        {
            Usuario usuarioActual =
                _sesionActual.UsuarioActual;

            return new AuditoriaRegistro(
                usuarioActual.IdUsuario,
                usuarioActual.NombreUsuario,
                ModuloAuditoria,
                accion,
                EntidadAuditoria,
                idEntidad,
                descripcion);
        }

        private IReadOnlyCollection<Persona>
            ObtenerParticipantesActivos(
                IEnumerable<int> idsParticipantes)
        {
            IReadOnlyCollection<Persona> participantes =
                ObtenerParticipantesExistentes(
                    idsParticipantes);

            if (participantes.Any(
                participante =>
                    !participante.Activo))
            {
                throw new ReglaNegocioException(
                    "Uno o más participantes seleccionados se encuentran inactivos.");
            }

            return participantes;
        }

        private IReadOnlyCollection<Persona>
            ObtenerParticipantesParaModificacion(
                Viaje viaje,
                IEnumerable<int> idsParticipantes)
        {
            IReadOnlyCollection<Persona> participantes =
                ObtenerParticipantesExistentes(
                    idsParticipantes);

            HashSet<int> idsHistoricos =
                new HashSet<int>(
                    viaje.Participantes
                        .Where(
                            participante =>
                                participante.IdPersona > 0)
                        .Select(
                            participante =>
                                participante.IdPersona));

            foreach (Persona participante in participantes)
            {
                if (!participante.Activo &&
                    !idsHistoricos.Contains(
                        participante.IdPersona))
                {
                    throw new ReglaNegocioException(
                        "No se pueden agregar participantes inactivos al viaje.");
                }
            }

            return participantes;
        }

        private IReadOnlyCollection<Persona>
            ObtenerParticipantesExistentes(
                IEnumerable<int> idsParticipantes)
        {
            if (idsParticipantes == null)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar al menos un participante.");
            }

            List<int> ids =
                idsParticipantes.ToList();

            if (ids.Count == 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar al menos un participante.");
            }

            if (ids.Any(
                idPersona =>
                    idPersona <= 0))
            {
                throw new ReglaNegocioException(
                    "Los participantes seleccionados no son válidos.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ReglaNegocioException(
                    "No se permiten participantes duplicados.");
            }

            IReadOnlyCollection<Persona> personas =
                _personaRepository.ObtenerPorIds(
                    ids.AsReadOnly());

            if (personas == null ||
                personas.Count != ids.Count)
            {
                throw new ReglaNegocioException(
                    "Uno o más participantes seleccionados no existen.");
            }

            Dictionary<int, Persona> porId =
                personas.ToDictionary(
                    persona =>
                        persona.IdPersona);

            var ordenadas =
                new List<Persona>();

            foreach (int idPersona in ids)
            {
                Persona persona;

                if (!porId.TryGetValue(
                    idPersona,
                    out persona))
                {
                    throw new ReglaNegocioException(
                        "Uno o más participantes seleccionados no existen.");
                }

                ordenadas.Add(
                    persona);
            }

            return ordenadas.AsReadOnly();
        }

        private void ExigirPermiso(
            string codigoPermiso)
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new AccesoDenegadoException(
                    "Debe iniciar sesión para realizar esta operación.");
            }

            if (!_sesionActual.UsuarioActual.Activo)
            {
                throw new AccesoDenegadoException(
                    "El usuario autenticado se encuentra inactivo.");
            }

            if (!_autorizacionService.TienePermiso(
                _sesionActual.UsuarioActual,
                codigoPermiso))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private static void ValidarIdViaje(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }
        }
    }
}
