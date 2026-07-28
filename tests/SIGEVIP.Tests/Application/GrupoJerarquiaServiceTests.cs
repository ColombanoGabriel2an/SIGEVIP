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
    public class GrupoJerarquiaServiceTests
    {
        [TestMethod]
        public void ListarGruposHijos_SinSesion_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            GrupoGestionService servicio =
                CrearServicio(
                    repository,
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.ListarGruposHijos(
                    10,
                    new int[0]));
        }

        [TestMethod]
        public void ListarGruposHijos_ConDatosValidos_PropagaSeleccion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "GRUPO_HIJO"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            GrupoSeleccionGrupoDto resultado =
                servicio.ListarGruposHijos(
                        10,
                        new[]
                        {
                            2
                        })
                    .Single(
                        grupo =>
                            grupo.IdGrupo == 2);

            Assert.IsTrue(
                resultado.Seleccionado);

            Assert.AreEqual(
                10,
                repository.UltimoIdGrupoPadreListado);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    2
                },
                repository
                    .UltimosIdsSeleccionados
                    .ToArray());
        }

        [TestMethod]
        public void Registrar_ConGrupoHijoValido_AsignaYPersisteJerarquia()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "GRUPO_HIJO"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            int idGrupo =
                servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo padre",
                        "Descripción del grupo padre",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            2
                        }));

            Assert.AreEqual(
                25,
                idGrupo);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    2
                },
                repository
                    .IdsGruposInsertados
                    .ToArray());

            Assert.AreEqual(
                2,
                repository
                    .GrupoInsertado
                    .Componentes
                    .OfType<Grupo>()
                    .Single()
                    .IdGrupo);
        }

        [TestMethod]
        public void Registrar_ConGrupoHijoInexistente_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo padre",
                        "Descripción del grupo padre",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            999
                        })));
        }

        [TestMethod]
        public void Registrar_ConGrupoHijoInactivo_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            Grupo grupoInactivo =
                CrearGrupo(
                    2,
                    "GRUPO_INACTIVO");

            grupoInactivo.Desactivar();

            repository.GruposDisponibles.Add(
                grupoInactivo);

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo padre",
                        "Descripción del grupo padre",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            2
                        })));
        }

        [TestMethod]
        public void Registrar_ConGruposHijosDuplicados_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "GRUPO_HIJO"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarGrupoCommand(
                        "Grupo padre",
                        "Descripción del grupo padre",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            2,
                            2
                        })));
        }

        [TestMethod]
        public void Modificar_ConNuevoConstructor_ReemplazaJerarquia()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            Grupo padre =
                CrearGrupo(
                    10,
                    "GRUPO_PADRE");

            padre.AgregarComponente(
                CrearGrupo(
                    3,
                    "HIJO_ANTERIOR"));

            repository.GrupoObtenido =
                padre;

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "HIJO_NUEVO"));

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    3,
                    "HIJO_ANTERIOR"));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarGrupoCommand(
                    10,
                    "Grupo padre modificado",
                    "Descripción modificada",
                    new[]
                    {
                        1
                    },
                    new[]
                    {
                        2
                    }));

            Assert.IsTrue(
                repository
                    .ActualizacionJerarquiaInvocada);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    2
                },
                repository
                    .IdsGruposActualizados
                    .ToArray());

            Assert.AreEqual(
                2,
                repository
                    .GrupoActualizado
                    .Componentes
                    .OfType<Grupo>()
                    .Single()
                    .IdGrupo);
        }

        [TestMethod]
        public void Modificar_ConConstructorAnterior_PreservaJerarquia()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            Grupo padre =
                CrearGrupo(
                    10,
                    "GRUPO_PADRE");

            padre.AgregarComponente(
                CrearGrupo(
                    3,
                    "HIJO_EXISTENTE"));

            repository.GrupoObtenido =
                padre;

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarGrupoCommand(
                    10,
                    "Grupo padre modificado",
                    "Descripción modificada",
                    new[]
                    {
                        1
                    }));

            Assert.IsTrue(
                repository
                    .ActualizacionAnteriorInvocada);

            Assert.IsFalse(
                repository
                    .ActualizacionJerarquiaInvocada);

            Assert.AreEqual(
                3,
                repository
                    .GrupoActualizado
                    .Componentes
                    .OfType<Grupo>()
                    .Single()
                    .IdGrupo);
        }

        [TestMethod]
        public void Modificar_ConAutorreferencia_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "GRUPO_PADRE");

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarGrupoCommand(
                        10,
                        "Grupo padre",
                        "Descripción válida",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            10
                        })));
        }

        [TestMethod]
        public void Modificar_ConHijoInactivoYaAsignado_PermiteConservar()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            Grupo hijo =
                CrearGrupo(
                    2,
                    "HIJO_INACTIVO");

            hijo.Desactivar();

            Grupo padre =
                CrearGrupo(
                    10,
                    "GRUPO_PADRE");

            padre.AgregarComponente(
                hijo);

            repository.GrupoObtenido =
                padre;

            repository.GruposDisponibles.Add(
                hijo);

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarGrupoCommand(
                    10,
                    "Grupo padre modificado",
                    "Descripción modificada",
                    new[]
                    {
                        1
                    },
                    new[]
                    {
                        2
                    }));

            Assert.IsTrue(
                repository
                    .ActualizacionJerarquiaInvocada);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    2
                },
                repository
                    .IdsGruposActualizados
                    .ToArray());
        }

        [TestMethod]
        public void Modificar_ConHijoInactivoNuevo_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.GrupoObtenido =
                CrearGrupo(
                    10,
                    "GRUPO_PADRE");

            Grupo hijo =
                CrearGrupo(
                    2,
                    "HIJO_INACTIVO");

            hijo.Desactivar();

            repository.GruposDisponibles.Add(
                hijo);

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarGrupoCommand(
                        10,
                        "Grupo padre modificado",
                        "Descripción modificada",
                        new[]
                        {
                            1
                        },
                        new[]
                        {
                            2
                        })));

            Assert.IsFalse(
                repository
                    .ActualizacionJerarquiaInvocada);
        }

        [TestMethod]
        public void ObtenerVistaPreviaPermisosEfectivos_SinSesion_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            GrupoGestionService servicio =
                CrearServicio(
                    repository,
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () =>
                    servicio
                        .ObtenerVistaPreviaPermisosEfectivos(
                            new[]
                            {
                                1
                            },
                            new int[0]));
        }

        [TestMethod]
        public void ObtenerVistaPreviaPermisosEfectivos_ConDuplicados_RechazaOperacion()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () =>
                    servicio
                        .ObtenerVistaPreviaPermisosEfectivos(
                            new[]
                            {
                                1,
                                1
                            },
                            new int[0]));
        }

        [TestMethod]
        public void ObtenerVistaPreviaPermisosEfectivos_ConSeleccionValida_DelegaYDevuelveResultado()
        {
            GrupoJerarquiaRepositoryFalso repository =
                CrearRepositoryBase();

            repository.VistaPreviaResultado.Add(
                new PermisoEfectivoGrupoDto(
                    1,
                    "CLIENTE_CONSULTAR",
                    "Consultar clientes",
                    "Consulta de clientes",
                    true));

            repository.VistaPreviaResultado.Add(
                new PermisoEfectivoGrupoDto(
                    2,
                    "VISITA_REGISTRAR",
                    "Registrar visitas",
                    "Registro de visitas",
                    false));

            GrupoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            IReadOnlyCollection<PermisoEfectivoGrupoDto>
                resultado =
                    servicio
                        .ObtenerVistaPreviaPermisosEfectivos(
                            new[]
                            {
                                1
                            },
                            new[]
                            {
                                20
                            });

            Assert.AreEqual(
                2,
                resultado.Count);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    1
                },
                repository
                    .UltimosIdsPermisosVistaPrevia
                    .ToArray());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    20
                },
                repository
                    .UltimosIdsGruposVistaPrevia
                    .ToArray());

            Assert.IsTrue(
                resultado.Single(
                    permiso =>
                        permiso.IdPermiso == 1)
                    .Directo);

            Assert.IsFalse(
                resultado.Single(
                    permiso =>
                        permiso.IdPermiso == 2)
                    .Directo);
        }

        private static GrupoJerarquiaRepositoryFalso
            CrearRepositoryBase()
        {
            GrupoJerarquiaRepositoryFalso repository =
                new GrupoJerarquiaRepositoryFalso();

            repository.PermisosDisponibles.Add(
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR"));

            return repository;
        }

        private static GrupoGestionService
            CrearServicioAutorizado(
                GrupoJerarquiaRepositoryFalso repository)
        {
            return CrearServicio(
                repository,
                CrearUsuarioAutorizado());
        }

        private static GrupoGestionService CrearServicio(
            GrupoJerarquiaRepositoryFalso repository,
            Usuario usuario)
        {
            return new GrupoGestionService(
                repository,
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static Usuario CrearUsuarioAutorizado()
        {
            Usuario usuario =
                new Usuario(
                    99,
                    99,
                    "administrador.jerarquia",
                    new byte[]
                    {
                        1,
                        2,
                        3
                    },
                    new byte[]
                    {
                        4,
                        5,
                        6
                    },
                    100000);

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

        private sealed class GrupoJerarquiaRepositoryFalso
            : IGrupoGestionRepository
        {
            public GrupoJerarquiaRepositoryFalso()
            {
                PermisosDisponibles =
                    new List<Permiso>();

                GruposDisponibles =
                    new List<Grupo>();

                VistaPreviaResultado =
                    new List<PermisoEfectivoGrupoDto>();

                IdInsertado = 25;
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

            public List<PermisoEfectivoGrupoDto>
                VistaPreviaResultado
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                UltimosIdsPermisosVistaPrevia
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                UltimosIdsGruposVistaPrevia
            {
                get;
                private set;
            }

            public Grupo GrupoObtenido
            {
                get;
                set;
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

            public int IdInsertado
            {
                get;
                set;
            }

            public int? UltimoIdGrupoPadreListado
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int>
                UltimosIdsSeleccionados
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

            public bool ActualizacionAnteriorInvocada
            {
                get;
                private set;
            }

            public bool ActualizacionJerarquiaInvocada
            {
                get;
                private set;
            }

            public IReadOnlyCollection<GrupoListadoDto> Listar(
                GrupoFiltro filtro)
            {
                return new List<GrupoListadoDto>()
                    .AsReadOnly();
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
                            permiso.Activo
                            || seleccionados.Contains(
                                permiso.IdPermiso))
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
                UltimoIdGrupoPadreListado =
                    idGrupoPadre;

                UltimosIdsSeleccionados =
                    (
                        idsSeleccionados
                        ?? new int[0]
                    )
                    .ToList()
                    .AsReadOnly();

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

            public IReadOnlyCollection<PermisoEfectivoGrupoDto>
                ObtenerPermisosEfectivosVistaPrevia(
                    IReadOnlyCollection<int> idsPermisosDirectos,
                    IReadOnlyCollection<int> idsGruposHijos)
            {
                UltimosIdsPermisosVistaPrevia =
                    idsPermisosDirectos
                        .ToList()
                        .AsReadOnly();

                UltimosIdsGruposVistaPrevia =
                    idsGruposHijos
                        .ToList()
                        .AsReadOnly();

                return VistaPreviaResultado
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

            public bool ExisteCodigo(
                string codigo,
                int? idGrupoExcluido)
            {
                return false;
            }

            public bool ExisteNombre(
                string nombre,
                int? idGrupoExcluido)
            {
                return false;
            }

            public bool ExistePermisoActivo(
                string codigoPermiso)
            {
                return false;
            }

            public int Insertar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                AuditoriaRegistro auditoria)
            {
                GrupoInsertado =
                    grupo;

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

                AuditoriaActualizacion =
                    auditoria;

                ActualizacionAnteriorInvocada =
                    true;
            }

            public void Actualizar(
                Grupo grupo,
                IReadOnlyCollection<int> idsPermisos,
                IReadOnlyCollection<int> idsGruposHijos,
                AuditoriaRegistro auditoria)
            {
                GrupoActualizado =
                    grupo;

                IdsGruposActualizados =
                    idsGruposHijos
                        .ToList()
                        .AsReadOnly();

                AuditoriaActualizacion =
                    auditoria;

                ActualizacionJerarquiaInvocada =
                    true;
            }

            public void Activar(
                int idGrupo,
                AuditoriaRegistro auditoria)
            {
            }

            public void Desactivar(
                int idGrupo,
                AuditoriaRegistro auditoria)
            {
            }
        }
    }
}
