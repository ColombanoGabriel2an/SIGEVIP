using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Grupos
{
    public sealed class GrupoGestionService
    {
        public const string PermisoGestionar =
            "GRUPO_GESTIONAR";

        public const string CodigoAdministradorGeneral =
            "ADMINISTRADOR_GENERAL";

        public const string PermisoUsuarioGestionar =
            "USUARIO_GESTIONAR";

        public const string PermisoGrupoGestionar =
            "GRUPO_GESTIONAR";

        public const string PermisoCatalogoGestionar =
            "PERMISO_GESTIONAR";

        private const int LongitudMaximaCodigo = 100;
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;

        private readonly IGrupoGestionRepository
            _grupoRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public GrupoGestionService(
            IGrupoGestionRepository grupoRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _grupoRepository =
                grupoRepository
                ?? throw new ArgumentNullException(
                    nameof(grupoRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<GrupoListadoDto> Listar(
            GrupoFiltro filtro)
        {
            ExigirPermisoGestionar();

            return _grupoRepository.Listar(
                filtro
                ?? GrupoFiltro.CrearSinFiltros());
        }

        public GrupoDetalleDto Obtener(
            int idGrupo)
        {
            ExigirPermisoGestionar();
            ValidarIdGrupo(idGrupo);

            GrupoDetalleDto detalle =
                _grupoRepository
                    .ObtenerDetallePorId(
                        idGrupo);

            if (detalle == null)
            {
                throw new ReglaNegocioException(
                    "El grupo indicado no existe.");
            }

            return detalle;
        }

        public IReadOnlyCollection<PermisoSeleccionGrupoDto>
            ListarPermisos(
                IReadOnlyCollection<int> idsSeleccionados)
        {
            ExigirPermisoGestionar();

            return _grupoRepository
                .ListarPermisosActivos(
                    idsSeleccionados
                    ?? new List<int>().AsReadOnly());
        }

        public IReadOnlyCollection<GrupoSeleccionGrupoDto>
            ListarGruposHijos(
                int? idGrupoPadre,
                IReadOnlyCollection<int> idsSeleccionados)
        {
            ExigirPermisoGestionar();

            if (idGrupoPadre.HasValue)
            {
                ValidarIdGrupo(
                    idGrupoPadre.Value);
            }

            return _grupoRepository
                .ListarGruposActivos(
                    idGrupoPadre,
                    idsSeleccionados
                    ?? new List<int>().AsReadOnly());
        }

        public int Registrar(
            RegistrarGrupoCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            string nombre =
                NormalizarTextoObligatorio(
                    command.Nombre,
                    "El nombre del grupo es obligatorio.");

            string descripcion =
                NormalizarTextoObligatorio(
                    command.Descripcion,
                    "La descripción del grupo es obligatoria.");

            ValidarLongitudes(
                nombre,
                descripcion);

            string codigo =
                GenerarCodigo(
                    nombre);

            if (_grupoRepository.ExisteCodigo(
                codigo,
                null))
            {
                throw new ReglaNegocioException(
                    "Ya existe un grupo con el código generado.");
            }

            if (_grupoRepository.ExisteNombre(
                nombre,
                null))
            {
                throw new ReglaNegocioException(
                    "Ya existe un grupo con el nombre indicado.");
            }

            List<int> idsPermisos =
                ValidarIdsPermisos(
                    command.IdsPermisos);

            List<Permiso> permisos =
                ObtenerYValidarPermisos(
                    idsPermisos);

            List<int> idsGruposHijos =
                ValidarIdsGruposHijos(
                    command.IdsGruposHijos,
                    null);

            List<Grupo> gruposHijos =
                ObtenerYValidarGruposHijos(
                    idsGruposHijos,
                    new int[0]);

            Grupo grupo =
                new Grupo(
                    0,
                    codigo,
                    nombre,
                    descripcion);

            grupo.ReemplazarPermisosDirectos(
                permisos);

            grupo.ReemplazarGruposHijos(
                gruposHijos);

            return _grupoRepository.Insertar(
                grupo,
                idsPermisos.AsReadOnly(),
                idsGruposHijos.AsReadOnly());
        }

        public void Modificar(
            ModificarGrupoCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            ValidarIdGrupo(
                command.IdGrupo);

            Grupo grupo =
                ObtenerGrupoExistente(
                    command.IdGrupo);

            string nombre =
                NormalizarTextoObligatorio(
                    command.Nombre,
                    "El nombre del grupo es obligatorio.");

            string descripcion =
                NormalizarTextoObligatorio(
                    command.Descripcion,
                    "La descripción del grupo es obligatoria.");

            ValidarLongitudes(
                nombre,
                descripcion);

            if (_grupoRepository.ExisteNombre(
                nombre,
                command.IdGrupo))
            {
                throw new ReglaNegocioException(
                    "Ya existe otro grupo con el nombre indicado.");
            }

            List<int> idsPermisos =
                ValidarIdsPermisos(
                    command.IdsPermisos);

            List<Permiso> permisos =
                ObtenerYValidarPermisos(
                    idsPermisos);

            if (EsAdministradorGeneral(grupo))
            {
                ValidarPermisosAdministrador(
                    permisos);
            }

            grupo.ActualizarDatos(
                nombre,
                descripcion);

            grupo.ReemplazarPermisosDirectos(
                permisos);

            if (command.ReemplazarGruposHijos)
            {
                List<int> idsGruposHijos =
                    ValidarIdsGruposHijos(
                        command.IdsGruposHijos,
                        command.IdGrupo);

                List<int> idsGruposHijosActuales =
                    grupo.Componentes
                        .OfType<Grupo>()
                        .Select(
                            grupoHijo =>
                                grupoHijo.IdGrupo)
                        .ToList();

                List<Grupo> gruposHijos =
                    ObtenerYValidarGruposHijos(
                        idsGruposHijos,
                        idsGruposHijosActuales);

                grupo.ReemplazarGruposHijos(
                    gruposHijos);

                _grupoRepository.Actualizar(
                    grupo,
                    idsPermisos.AsReadOnly(),
                    idsGruposHijos.AsReadOnly());

                return;
            }

            _grupoRepository.Actualizar(
                grupo,
                idsPermisos.AsReadOnly());
        }

        public void Activar(
            int idGrupo)
        {
            ExigirPermisoGestionar();
            ValidarIdGrupo(idGrupo);

            Grupo grupo =
                ObtenerGrupoExistente(
                    idGrupo);

            grupo.Activar();

            _grupoRepository.Activar(
                idGrupo);
        }

        public void Desactivar(
            int idGrupo)
        {
            ExigirPermisoGestionar();
            ValidarIdGrupo(idGrupo);

            Grupo grupo =
                ObtenerGrupoExistente(
                    idGrupo);

            if (EsAdministradorGeneral(
                grupo))
            {
                throw new ReglaNegocioException(
                    "No puede desactivar el grupo ADMINISTRADOR_GENERAL.");
            }

            grupo.Desactivar();

            _grupoRepository.Desactivar(
                idGrupo);
        }

        public static string GenerarCodigo(
            string nombre)
        {
            string texto =
                NormalizarTextoObligatorio(
                    nombre,
                    "El nombre del grupo es obligatorio.");

            string descompuesto =
                texto.Normalize(
                    NormalizationForm.FormD);

            StringBuilder resultado =
                new StringBuilder();

            bool separadorPendiente = false;

            foreach (char caracter in descompuesto)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(
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
                        resultado.Append('_');
                    }

                    resultado.Append(
                        char.ToUpperInvariant(
                            caracter));

                    separadorPendiente = false;
                }
                else
                {
                    separadorPendiente = true;
                }
            }

            string codigo =
                resultado
                    .ToString()
                    .Trim('_');

            if (string.IsNullOrWhiteSpace(
                codigo))
            {
                throw new ReglaNegocioException(
                    "No fue posible generar un código válido para el grupo.");
            }

            if (codigo.Length >
                LongitudMaximaCodigo)
            {
                throw new ReglaNegocioException(
                    "El código generado para el grupo supera los 100 caracteres.");
            }

            return codigo;
        }

        private Grupo ObtenerGrupoExistente(
            int idGrupo)
        {
            Grupo grupo =
                _grupoRepository.ObtenerPorId(
                    idGrupo);

            if (grupo == null)
            {
                throw new ReglaNegocioException(
                    "El grupo indicado no existe.");
            }

            return grupo;
        }

        private List<Permiso> ObtenerYValidarPermisos(
            IReadOnlyCollection<int> idsPermisos)
        {
            IReadOnlyCollection<Permiso> resultado =
                _grupoRepository
                    .ObtenerPermisosPorIds(
                        idsPermisos);

            List<Permiso> permisos =
                resultado == null
                    ? new List<Permiso>()
                    : resultado.ToList();

            if (permisos.Count !=
                idsPermisos.Count)
            {
                throw new ReglaNegocioException(
                    "Uno o más permisos indicados no existen.");
            }

            foreach (Permiso permiso in permisos)
            {
                if (permiso == null)
                {
                    throw new ReglaNegocioException(
                        "Uno o más permisos indicados no existen.");
                }

                if (!permiso.Activo)
                {
                    throw new ReglaNegocioException(
                        "Uno o más permisos indicados se encuentran inactivos.");
                }
            }

            HashSet<int> idsRecuperados =
                new HashSet<int>(
                    permisos.Select(
                        permiso =>
                            permiso.IdPermiso));

            foreach (int idPermiso in idsPermisos)
            {
                if (!idsRecuperados.Contains(
                    idPermiso))
                {
                    throw new ReglaNegocioException(
                        "Uno o más permisos indicados no existen.");
                }
            }

            return permisos;
        }

        private List<Grupo> ObtenerYValidarGruposHijos(
            IReadOnlyCollection<int> idsGruposHijos,
            IReadOnlyCollection<int> idsGruposHijosActuales)
        {
            if (idsGruposHijos.Count == 0)
            {
                return new List<Grupo>();
            }

            IReadOnlyCollection<Grupo> resultado =
                _grupoRepository
                    .ObtenerGruposPorIds(
                        idsGruposHijos);

            List<Grupo> grupos =
                resultado == null
                    ? new List<Grupo>()
                    : resultado.ToList();

            if (grupos.Count !=
                idsGruposHijos.Count ||
                grupos.Any(
                    grupo =>
                        grupo == null))
            {
                throw new ReglaNegocioException(
                    "Uno o más grupos hijos indicados no existen.");
            }

            HashSet<int> idsRecuperados =
                new HashSet<int>(
                    grupos.Select(
                        grupo =>
                            grupo.IdGrupo));

            foreach (int idGrupo in idsGruposHijos)
            {
                if (!idsRecuperados.Contains(
                    idGrupo))
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos hijos indicados no existen.");
                }
            }

            HashSet<int> idsActuales =
                new HashSet<int>(
                    idsGruposHijosActuales
                    ?? new int[0]);

            foreach (Grupo grupo in grupos)
            {
                if (!grupo.Activo &&
                    !idsActuales.Contains(
                        grupo.IdGrupo))
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos hijos indicados se encuentran inactivos.");
                }
            }

            return grupos;
        }

        private static List<int> ValidarIdsGruposHijos(
            IEnumerable<int> idsGruposHijos,
            int? idGrupoPadre)
        {
            if (idsGruposHijos == null)
            {
                throw new ReglaNegocioException(
                    "La colección de grupos hijos es obligatoria.");
            }

            List<int> ids =
                idsGruposHijos.ToList();

            if (ids.Any(
                idGrupo =>
                    idGrupo <= 0))
            {
                throw new ReglaNegocioException(
                    "Los grupos hijos contienen identificadores inválidos.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ReglaNegocioException(
                    "No se pueden asignar grupos hijos duplicados.");
            }

            if (idGrupoPadre.HasValue &&
                ids.Contains(
                    idGrupoPadre.Value))
            {
                throw new ReglaNegocioException(
                    "Un grupo no puede agregarse a sí mismo.");
            }

            return ids;
        }

        private void ValidarPermisosAdministrador(
            IEnumerable<Permiso> permisos)
        {
            HashSet<string> codigos =
                new HashSet<string>(
                    permisos.Select(
                        permiso =>
                            permiso.Codigo),
                    StringComparer.OrdinalIgnoreCase);

            if (!codigos.Contains(
                PermisoGrupoGestionar))
            {
                throw new ReglaNegocioException(
                    "ADMINISTRADOR_GENERAL debe conservar GRUPO_GESTIONAR.");
            }

            if (!codigos.Contains(
                PermisoUsuarioGestionar))
            {
                throw new ReglaNegocioException(
                    "ADMINISTRADOR_GENERAL debe conservar USUARIO_GESTIONAR.");
            }

            if (_grupoRepository
                    .ExistePermisoActivo(
                        PermisoCatalogoGestionar) &&
                !codigos.Contains(
                    PermisoCatalogoGestionar))
            {
                throw new ReglaNegocioException(
                    "ADMINISTRADOR_GENERAL debe conservar PERMISO_GESTIONAR.");
            }
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

        private static List<int> ValidarIdsPermisos(
            IEnumerable<int> idsPermisos)
        {
            if (idsPermisos == null)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un permiso al grupo.");
            }

            List<int> ids =
                idsPermisos.ToList();

            if (ids.Count == 0)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un permiso al grupo.");
            }

            if (ids.Any(
                idPermiso =>
                    idPermiso <= 0))
            {
                throw new ReglaNegocioException(
                    "Los permisos seleccionados contienen identificadores inválidos.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ReglaNegocioException(
                    "No se pueden asignar permisos duplicados al grupo.");
            }

            return ids;
        }

        private static void ValidarIdGrupo(
            int idGrupo)
        {
            if (idGrupo <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idGrupo),
                    "El identificador del grupo debe ser mayor que cero.");
            }
        }

        private static void ValidarLongitudes(
            string nombre,
            string descripcion)
        {
            if (nombre.Length >
                LongitudMaximaNombre)
            {
                throw new ReglaNegocioException(
                    "El nombre del grupo no puede superar los 150 caracteres.");
            }

            if (descripcion.Length >
                LongitudMaximaDescripcion)
            {
                throw new ReglaNegocioException(
                    "La descripción del grupo no puede superar los 500 caracteres.");
            }
        }

        private static string NormalizarTextoObligatorio(
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

        private static bool EsAdministradorGeneral(
            Grupo grupo)
        {
            return grupo != null &&
                string.Equals(
                    grupo.Codigo,
                    CodigoAdministradorGeneral,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
