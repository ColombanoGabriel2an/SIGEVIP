using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Grupos;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class GrupoGestionServiceTests
    {
        [TestMethod]
        public void Listar_SinSesion_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicio(
                    new GrupoGestionRepositoryFalso(),
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConUsuarioInactivo_RechazaOperacion()
        {
            Usuario usuario =
                CrearUsuarioAutorizado();

            usuario.Desactivar();

            GrupoGestionService servicio =
                CrearServicio(
                    new GrupoGestionRepositoryFalso(),
                    usuario);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_SinPermisoGestionar_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicio(
                    new GrupoGestionRepositoryFalso(),
                    CrearUsuario(
                        99,
                        "operador"));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConPermisoGestionar_DevuelveResultados()
        {
            GrupoGestionRepositoryFalso repository =
                new GrupoGestionRepositoryFalso();

            repository.Resultados.Add(
                new GrupoListadoDto(
                    1,
                    "COMERCIAL",
                    "Comercial",
                    "Grupo comercial",
                    4,
                    2,
                    true));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            IReadOnlyCollection<GrupoListadoDto> resultado =
                servicio.Listar(null);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.IsNotNull(
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void Registrar_ConNombreVacio_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    CrearRepositoryValido());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        " ",
                        "Descripción válida",
                        new[] { 1 })));
        }

        [TestMethod]
        public void Registrar_ConDescripcionVacia_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    CrearRepositoryValido());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo nuevo",
                        " ",
                        new[] { 1 })));
        }

        [TestMethod]
        public void Registrar_ConCodigoDuplicado_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.CodigoExistente = true;

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConNombreDuplicado_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.NombreExistente = true;

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_SinPermisos_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    CrearRepositoryValido());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo nuevo",
                        "Descripción válida",
                        new int[0])));
        }

        [TestMethod]
        public void Registrar_ConPermisoInexistente_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.PermisosDisponibles.Clear();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConPermisoInactivo_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.PermisosDisponibles[0]
                .Desactivar();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_CreaGrupoActivo()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            int idGrupo =
                servicio.Registrar(
                    CrearComandoRegistro());

            Assert.AreEqual(
                25,
                idGrupo);

            Assert.IsNotNull(
                repository.GrupoInsertado);

            Assert.IsTrue(
                repository.GrupoInsertado.Activo);

            Assert.AreEqual(
                "GRUPO_DEMOSTRACION",
                repository.GrupoInsertado.Codigo);

            Assert.AreEqual(
                "Grupo demostración",
                repository.GrupoInsertado.Nombre);

            Assert.IsNotNull(
                repository.AuditoriaInsertada);

            Assert.AreEqual(
                "Seguridad",
                repository.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                repository.AuditoriaInsertada.Accion);

            Assert.AreEqual(
                "Grupo",
                repository.AuditoriaInsertada.Entidad);

            Assert.IsNull(
                repository.AuditoriaInsertada.IdEntidad);
        }

        [TestMethod]
        public void Registrar_ConVariosPermisos_AsignaTodos()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    2,
                    "VIAJE_CONSULTAR"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Registrar(
                new RegistrarGrupoCommand(
                    "Grupo demostración",
                    "Descripción válida",
                    new[] { 1, 2 }));

            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                repository
                    .IdsPermisosInsertados
                    .ToArray());

            Assert.AreEqual(
                2,
                repository
                    .GrupoInsertado
                    .Componentes
                    .OfType<Permiso>()
                    .Count());
        }

        [TestMethod]
        public void GenerarCodigo_NormalizaAcentosYSeparadores()
        {
            string codigo =
                GrupoGestionService.GenerarCodigo(
                    "  Gestión   comercial / región norte  ");

            Assert.AreEqual(
                "GESTION_COMERCIAL_REGION_NORTE",
                codigo);
        }

        [TestMethod]
        public void Modificar_GrupoInexistente_RechazaOperacion()
        {
            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    CrearRepositoryValido());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void Modificar_ConDatosValidos_ConservaCodigo()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "CODIGO_ESTABLE");

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                CrearComandoModificacion());

            Assert.AreEqual(
                "CODIGO_ESTABLE",
                repository
                    .GrupoActualizado
                    .Codigo);
        }

        [TestMethod]
        public void Modificar_ConDatosValidos_ActualizaNombreYDescripcion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "CODIGO_ESTABLE");

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                CrearComandoModificacion());

            Assert.AreEqual(
                "Grupo modificado",
                repository
                    .GrupoActualizado
                    .Nombre);

            Assert.AreEqual(
                "Descripción modificada",
                repository
                    .GrupoActualizado
                    .Descripcion);

            Assert.IsNotNull(
                repository.AuditoriaActualizacion);

            Assert.AreEqual(
                "Modificacion",
                repository
                    .AuditoriaActualizacion
                    .Accion);

            Assert.AreEqual(
                10,
                repository
                    .AuditoriaActualizacion
                    .IdEntidad);
        }

        [TestMethod]
        public void Modificar_ConPermisosValidos_ReemplazaPermisosDirectos()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    2,
                    "VIAJE_CONSULTAR"));

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "CODIGO_ESTABLE");

            repository.GrupoObtenido.AgregarComponente(
                CrearPermiso(
                    3,
                    "CLIENTE_GESTIONAR"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarGrupoCommand(
                    10,
                    "Grupo modificado",
                    "Descripción modificada",
                    new[] { 1, 2 }));

            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                repository
                    .IdsPermisosActualizados
                    .ToArray());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR"
                },
                repository
                    .GrupoActualizado
                    .Componentes
                    .OfType<Permiso>()
                    .Select(
                        permiso => permiso.Codigo)
                    .ToArray());
        }

        [TestMethod]
        public void Modificar_ConNombreDuplicado_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "CODIGO_ESTABLE");

            repository.NombreExistente = true;

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));

            Assert.AreEqual(
                10,
                repository
                    .UltimoIdGrupoExcluidoNombre);
        }

        [TestMethod]
        public void Modificar_AdministradorSinGrupoGestionar_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    4,
                    GrupoGestionService
                        .CodigoAdministradorGeneral);

            repository.PermisosDisponibles.Clear();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    2,
                    GrupoGestionService
                        .PermisoUsuarioGestionar));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarGrupoCommand(
                        4,
                        "Administrador General",
                        "Grupo administrador",
                        new[] { 2 })));
        }

        [TestMethod]
        public void Modificar_AdministradorSinUsuarioGestionar_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    4,
                    GrupoGestionService
                        .CodigoAdministradorGeneral);

            repository.PermisosDisponibles.Clear();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    1,
                    GrupoGestionService
                        .PermisoGrupoGestionar));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarGrupoCommand(
                        4,
                        "Administrador General",
                        "Grupo administrador",
                        new[] { 1 })));
        }

        [TestMethod]
        public void Modificar_AdministradorSinPermisoGestionarActivo_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    4,
                    GrupoGestionService
                        .CodigoAdministradorGeneral);

            repository.PermisosDisponibles.Clear();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    1,
                    GrupoGestionService
                        .PermisoGrupoGestionar));

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    2,
                    GrupoGestionService
                        .PermisoUsuarioGestionar));

            repository.PermisoCatalogoActivo = true;

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarGrupoCommand(
                        4,
                        "Administrador General",
                        "Grupo administrador",
                        new[] { 1, 2 })));
        }

        [TestMethod]
        public void Desactivar_GrupoExistente_CambiaEstado()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "COMERCIAL_TEMPORAL");

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Desactivar(
                10);

            Assert.IsFalse(
                repository
                    .GrupoObtenido
                    .Activo);

            Assert.AreEqual(
                10,
                repository.IdGrupoDesactivado);

            Assert.IsNotNull(
                repository.AuditoriaDesactivacion);

            Assert.AreEqual(
                "Desactivacion",
                repository
                    .AuditoriaDesactivacion
                    .Accion);
        }

        [TestMethod]
        public void Activar_GrupoInactivo_CambiaEstado()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "COMERCIAL_TEMPORAL");

            repository.GrupoObtenido
                .Desactivar();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Activar(
                10);

            Assert.IsTrue(
                repository
                    .GrupoObtenido
                    .Activo);

            Assert.AreEqual(
                10,
                repository.IdGrupoActivado);

            Assert.IsNotNull(
                repository.AuditoriaActivacion);

            Assert.AreEqual(
                "Activacion",
                repository
                    .AuditoriaActivacion
                    .Accion);
        }

        [TestMethod]
        public void Desactivar_AdministradorGeneral_RechazaOperacion()
        {
            GrupoGestionRepositoryFalso repository =
                CrearRepositoryValido();

            repository.GrupoObtenido =
                CrearGrupo(
                    4,
                    GrupoGestionService
                        .CodigoAdministradorGeneral);

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Desactivar(
                    4));

            Assert.IsNull(
                repository.IdGrupoDesactivado);
        }

        private static GrupoGestionRepositoryFalso
            CrearRepositoryValido()
        {
            GrupoGestionRepositoryFalso repository =
                new GrupoGestionRepositoryFalso();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR"));

            return repository;
        }

        private static GrupoGestionService
            CrearServicioAutorizado(
                GrupoGestionRepositoryFalso repository)
        {
            return CrearServicio(
                repository,
                CrearUsuarioAutorizado());
        }

        private static GrupoGestionService CrearServicio(
            GrupoGestionRepositoryFalso repository,
            Usuario usuario)
        {
            return new GrupoGestionService(
                repository,
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static RegistrarGrupoCommand
            CrearComandoRegistro()
        {
            return new RegistrarGrupoCommand(
                " Grupo demostración ",
                " Descripción válida ",
                new[] { 1 });
        }

        private static ModificarGrupoCommand
            CrearComandoModificacion()
        {
            return new ModificarGrupoCommand(
                10,
                " Grupo modificado ",
                " Descripción modificada ",
                new[] { 1 });
        }

        private static Usuario CrearUsuarioAutorizado()
        {
            Usuario usuario =
                CrearUsuario(
                    99,
                    "administrador.prueba");

            Grupo grupo =
                CrearGrupo(
                    900,
                    "GESTORES_DE_GRUPOS");

            grupo.AgregarComponente(
                CrearPermiso(
                    901,
                    GrupoGestionService
                        .PermisoGestionar));

            usuario.AgregarGrupo(
                grupo);

            return usuario;
        }

        private static Usuario CrearUsuario(
            int idUsuario,
            string nombreUsuario)
        {
            return new Usuario(
                idUsuario,
                idUsuario,
                nombreUsuario,
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                100000);
        }

        private static Grupo CrearGrupo(
            int idGrupo,
            string codigo)
        {
            return new Grupo(
                idGrupo,
                codigo,
                "Grupo " + codigo,
                "Descripción " + codigo);
        }

        private static Permiso CrearPermiso(
            int idPermiso,
            string codigo)
        {
            return new Permiso(
                idPermiso,
                codigo,
                "Permiso " + codigo,
                "Descripción " + codigo);
        }

        private sealed class SesionActualFalsa
            : ISesionActual
        {
            public SesionActualFalsa(
                Usuario usuario)
            {
                UsuarioActual =
                    usuario;
            }

            public bool HayUsuarioAutenticado
            {
                get
                {
                    return UsuarioActual != null;
                }
            }

            public Usuario UsuarioActual
            {
                get;
                private set;
            }

            public void Iniciar(
                Usuario usuario)
            {
                UsuarioActual =
                    usuario;
            }

            public void Cerrar()
            {
                UsuarioActual =
                    null;
            }
        }

        private sealed class GrupoGestionRepositoryFalso
            : IGrupoGestionRepository
        {
            public GrupoGestionRepositoryFalso()
            {
                Resultados =
                    new List<GrupoListadoDto>();

                PermisosDisponibles =
                    new List<Permiso>();

                GruposDisponibles =
                    new List<Grupo>();

                IdInsertado = 25;
            }

            public List<GrupoListadoDto> Resultados
            {
                get;
                private set;
            }

            public List<Permiso> PermisosDisponibles
            {
                get;
                private set;
            }

            public List<Grupo> GruposDisponibles
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                IdsGruposInsertados
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                IdsGruposActualizados
            {
                get;
                private set;
            }

            public Grupo GrupoObtenido
            {
                get;
                set;
            }

            public bool CodigoExistente
            {
                get;
                set;
            }

            public bool NombreExistente
            {
                get;
                set;
            }

            public bool PermisoCatalogoActivo
            {
                get;
                set;
            }

            public int IdInsertado
            {
                get;
                set;
            }

            public GrupoFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public int? UltimoIdGrupoExcluidoCodigo
            {
                get;
                private set;
            }

            public int? UltimoIdGrupoExcluidoNombre
            {
                get;
                private set;
            }

            public Grupo GrupoInsertado
            {
                get;
                private set;
            }

            public Grupo GrupoActualizado
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaInsertada
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaActualizacion
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaActivacion
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaDesactivacion
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                IdsPermisosInsertados
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                IdsPermisosActualizados
            {
                get;
                private set;
            }

            public int? IdGrupoActivado
            {
                get;
                private set;
            }

            public int? IdGrupoDesactivado
            {
                get;
                private set;
            }

            public IReadOnlyCollection<GrupoListadoDto> Listar(
                GrupoFiltro filtro)
            {
                UltimoFiltro =
                    filtro;

                return Resultados.AsReadOnly();
            }

            public GrupoDetalleDto ObtenerDetallePorId(
                int idGrupo)
            {
                if (GrupoObtenido == null ||
                    GrupoObtenido.IdGrupo !=
                        idGrupo)
                {
                    return null;
                }

                return new GrupoDetalleDto(
                    GrupoObtenido.IdGrupo,
                    GrupoObtenido.Codigo,
                    GrupoObtenido.Nombre,
                    GrupoObtenido.Descripcion,
                    GrupoObtenido.Activo,
                    GrupoObtenido.Componentes
                        .OfType<Permiso>()
                        .Select(
                            permiso =>
                                permiso.IdPermiso),
                    GrupoObtenido.Componentes
                        .OfType<Grupo>()
                        .Select(
                            grupo =>
                                grupo.IdGrupo));
            }

            public Grupo ObtenerPorId(
                int idGrupo)
            {
                if (GrupoObtenido == null ||
                    GrupoObtenido.IdGrupo !=
                        idGrupo)
                {
                    return null;
                }

                return GrupoObtenido;
            }

            public IReadOnlyCollection<PermisoSeleccionGrupoDto>
                ListarPermisosActivos(
                    IReadOnlyCollection<int> idsSeleccionados)
            {
                HashSet<int> seleccionados =
                    new HashSet<int>(
                        idsSeleccionados
                        ?? new int[0]);

                return PermisosDisponibles
                    .Where(
                        permiso =>
                            permiso.Activo)
                    .Select(
                        permiso =>
                            new PermisoSeleccionGrupoDto(
                                permiso.IdPermiso,
                                permiso.Codigo,
                                permiso.Nombre,
                                permiso.Descripcion,
                                seleccionados.Contains(
                                    permiso.IdPermiso)))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<GrupoSeleccionGrupoDto>
                ListarGruposActivos(
                    int? idGrupoPadre,
                    IReadOnlyCollection<int> idsSeleccionados)
            {
                HashSet<int> seleccionados =
                    new HashSet<int>(
                        idsSeleccionados
                        ?? new int[0]);

                return GruposDisponibles
                    .Where(
                        grupo =>
                            (
                                grupo.Activo
                                || seleccionados.Contains(
                                    grupo.IdGrupo)
                            )
                            && (
                                !idGrupoPadre.HasValue
                                || grupo.IdGrupo !=
                                    idGrupoPadre.Value
                            ))
                    .Select(
                        grupo =>
                            new GrupoSeleccionGrupoDto(
                                grupo.IdGrupo,
                                grupo.Codigo,
                                grupo.Nombre,
                                grupo.Descripcion,
                                grupo.Activo,
                                seleccionados.Contains(
                                    grupo.IdGrupo)))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<Grupo>
                ObtenerGruposPorIds(
                    IReadOnlyCollection<int> idsGrupos)
            {
                HashSet<int> ids =
                    new HashSet<int>(
                        idsGrupos);

                return GruposDisponibles
                    .Where(
                        grupo =>
                            ids.Contains(
                                grupo.IdGrupo))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<PermisoEfectivoGrupoDto>
                ObtenerPermisosEfectivosVistaPrevia(
                    IReadOnlyCollection<int> idsPermisosDirectos,
                    IReadOnlyCollection<int> idsGruposHijos)
            {
                HashSet<int> idsDirectos =
                    new HashSet<int>(
                        idsPermisosDirectos
                        ?? new int[0]);

                return PermisosDisponibles
                    .Where(
                        permiso =>
                            idsDirectos.Contains(
                                permiso.IdPermiso))
                    .Select(
                        permiso =>
                            new PermisoEfectivoGrupoDto(
                                permiso.IdPermiso,
                                permiso.Codigo,
                                permiso.Nombre,
                                permiso.Descripcion,
                                true))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<Permiso>
                ObtenerPermisosPorIds(
                    IReadOnlyCollection<int> idsPermisos)
            {
                HashSet<int> ids =
                    new HashSet<int>(
                        idsPermisos);

                return PermisosDisponibles
                    .Where(
                        permiso =>
                            ids.Contains(
                                permiso.IdPermiso))
                    .ToList()
                    .AsReadOnly();
            }

            public bool ExisteCodigo(
                string codigo,
                int? idGrupoExcluido)
            {
                UltimoIdGrupoExcluidoCodigo =
                    idGrupoExcluido;

                return CodigoExistente;
            }

            public bool ExisteNombre(
                string nombre,
                int? idGrupoExcluido)
            {
                UltimoIdGrupoExcluidoNombre =
                    idGrupoExcluido;

                return NombreExistente;
            }

            public bool ExistePermisoActivo(
                string codigoPermiso)
            {
                return PermisoCatalogoActivo;
            }

            public int Insertar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                AuditoriaRegistro auditoria)
            {
                GrupoInsertado =
                    grupo;

                IdsPermisosInsertados =
                    idsPermisos
                        .ToList()
                        .AsReadOnly();

                IdsGruposInsertados =
                    new int[0];

                AuditoriaInsertada =
                    auditoria;

                return IdInsertado;
            }

            public int Insertar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                IReadOnlyCollection<int> idsGruposHijos,
                AuditoriaRegistro auditoria)
            {
                GrupoInsertado =
                    grupo;

                IdsPermisosInsertados =
                    idsPermisos
                        .ToList()
                        .AsReadOnly();

                IdsGruposInsertados =
                    idsGruposHijos
                        .ToList()
                        .AsReadOnly();

                AuditoriaInsertada =
                    auditoria;

                return IdInsertado;
            }

            public void Actualizar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                AuditoriaRegistro auditoria)
            {
                GrupoActualizado =
                    grupo;

                IdsPermisosActualizados =
                    idsPermisos
                        .ToList()
                        .AsReadOnly();

                AuditoriaActualizacion =
                    auditoria;
            }

            public void Actualizar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                IReadOnlyCollection<int> idsGruposHijos,
                AuditoriaRegistro auditoria)
            {
                GrupoActualizado =
                    grupo;

                IdsPermisosActualizados =
                    idsPermisos
                        .ToList()
                        .AsReadOnly();

                IdsGruposActualizados =
                    idsGruposHijos
                        .ToList()
                        .AsReadOnly();

                AuditoriaActualizacion =
                    auditoria;
            }

            public void Activar(
                int idGrupo,
                AuditoriaRegistro auditoria)
            {
                IdGrupoActivado =
                    idGrupo;

                AuditoriaActivacion =
                    auditoria;
            }

            public void Desactivar(
                int idGrupo,
                AuditoriaRegistro auditoria)
            {
                IdGrupoDesactivado =
                    idGrupo;

                AuditoriaDesactivacion =
                    auditoria;
            }
        }
    }
}
