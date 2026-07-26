using System;
using System.Collections.Generic;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Clientes
{
    public sealed class ClienteService
    {
        public const string PermisoConsultar =
            "CLIENTE_CONSULTAR";

        public const string PermisoGestionar =
            "CLIENTE_GESTIONAR";

        private readonly IClienteRepository
            _clienteRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public ClienteService(
            IClienteRepository clienteRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _clienteRepository =
                clienteRepository
                ?? throw new ArgumentNullException(
                    nameof(clienteRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<ClienteListadoDto> Listar(
            ClienteFiltro filtro)
        {
            ExigirPermiso(
                PermisoConsultar);

            return _clienteRepository.Listar(
                filtro
                ?? ClienteFiltro.CrearSinFiltros());
        }

        public Cliente Obtener(
            int idCliente)
        {
            ExigirPermiso(
                PermisoConsultar);

            ValidarIdCliente(idCliente);

            Cliente cliente =
                _clienteRepository.ObtenerPorId(
                    idCliente);

            if (cliente == null)
            {
                throw new ReglaNegocioException(
                    "El cliente indicado no existe.");
            }

            return cliente;
        }

        public int Registrar(
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia)
        {
            ExigirPermiso(
                PermisoGestionar);

            Cliente cliente =
                new Cliente(
                    0,
                    razonSocial,
                    cuit,
                    email,
                    telefono,
                    localidad,
                    provincia);

            if (_clienteRepository.ExisteCuit(
                cliente.Cuit,
                null))
            {
                throw new ReglaNegocioException(
                    "Ya existe un cliente con el CUIT indicado.");
            }

            return _clienteRepository.Insertar(
                cliente);
        }

        public void Modificar(
            int idCliente,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string localidad,
            string provincia)
        {
            ExigirPermiso(
                PermisoGestionar);

            ValidarIdCliente(idCliente);

            Cliente clienteActual =
                _clienteRepository.ObtenerPorId(
                    idCliente);

            if (clienteActual == null)
            {
                throw new ReglaNegocioException(
                    "El cliente indicado no existe.");
            }

            Cliente datosValidados =
                new Cliente(
                    idCliente,
                    razonSocial,
                    cuit,
                    email,
                    telefono,
                    localidad,
                    provincia);

            if (_clienteRepository.ExisteCuit(
                datosValidados.Cuit,
                idCliente))
            {
                throw new ReglaNegocioException(
                    "Ya existe otro cliente con el CUIT indicado.");
            }

            clienteActual.ActualizarDatos(
                datosValidados.RazonSocial,
                datosValidados.Cuit,
                datosValidados.Email,
                datosValidados.Telefono,
                datosValidados.Localidad,
                datosValidados.Provincia);

            _clienteRepository.Actualizar(
                clienteActual);
        }

        public void Activar(
            int idCliente)
        {
            ExigirPermiso(
                PermisoGestionar);

            ValidarIdCliente(idCliente);

            Cliente cliente =
                ObtenerClienteGestionable(
                    idCliente);

            cliente.Activar();

            _clienteRepository.Activar(
                idCliente);
        }

        public void Desactivar(
            int idCliente)
        {
            ExigirPermiso(
                PermisoGestionar);

            ValidarIdCliente(idCliente);

            Cliente cliente =
                ObtenerClienteGestionable(
                    idCliente);

            cliente.Desactivar();

            _clienteRepository.Desactivar(
                idCliente);
        }

        private Cliente ObtenerClienteGestionable(
            int idCliente)
        {
            Cliente cliente =
                _clienteRepository.ObtenerPorId(
                    idCliente);

            if (cliente == null)
            {
                throw new ReglaNegocioException(
                    "El cliente indicado no existe.");
            }

            return cliente;
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
