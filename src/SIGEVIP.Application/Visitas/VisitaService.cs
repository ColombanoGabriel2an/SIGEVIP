using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Visitas
{
    public sealed class VisitaService
    {
        public const string PermisoRegistrar =
            "VISITA_REGISTRAR";

        private readonly IVisitaRepository
            _visitaRepository;

        private readonly IClienteConsultaVisitaRepository
            _clienteRepository;

        private readonly IViajeRepository
            _viajeRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public VisitaService(
            IVisitaRepository visitaRepository,
            IClienteConsultaVisitaRepository clienteRepository,
            IViajeRepository viajeRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _visitaRepository =
                visitaRepository
                ?? throw new ArgumentNullException(
                    nameof(visitaRepository));

            _clienteRepository =
                clienteRepository
                ?? throw new ArgumentNullException(
                    nameof(clienteRepository));

            _viajeRepository =
                viajeRepository
                ?? throw new ArgumentNullException(
                    nameof(viajeRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<ClienteSeleccionVisitaDto>
            ListarClientesDisponibles()
        {
            ExigirPermiso(
                PermisoRegistrar);

            IReadOnlyCollection<ClienteSeleccionVisitaDto>
                resultados =
                    _clienteRepository
                        .ListarActivos();

            return resultados
                   ?? new List<ClienteSeleccionVisitaDto>()
                       .AsReadOnly();
        }

        public int Registrar(
            int idViaje,
            DateTime fecha,
            string observacion,
            string localidadEncuentro,
            IEnumerable<int> idsClientes)
        {
            ExigirPermiso(
                PermisoRegistrar);

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

            IReadOnlyCollection<Cliente> clientes =
                ObtenerClientesActivos(
                    idsClientes);

            var visita =
                new Visita(
                    0,
                    fecha,
                    observacion,
                    localidadEncuentro);

            foreach (
                Cliente cliente
                in clientes)
            {
                visita.AgregarCliente(
                    cliente);
            }

            viaje.AgregarVisita(
                visita);

            return _visitaRepository.Insertar(
                visita);
        }

        public IReadOnlyCollection<VisitaListadoDto>
            ListarPorViaje(
                int idViaje)
        {
            ExigirPermiso(
                ViajeService.PermisoConsultar);

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

            IReadOnlyCollection<VisitaListadoDto>
                resultados =
                    _visitaRepository
                        .ListarPorViaje(
                            idViaje);

            return resultados
                   ?? new List<VisitaListadoDto>()
                       .AsReadOnly();
        }

        public IReadOnlyCollection<VisitaListadoDto>
            ListarPorCliente(
                int idCliente)
        {
            ExigirPermiso(
                ClienteService.PermisoConsultar);

            ValidarIdCliente(
                idCliente);

            IReadOnlyCollection<Cliente> clientes =
                _clienteRepository.ObtenerPorIds(
                    new List<int>
                    {
                        idCliente
                    }.AsReadOnly());

            if (clientes == null ||
                clientes.Count != 1 ||
                clientes.Single().IdCliente != idCliente)
            {
                throw new ReglaNegocioException(
                    "El cliente indicado no existe.");
            }

            IReadOnlyCollection<VisitaListadoDto>
                resultados =
                    _visitaRepository
                        .ListarPorCliente(
                            idCliente);

            return resultados
                   ?? new List<VisitaListadoDto>()
                       .AsReadOnly();
        }

        private IReadOnlyCollection<Cliente>
            ObtenerClientesActivos(
                IEnumerable<int> idsClientes)
        {
            if (idsClientes == null)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar al menos un cliente.");
            }

            List<int> ids =
                idsClientes.ToList();

            if (ids.Count == 0)
            {
                throw new ReglaNegocioException(
                    "Debe seleccionar al menos un cliente.");
            }

            if (ids.Any(
                idCliente =>
                    idCliente <= 0))
            {
                throw new ReglaNegocioException(
                    "Los clientes seleccionados no son válidos.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ReglaNegocioException(
                    "No se permiten clientes duplicados.");
            }

            IReadOnlyCollection<Cliente> clientes =
                _clienteRepository.ObtenerPorIds(
                    ids.AsReadOnly());

            if (clientes == null ||
                clientes.Count != ids.Count)
            {
                throw new ReglaNegocioException(
                    "Uno o más clientes seleccionados no existen.");
            }

            Dictionary<int, Cliente> porId =
                clientes.ToDictionary(
                    cliente =>
                        cliente.IdCliente);

            var ordenados =
                new List<Cliente>();

            foreach (
                int idCliente
                in ids)
            {
                Cliente cliente;

                if (!porId.TryGetValue(
                    idCliente,
                    out cliente))
                {
                    throw new ReglaNegocioException(
                        "Uno o más clientes seleccionados no existen.");
                }

                if (!cliente.Activo)
                {
                    throw new ReglaNegocioException(
                        "Uno o más clientes seleccionados se encuentran inactivos.");
                }

                ordenados.Add(
                    cliente);
            }

            return ordenados.AsReadOnly();
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

        private static void ValidarIdCliente(
            int idCliente)
        {
            if (idCliente <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idCliente),
                    "El identificador del cliente debe ser mayor que cero.");
            }
        }
    }
}