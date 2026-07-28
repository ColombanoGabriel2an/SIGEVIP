using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Permisos
{
    public sealed class PermisoGestionService
    {
        public const string PermisoGestionar =
            "PERMISO_GESTIONAR";

        public const string CodigoUsuarioGestionar =
            "USUARIO_GESTIONAR";

        public const string CodigoGrupoGestionar =
            "GRUPO_GESTIONAR";

        public const string CodigoPermisoGestionar =
            "PERMISO_GESTIONAR";

        private const int LongitudMaximaCodigo = 100;
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;

        private const string ModuloAuditoria =
            "Seguridad";

        private const string EntidadAuditoria =
            "Permiso";

        private readonly IPermisoGestionRepository
            _permisoRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public PermisoGestionService(
            IPermisoGestionRepository permisoRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _permisoRepository =
                permisoRepository
                ?? throw new ArgumentNullException(
                    nameof(permisoRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<PermisoListadoDto>
            Listar(
                PermisoFiltro filtro)
        {
            ExigirPermisoGestionar();

            return _permisoRepository.Listar(
                filtro
                ?? PermisoFiltro.CrearSinFiltros());
        }

        public PermisoDetalleDto Obtener(
            int idPermiso)
        {
            ExigirPermisoGestionar();
            ValidarIdPermiso(
                idPermiso);

            PermisoDetalleDto detalle =
                _permisoRepository
                    .ObtenerDetallePorId(
                        idPermiso);

            if (detalle == null)
            {
                throw new ReglaNegocioException(
                    "El permiso indicado no existe.");
            }

            return detalle;
        }

        public int Registrar(
            RegistrarPermisoCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            string codigo =
                NormalizarCodigo(
                    command.Codigo);

            string nombre =
                NormalizarTextoObligatorio(
                    command.Nombre,
                    "El nombre del permiso es obligatorio.");

            string descripcion =
                NormalizarTextoOpcional(
                    command.Descripcion);

            ValidarLongitudes(
                codigo,
                nombre,
                descripcion);

            if (_permisoRepository.ExisteCodigo(
                codigo,
                null))
            {
                throw new ReglaNegocioException(
                    "Ya existe un permiso con el código indicado.");
            }

            Permiso permiso =
                new Permiso(
                    0,
                    codigo,
                    nombre,
                    descripcion);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Alta",
                    null,
                    "Se registró el permiso " +
                    permiso.Codigo +
                    ".");

            return _permisoRepository.Insertar(
                permiso,
                auditoria);
        }

        public void Modificar(
            ModificarPermisoCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            ValidarIdPermiso(
                command.IdPermiso);

            Permiso permiso =
                ObtenerPermisoExistente(
                    command.IdPermiso);

            string nombre =
                NormalizarTextoObligatorio(
                    command.Nombre,
                    "El nombre del permiso es obligatorio.");

            string descripcion =
                NormalizarTextoOpcional(
                    command.Descripcion);

            ValidarLongitudes(
                permiso.Codigo,
                nombre,
                descripcion);

            permiso.ActualizarDatos(
                nombre,
                descripcion);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Modificacion",
                    permiso.IdPermiso,
                    "Se modificaron el nombre y la descripción del permiso " +
                    permiso.Codigo +
                    ".");

            _permisoRepository.Actualizar(
                permiso,
                auditoria);
        }

        public void Activar(
            int idPermiso)
        {
            ExigirPermisoGestionar();
            ValidarIdPermiso(
                idPermiso);

            Permiso permiso =
                ObtenerPermisoExistente(
                    idPermiso);

            permiso.Activar();

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Activacion",
                    idPermiso,
                    "Se activó el permiso " +
                    permiso.Codigo +
                    ".");

            _permisoRepository.Activar(
                idPermiso,
                auditoria);
        }

        public void Desactivar(
            int idPermiso)
        {
            ExigirPermisoGestionar();
            ValidarIdPermiso(
                idPermiso);

            Permiso permiso =
                ObtenerPermisoExistente(
                    idPermiso);

            if (EsPermisoCritico(
                permiso.Codigo))
            {
                throw new ReglaNegocioException(
                    "No puede desactivar el permiso " +
                    permiso.Codigo +
                    " porque es necesario para administrar la seguridad.");
            }

            permiso.Desactivar();

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Desactivacion",
                    idPermiso,
                    "Se desactivó el permiso " +
                    permiso.Codigo +
                    ".");

            _permisoRepository.Desactivar(
                idPermiso,
                auditoria);
        }

        public static string NormalizarCodigo(
            string codigo)
        {
            string texto =
                NormalizarTextoObligatorio(
                    codigo,
                    "El código del permiso es obligatorio.");

            string descompuesto =
                texto.Normalize(
                    NormalizationForm.FormD);

            StringBuilder resultado =
                new StringBuilder();

            bool separadorPendiente =
                false;

            foreach (
                char caracter
                in descompuesto)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            caracter);

                if (categoria ==
                    UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(
                    caracter))
                {
                    if (separadorPendiente &&
                        resultado.Length > 0)
                    {
                        resultado.Append(
                            '_');
                    }

                    resultado.Append(
                        char.ToUpperInvariant(
                            caracter));

                    separadorPendiente =
                        false;
                }
                else
                {
                    separadorPendiente =
                        true;
                }
            }

            string codigoNormalizado =
                resultado
                    .ToString()
                    .Trim('_');

            if (string.IsNullOrWhiteSpace(
                codigoNormalizado))
            {
                throw new ReglaNegocioException(
                    "El código indicado no contiene caracteres válidos.");
            }

            if (codigoNormalizado.Length >
                LongitudMaximaCodigo)
            {
                throw new ReglaNegocioException(
                    "El código del permiso no puede superar los 100 caracteres.");
            }

            return codigoNormalizado;
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

        private Permiso ObtenerPermisoExistente(
            int idPermiso)
        {
            Permiso permiso =
                _permisoRepository
                    .ObtenerPorId(
                        idPermiso);

            if (permiso == null)
            {
                throw new ReglaNegocioException(
                    "El permiso indicado no existe.");
            }

            return permiso;
        }

        private void ExigirPermisoGestionar()
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new AccesoDenegadoException(
                    "Debe iniciar sesión para realizar esta operación.");
            }

            if (!_sesionActual
                    .UsuarioActual
                    .Activo)
            {
                throw new AccesoDenegadoException(
                    "El usuario autenticado se encuentra inactivo.");
            }

            if (!_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        PermisoGestionar))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private static void ValidarIdPermiso(
            int idPermiso)
        {
            if (idPermiso <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPermiso),
                    "El identificador del permiso debe ser mayor que cero.");
            }
        }

        private static void ValidarLongitudes(
            string codigo,
            string nombre,
            string descripcion)
        {
            if (codigo.Length >
                LongitudMaximaCodigo)
            {
                throw new ReglaNegocioException(
                    "El código del permiso no puede superar los 100 caracteres.");
            }

            if (nombre.Length >
                LongitudMaximaNombre)
            {
                throw new ReglaNegocioException(
                    "El nombre del permiso no puede superar los 150 caracteres.");
            }

            if (descripcion.Length >
                LongitudMaximaDescripcion)
            {
                throw new ReglaNegocioException(
                    "La descripción del permiso no puede superar los 500 caracteres.");
            }
        }

        private static string
            NormalizarTextoObligatorio(
                string valor,
                string mensaje)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                throw new ReglaNegocioException(
                    mensaje);
            }

            return valor.Trim();
        }

        private static string
            NormalizarTextoOpcional(
                string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }

        private static bool EsPermisoCritico(
            string codigo)
        {
            return string.Equals(
                    codigo,
                    CodigoUsuarioGestionar,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    codigo,
                    CodigoGrupoGestionar,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    codigo,
                    CodigoPermisoGestionar,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}