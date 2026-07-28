using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viaticos
{
    public sealed class ViaticoService
    {
        public const string PermisoConsultar =
            "VIATICO_CONSULTAR";

        public const string PermisoRegistrar =
            "VIATICO_REGISTRAR";

        public const string PermisoModificar =
            "VIATICO_MODIFICAR";

        private const string ModuloAuditoria =
            "Viaticos";

        private const string EntidadAuditoria =
            "Viatico";

        private readonly IViaticoRepository
            _viaticoRepository;

        private readonly IViajeRepository
            _viajeRepository;

        private readonly IPersonaConsultaViaticoRepository
            _personaRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public ViaticoService(
            IViaticoRepository viaticoRepository,
            IViajeRepository viajeRepository,
            IPersonaConsultaViaticoRepository personaRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _viaticoRepository =
                viaticoRepository
                ?? throw new ArgumentNullException(
                    nameof(viaticoRepository));

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

        public IReadOnlyCollection<ViaticoListadoDto>
            ListarPorViaje(
                int idViaje,
                ViaticoFiltro filtro)
        {
            ExigirPermiso(
                PermisoConsultar);

            Viaje viaje =
                ObtenerViaje(
                    idViaje);

            IReadOnlyCollection<ViaticoListadoDto>
                resultados =
                    _viaticoRepository
                        .ListarPorViaje(
                            viaje.IdViaje,
                            filtro
                            ?? ViaticoFiltro
                                .CrearSinFiltros());

            return resultados
                   ?? new List<ViaticoListadoDto>()
                       .AsReadOnly();
        }

        public Viatico Obtener(
            int idViatico)
        {
            ExigirPermiso(
                PermisoConsultar);

            ValidarId(
                idViatico,
                nameof(idViatico),
                "El identificador del viático debe ser mayor que cero.");

            Viatico viatico =
                _viaticoRepository
                    .ObtenerPorId(
                        idViatico);

            if (viatico == null)
            {
                throw new ReglaNegocioException(
                    "El viático indicado no existe.");
            }

            return viatico;
        }

        public IReadOnlyCollection<PagadorSeleccionDto>
            ListarPagadoresDisponibles()
        {
            ExigirAlMenosUnPermiso(
                PermisoRegistrar,
                PermisoModificar);

            IReadOnlyCollection<PagadorSeleccionDto>
                resultados =
                    _personaRepository
                        .ListarActivas();

            return resultados
                   ?? new List<PagadorSeleccionDto>()
                       .AsReadOnly();
        }

        public int Registrar(
            RegistrarViaticoCommand command)
        {
            ExigirPermiso(
                PermisoRegistrar);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            Persona pagador =
                ResolverPagadorActivo(
                    command.IdPersonaPagadora);

            Comprobante comprobante =
                CrearComprobante(
                    command.Comprobante);

            var viatico =
                new Viatico(
                    0,
                    command.Fecha,
                    command.Categoria,
                    command.MetodoPago,
                    pagador,
                    command.Monto,
                    command.Descripcion,
                    comprobante);

            viaje.AgregarViatico(
                viatico);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Alta",
                    null,
                    "Se registró un viático y su comprobante asociado.");

            return _viaticoRepository
                .Insertar(
                    viatico,
                    auditoria);
        }

        public void Modificar(
            ModificarViaticoCommand command)
        {
            ExigirPermiso(
                PermisoModificar);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            Viatico viatico =
                viaje.Viaticos
                    .FirstOrDefault(
                        item =>
                            item.IdViatico ==
                            command.IdViatico);

            if (viatico == null)
            {
                throw new ReglaNegocioException(
                    "El viático indicado no pertenece al viaje.");
            }

            Persona pagador =
                ResolverPagadorActivo(
                    command.IdPersonaPagadora);

            Comprobante comprobante =
                CrearComprobante(
                    command.Comprobante);

            viaje.ModificarViatico(
                viatico,
                command.Fecha,
                command.Categoria,
                command.MetodoPago,
                pagador,
                command.Monto,
                command.Descripcion,
                comprobante);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Modificacion",
                    viatico.IdViatico,
                    "Se modificó el viático y su comprobante asociado.");

            _viaticoRepository.Actualizar(
                viatico,
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

        private Viaje ObtenerViaje(
            int idViaje)
        {
            ValidarId(
                idViaje,
                nameof(idViaje),
                "El identificador del viaje debe ser mayor que cero.");

            Viaje viaje =
                _viajeRepository
                    .ObtenerPorId(
                        idViaje);

            if (viaje == null)
            {
                throw new ReglaNegocioException(
                    "El viaje indicado no existe.");
            }

            return viaje;
        }

        private Persona ResolverPagadorActivo(
            int? idPersonaPagadora)
        {
            if (!idPersonaPagadora.HasValue)
            {
                return null;
            }

            Persona persona =
                _personaRepository
                    .ObtenerPorId(
                        idPersonaPagadora.Value);

            if (persona == null)
            {
                throw new ReglaNegocioException(
                    "La persona pagadora indicada no existe.");
            }

            if (!persona.Activo)
            {
                throw new ReglaNegocioException(
                    "La persona pagadora indicada se encuentra inactiva.");
            }

            return persona;
        }

        private static Comprobante CrearComprobante(
            ComprobanteInput input)
        {
            if (input == null)
            {
                return null;
            }

            return new Comprobante(
                input.IdComprobante,
                input.Tipo,
                input.CuitProveedor,
                input.RazonSocialProveedor,
                input.SituacionFiscal,
                input.Sucursal,
                input.Numero,
                input.MontoGravado,
                input.MontoImpuestos);
        }

        private void ExigirPermiso(
            string codigoPermiso)
        {
            ExigirUsuarioValido();

            if (!_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigoPermiso))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private void ExigirAlMenosUnPermiso(
            params string[] codigosPermiso)
        {
            ExigirUsuarioValido();

            bool autorizado =
                codigosPermiso.Any(
                    codigo =>
                        _autorizacionService
                            .TienePermiso(
                                _sesionActual.UsuarioActual,
                                codigo));

            if (!autorizado)
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private void ExigirUsuarioValido()
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
        }

        private static void ValidarId(
            int id,
            string nombreParametro,
            string mensaje)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    mensaje);
            }
        }
    }
}