using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionService
    {
        public const string PermisoEnviar =
            "VIAJE_ENVIAR_RENDICION";

        public const string PermisoRevisar =
            "RENDICION_REVISAR";

        public const string PermisoExcluirViatico =
            "RENDICION_EXCLUIR_VIATICO";

        public const string PermisoReactivarViatico =
            "RENDICION_REACTIVAR_VIATICO";

        public const string PermisoAjustarAnticipo =
            "RENDICION_AJUSTAR_ANTICIPO";

        public const string PermisoAprobar =
            "RENDICION_APROBAR";

        public const string PermisoCancelar =
            "RENDICION_CANCELAR";

        private const string ModuloAuditoria =
            "Rendiciones";

        private const string EntidadViajeAuditoria =
            "Viaje";

        private const string EntidadViaticoAuditoria =
            "Viatico";

        private readonly IRendicionRepository
            _rendicionRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public RendicionService(
            IRendicionRepository rendicionRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _rendicionRepository =
                rendicionRepository
                ?? throw new ArgumentNullException(
                    nameof(rendicionRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<RendicionListadoDto>
            ListarPendientes()
        {
            ExigirPermiso(
                PermisoRevisar);

            IReadOnlyCollection<RendicionListadoDto>
                resultados =
                    _rendicionRepository
                        .ListarPendientes();

            return resultados
                   ?? new List<RendicionListadoDto>()
                       .AsReadOnly();
        }

        public RendicionDetalleDto ObtenerDetalle(
            int idViaje)
        {
            ExigirPermiso(
                PermisoRevisar);

            Viaje viaje =
                ObtenerViaje(
                    idViaje);

            return CrearDetalle(
                viaje);
        }

        public void Enviar(
            EnviarRendicionCommand command)
        {
            ExigirPermiso(
                PermisoEnviar);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            viaje.EnviarARendicion(
                ObtenerIdUsuarioActual(),
                DateTime.Now);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "EnvioARendicion",
                    EntidadViajeAuditoria,
                    viaje.IdViaje,
                    "Se envió el viaje a rendición.");

            _rendicionRepository.Enviar(
                viaje,
                auditoria);
        }

        public void ExcluirViatico(
            ExcluirViaticoCommand command)
        {
            ExigirPermiso(
                PermisoExcluirViatico);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            Viatico viatico =
                ObtenerViaticoDelViaje(
                    viaje,
                    command.IdViatico);

            viaje.ExcluirViatico(
                viatico,
                command.Motivo,
                ObtenerIdUsuarioActual(),
                DateTime.Now);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Exclusion",
                    EntidadViaticoAuditoria,
                    viatico.IdViatico,
                    "Se excluyó un viático de la rendición.");

            _rendicionRepository
                .ExcluirViatico(
                    viaje,
                    viatico,
                    auditoria);
        }

        public void ReactivarViatico(
            ReactivarViaticoCommand command)
        {
            ExigirPermiso(
                PermisoReactivarViatico);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            Viatico viatico =
                ObtenerViaticoDelViaje(
                    viaje,
                    command.IdViatico);

            viaje.ReactivarViatico(
                viatico,
                ObtenerIdUsuarioActual(),
                DateTime.Now);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Reactivacion",
                    EntidadViaticoAuditoria,
                    viatico.IdViatico,
                    "Se reactivó un viático de la rendición.");

            _rendicionRepository
                .ReactivarViatico(
                    viaje,
                    viatico,
                    auditoria);
        }

        public void AjustarAnticipo(
            AjustarAnticipoCommand command)
        {
            ExigirPermiso(
                PermisoAjustarAnticipo);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            viaje.AjustarMontoAnticipado(
                command.MontoAnticipado);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "AjusteAnticipo",
                    EntidadViajeAuditoria,
                    viaje.IdViaje,
                    "Se ajustó el anticipo del viaje en rendición.");

            _rendicionRepository
                .AjustarMontoAnticipado(
                    viaje,
                    auditoria);
        }

        public void Aprobar(
            AprobarRendicionCommand command)
        {
            ExigirPermiso(
                PermisoAprobar);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            viaje.Aprobar(
                ObtenerIdUsuarioActual(),
                DateTime.Now);

            _rendicionRepository.Aprobar(
                viaje);
        }

        public void Cancelar(
            CancelarRendicionCommand command)
        {
            ExigirPermiso(
                PermisoCancelar);

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            Viaje viaje =
                ObtenerViaje(
                    command.IdViaje);

            viaje.Cancelar(
                command.Motivo,
                ObtenerIdUsuarioActual(),
                DateTime.Now);

            _rendicionRepository.Cancelar(
                viaje);
        }

        private AuditoriaRegistro CrearAuditoria(
            string accion,
            string entidad,
            int idEntidad,
            string descripcion)
        {
            Usuario usuarioActual =
                _sesionActual.UsuarioActual;

            return new AuditoriaRegistro(
                usuarioActual.IdUsuario,
                usuarioActual.NombreUsuario,
                ModuloAuditoria,
                accion,
                entidad,
                idEntidad,
                descripcion);
        }

        private Viaje ObtenerViaje(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }

            Viaje viaje =
                _rendicionRepository
                    .ObtenerPorId(
                        idViaje);

            if (viaje == null)
            {
                throw new ReglaNegocioException(
                    "El viaje indicado no existe.");
            }

            return viaje;
        }

        private static Viatico ObtenerViaticoDelViaje(
            Viaje viaje,
            int idViatico)
        {
            Viatico viatico =
                viaje.Viaticos
                    .FirstOrDefault(
                        item =>
                            item.IdViatico ==
                            idViatico);

            if (viatico == null)
            {
                throw new ReglaNegocioException(
                    "El viático indicado no pertenece al viaje.");
            }

            return viatico;
        }

        private int ObtenerIdUsuarioActual()
        {
            int idUsuario =
                _sesionActual
                    .UsuarioActual
                    .IdUsuario;

            if (idUsuario <= 0)
            {
                throw new AccesoDenegadoException(
                    "El usuario autenticado no posee un identificador válido.");
            }

            return idUsuario;
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

            if (!_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigoPermiso))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private static RendicionDetalleDto CrearDetalle(
            Viaje viaje)
        {
            IReadOnlyCollection<RendicionParticipanteDto>
                participantes =
                    viaje.Participantes
                        .Select(
                            persona =>
                                new RendicionParticipanteDto(
                                    persona.IdPersona,
                                    persona.Nombre +
                                    " " +
                                    persona.Apellido,
                                    persona.Email,
                                    persona.Activo))
                        .ToList()
                        .AsReadOnly();

            IReadOnlyCollection<RendicionVisitaDto>
                visitas =
                    viaje.Visitas
                        .Select(
                            CrearVisitaDto)
                        .ToList()
                        .AsReadOnly();

            IReadOnlyCollection<RendicionViaticoDto>
                viaticos =
                    viaje.Viaticos
                        .Select(
                            CrearViaticoDto)
                        .ToList()
                        .AsReadOnly();

            return new RendicionDetalleDto(
                viaje.IdViaje,
                viaje.FechaInicio,
                viaje.FechaFin,
                viaje.Descripcion,
                viaje.TipoViaje,
                viaje.EstadoActual,
                viaje.MontoAnticipado,
                viaje.TotalGastado,
                viaje.SaldoPendiente,
                participantes,
                visitas,
                viaticos,
                viaje.IdUsuarioEnvioRendicion,
                viaje.FechaEnvioRendicion);
        }

        private static RendicionVisitaDto CrearVisitaDto(
            Visita visita)
        {
            IReadOnlyCollection<RendicionClienteDto>
                clientes =
                    visita.Clientes
                        .Select(
                            cliente =>
                                new RendicionClienteDto(
                                    cliente.IdCliente,
                                    cliente.RazonSocial,
                                    cliente.Cuit,
                                    cliente.Localidad,
                                    cliente.Provincia,
                                    cliente.Activo))
                        .ToList()
                        .AsReadOnly();

            return new RendicionVisitaDto(
                visita.IdVisita,
                visita.Fecha,
                visita.Observacion,
                visita.LocalidadEncuentro,
                clientes);
        }

        private static RendicionViaticoDto CrearViaticoDto(
            Viatico viatico)
        {
            string pagadoPor =
                viatico.PagadoPor == null
                    ? string.Empty
                    : viatico.PagadoPor.Nombre +
                      " " +
                      viatico.PagadoPor.Apellido;

            return new RendicionViaticoDto(
                viatico.IdViatico,
                viatico.Fecha,
                viatico.Categoria,
                viatico.MetodoPago,
                pagadoPor,
                viatico.Monto,
                viatico.Descripcion,
                viatico.Estado,
                CrearComprobanteDto(
                    viatico.Comprobante),
                viatico.MotivoExclusion,
                viatico.IdUsuarioExclusion,
                viatico.FechaExclusion,
                viatico.IdUsuarioReactivacion,
                viatico.FechaReactivacion);
        }

        private static RendicionComprobanteDto
            CrearComprobanteDto(
                Comprobante comprobante)
        {
            if (comprobante == null)
            {
                return null;
            }

            return new RendicionComprobanteDto(
                comprobante.IdComprobante,
                comprobante.Tipo,
                comprobante.CuitProveedor,
                comprobante.RazonSocialProveedor,
                comprobante.SituacionFiscal,
                comprobante.Sucursal,
                comprobante.Numero,
                comprobante.MontoGravado,
                comprobante.MontoImpuestos,
                comprobante.Total);
        }
    }
}