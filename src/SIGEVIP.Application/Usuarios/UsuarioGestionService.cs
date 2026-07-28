using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Usuarios
{
    public sealed class UsuarioGestionService
    {
        public const string PermisoGestionar =
            "USUARIO_GESTIONAR";

        public const string CodigoGrupoAdministrador =
            "ADMINISTRADOR_GENERAL";

        public const int LongitudMinimaPassword = 8;

        private const string ModuloAuditoria =
            "Seguridad";

        private const string EntidadAuditoria =
            "Usuario";

        private readonly IUsuarioGestionRepository
            _usuarioRepository;

        private readonly IPasswordHasher
            _passwordHasher;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public UsuarioGestionService(
            IUsuarioGestionRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _usuarioRepository =
                usuarioRepository
                ?? throw new ArgumentNullException(
                    nameof(usuarioRepository));

            _passwordHasher =
                passwordHasher
                ?? throw new ArgumentNullException(
                    nameof(passwordHasher));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<UsuarioListadoDto> Listar(
            UsuarioFiltro filtro)
        {
            ExigirPermisoGestionar();

            return _usuarioRepository.Listar(
                filtro
                ?? UsuarioFiltro.CrearSinFiltros());
        }

        public UsuarioDetalleDto Obtener(
            int idUsuario)
        {
            ExigirPermisoGestionar();
            ValidarIdUsuario(idUsuario);

            UsuarioDetalleDto detalle =
                _usuarioRepository.ObtenerDetallePorId(
                    idUsuario);

            if (detalle == null)
            {
                throw new ReglaNegocioException(
                    "El usuario indicado no existe.");
            }

            return detalle;
        }

        public IReadOnlyCollection<PersonaSeleccionUsuarioDto>
            ListarPersonasDisponibles()
        {
            ExigirPermisoGestionar();

            return _usuarioRepository
                .ListarPersonasDisponibles();
        }

        public IReadOnlyCollection<GrupoSeleccionUsuarioDto>
            ListarGrupos(
                IReadOnlyCollection<int> idsSeleccionados)
        {
            ExigirPermisoGestionar();

            return _usuarioRepository.ListarGruposActivos(
                idsSeleccionados
                ?? new List<int>().AsReadOnly());
        }

        public int Registrar(
            RegistrarUsuarioCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            ValidarIdPersona(
                command.IdPersona);

            string nombreUsuario =
                NormalizarNombreUsuario(
                    command.NombreUsuario);

            ValidarNombreUsuario(
                nombreUsuario);

            ValidarPasswordInicial(
                command.Password,
                command.ConfirmacionPassword);

            List<int> idsGrupos =
                ValidarIdsGrupos(
                    command.IdsGrupos);

            Persona persona =
                _usuarioRepository.ObtenerPersonaPorId(
                    command.IdPersona);

            if (persona == null)
            {
                throw new ReglaNegocioException(
                    "La persona indicada no existe.");
            }

            if (!persona.Activo)
            {
                throw new ReglaNegocioException(
                    "La persona indicada se encuentra inactiva.");
            }

            if (_usuarioRepository.PersonaTieneUsuario(
                command.IdPersona))
            {
                throw new ReglaNegocioException(
                    "La persona indicada ya posee un usuario.");
            }

            if (_usuarioRepository.ExisteNombreUsuario(
                nombreUsuario,
                null))
            {
                throw new ReglaNegocioException(
                    "Ya existe un usuario con el nombre indicado.");
            }

            List<Grupo> grupos =
                ObtenerYValidarGrupos(
                    idsGrupos);

            PasswordHashResult passwordHash =
                _passwordHasher.CrearHash(
                    command.Password);

            Usuario usuario =
                new Usuario(
                    0,
                    persona.IdPersona,
                    nombreUsuario,
                    passwordHash.Hash,
                    passwordHash.Salt,
                    passwordHash.Iteraciones);

            usuario.ReemplazarGrupos(
                grupos);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Alta",
                    null,
                    "Se registró el usuario " +
                    usuario.NombreUsuario +
                    ".");

            return _usuarioRepository.Insertar(
                usuario,
                idsGrupos.AsReadOnly(),
                auditoria);
        }

        public void Modificar(
            ModificarUsuarioCommand command)
        {
            ExigirPermisoGestionar();

            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command));
            }

            ValidarIdUsuario(
                command.IdUsuario);

            if (EsUsuarioActual(
                command.IdUsuario))
            {
                throw new ReglaNegocioException(
                    "No puede modificar su propia cuenta durante la sesión actual.");
            }

            Usuario usuario =
                _usuarioRepository.ObtenerPorId(
                    command.IdUsuario);

            if (usuario == null)
            {
                throw new ReglaNegocioException(
                    "El usuario indicado no existe.");
            }

            string nombreUsuario =
                NormalizarNombreUsuario(
                    command.NombreUsuario);

            ValidarNombreUsuario(
                nombreUsuario);

            if (_usuarioRepository.ExisteNombreUsuario(
                nombreUsuario,
                command.IdUsuario))
            {
                throw new ReglaNegocioException(
                    "Ya existe otro usuario con el nombre indicado.");
            }

            List<int> idsGrupos =
                ValidarIdsGrupos(
                    command.IdsGrupos);

            List<Grupo> gruposNuevos =
                ObtenerYValidarGrupos(
                    idsGrupos);

            bool eraAdministrador =
                ContieneGrupoAdministrador(
                    usuario.Grupos);

            bool continuaraComoAdministrador =
                ContieneGrupoAdministrador(
                    gruposNuevos);

            if (eraAdministrador &&
                !continuaraComoAdministrador &&
                !_usuarioRepository
                    .ExisteOtroAdministradorActivo(
                        command.IdUsuario))
            {
                throw new ReglaNegocioException(
                    "No puede retirar el grupo ADMINISTRADOR_GENERAL al último administrador activo.");
            }

            usuario.ActualizarNombreUsuario(
                nombreUsuario);

            usuario.ReemplazarGrupos(
                gruposNuevos);

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Modificacion",
                    usuario.IdUsuario,
                    "Se modificaron el nombre y los grupos del usuario " +
                    usuario.NombreUsuario +
                    ".");

            _usuarioRepository.Actualizar(
                usuario,
                idsGrupos.AsReadOnly(),
                auditoria);
        }

        public void Activar(
            int idUsuario)
        {
            ExigirPermisoGestionar();
            ValidarIdUsuario(idUsuario);

            Usuario usuario =
                ObtenerUsuarioExistente(
                    idUsuario);

            usuario.Activar();

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Activacion",
                    idUsuario,
                    "Se activó el usuario " +
                    usuario.NombreUsuario +
                    ".");

            _usuarioRepository.Activar(
                idUsuario,
                auditoria);
        }

        public void Desactivar(
            int idUsuario)
        {
            ExigirPermisoGestionar();
            ValidarIdUsuario(idUsuario);

            if (EsUsuarioActual(
                idUsuario))
            {
                throw new ReglaNegocioException(
                    "No puede desactivar el usuario actualmente autenticado.");
            }

            Usuario usuario =
                ObtenerUsuarioExistente(
                    idUsuario);

            if (ContieneGrupoAdministrador(
                    usuario.Grupos) &&
                !_usuarioRepository
                    .ExisteOtroAdministradorActivo(
                        idUsuario))
            {
                throw new ReglaNegocioException(
                    "No puede desactivar al último administrador activo.");
            }

            usuario.Desactivar();

            AuditoriaRegistro auditoria =
                CrearAuditoria(
                    "Desactivacion",
                    idUsuario,
                    "Se desactivó el usuario " +
                    usuario.NombreUsuario +
                    ".");

            _usuarioRepository.Desactivar(
                idUsuario,
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

        private Usuario ObtenerUsuarioExistente(
            int idUsuario)
        {
            Usuario usuario =
                _usuarioRepository.ObtenerPorId(
                    idUsuario);

            if (usuario == null)
            {
                throw new ReglaNegocioException(
                    "El usuario indicado no existe.");
            }

            return usuario;
        }

        private List<Grupo> ObtenerYValidarGrupos(
            IReadOnlyCollection<int> idsGrupos)
        {
            IReadOnlyCollection<Grupo> resultado =
                _usuarioRepository.ObtenerGruposPorIds(
                    idsGrupos);

            List<Grupo> grupos =
                resultado == null
                    ? new List<Grupo>()
                    : resultado.ToList();

            if (grupos.Count != idsGrupos.Count)
            {
                throw new ReglaNegocioException(
                    "Uno o más grupos indicados no existen.");
            }

            foreach (Grupo grupo in grupos)
            {
                if (grupo == null)
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos indicados no existen.");
                }

                if (!grupo.Activo)
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos indicados se encuentran inactivos.");
                }
            }

            HashSet<int> idsRecuperados =
                new HashSet<int>(
                    grupos.Select(
                        grupo => grupo.IdGrupo));

            foreach (int idGrupo in idsGrupos)
            {
                if (!idsRecuperados.Contains(
                    idGrupo))
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos indicados no existen.");
                }
            }

            return grupos;
        }

        private void ExigirPermisoGestionar()
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
                PermisoGestionar))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private bool EsUsuarioActual(
            int idUsuario)
        {
            return _sesionActual.UsuarioActual != null &&
                _sesionActual.UsuarioActual.IdUsuario ==
                    idUsuario;
        }

        private static List<int> ValidarIdsGrupos(
            IEnumerable<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un grupo al usuario.");
            }

            List<int> ids =
                idsGrupos.ToList();

            if (ids.Count == 0)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un grupo al usuario.");
            }

            if (ids.Any(
                idGrupo => idGrupo <= 0))
            {
                throw new ReglaNegocioException(
                    "Los grupos seleccionados contienen identificadores inválidos.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ReglaNegocioException(
                    "No se pueden asignar grupos duplicados al usuario.");
            }

            return ids;
        }

        private static void ValidarPasswordInicial(
            string password,
            string confirmacionPassword)
        {
            if (string.IsNullOrWhiteSpace(
                password))
            {
                throw new ReglaNegocioException(
                    "La contraseña inicial es obligatoria.");
            }

            if (password.Length <
                LongitudMinimaPassword)
            {
                throw new ReglaNegocioException(
                    "La contraseña inicial debe contener al menos 8 caracteres.");
            }

            if (!string.Equals(
                password,
                confirmacionPassword,
                StringComparison.Ordinal))
            {
                throw new ReglaNegocioException(
                    "La contraseña y su confirmación no coinciden.");
            }
        }

        private static void ValidarNombreUsuario(
            string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(
                nombreUsuario))
            {
                throw new ReglaNegocioException(
                    "El nombre de usuario es obligatorio.");
            }
        }

        private static void ValidarIdUsuario(
            int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario),
                    "El identificador del usuario debe ser mayor que cero.");
            }
        }

        private static void ValidarIdPersona(
            int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona),
                    "El identificador de la persona debe ser mayor que cero.");
            }
        }

        private static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return string.IsNullOrWhiteSpace(
                nombreUsuario)
                ? string.Empty
                : nombreUsuario
                    .Trim()
                    .ToLowerInvariant();
        }

        private static bool ContieneGrupoAdministrador(
            IEnumerable<Grupo> grupos)
        {
            if (grupos == null)
            {
                return false;
            }

            return grupos.Any(
                grupo =>
                    grupo != null &&
                    string.Equals(
                        grupo.Codigo,
                        CodigoGrupoAdministrador,
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}
