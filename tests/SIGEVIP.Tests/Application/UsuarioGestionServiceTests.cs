using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class UsuarioGestionServiceTests
    {
        [TestMethod]
        public void Listar_SinSesion_RechazaOperacion()
        {
            UsuarioGestionService servicio =
                CrearServicio(
                    new UsuarioGestionRepositoryFalso(),
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConUsuarioInactivo_RechazaOperacion()
        {
            Usuario usuarioActual =
                CrearUsuarioAutorizado();

            usuarioActual.Desactivar();

            UsuarioGestionService servicio =
                CrearServicio(
                    new UsuarioGestionRepositoryFalso(),
                    usuarioActual);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_SinPermisoGestionar_RechazaOperacion()
        {
            UsuarioGestionService servicio =
                CrearServicio(
                    new UsuarioGestionRepositoryFalso(),
                    CrearUsuario(
                        99,
                        "operador"));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConPermisoGestionar_DevuelveResultados()
        {
            UsuarioGestionRepositoryFalso repository =
                new UsuarioGestionRepositoryFalso();

            repository.Resultados.Add(
                new UsuarioListadoDto(
                    10,
                    "usuario.prueba",
                    "Persona Prueba",
                    "persona@sigevip.test",
                    "COMERCIAL",
                    true));

            UsuarioGestionService servicio =
                CrearServicio(
                    repository,
                    CrearUsuarioAutorizado());

            IReadOnlyCollection<UsuarioListadoDto> resultado =
                servicio.Listar(null);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.IsNotNull(
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void Registrar_ConPersonaInexistente_RechazaOperacion()
        {
            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    new UsuarioGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConPersonaInactiva_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.PersonaObtenida.Desactivar();

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConPersonaQueYaTieneUsuario_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.PersonaConUsuario = true;

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConNombreUsuarioVacio_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            RegistrarUsuarioCommand command =
                new RegistrarUsuarioCommand(
                    20,
                    " ",
                    "Clave123",
                    "Clave123",
                    new[] { 1 });

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    command));
        }

        [TestMethod]
        public void Registrar_ConNombreUsuarioDuplicado_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.NombreUsuarioExistente = true;

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConPasswordCorta_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            RegistrarUsuarioCommand command =
                new RegistrarUsuarioCommand(
                    20,
                    "nuevo.usuario",
                    "Clave1",
                    "Clave1",
                    new[] { 1 });

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    command));
        }

        [TestMethod]
        public void Registrar_ConConfirmacionDiferente_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            RegistrarUsuarioCommand command =
                new RegistrarUsuarioCommand(
                    20,
                    "nuevo.usuario",
                    "Clave123",
                    "Otra123",
                    new[] { 1 });

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    command));
        }

        [TestMethod]
        public void Registrar_SinGrupos_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            RegistrarUsuarioCommand command =
                new RegistrarUsuarioCommand(
                    20,
                    "nuevo.usuario",
                    "Clave123",
                    "Clave123",
                    new int[0]);

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    command));
        }

        [TestMethod]
        public void Registrar_ConGrupoInexistente_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.GruposDisponibles.Clear();

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConGrupoInactivo_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.GruposDisponibles[0].Desactivar();

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_CreaUsuarioActivo()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            PasswordHasherFalso hasher =
                new PasswordHasherFalso();

            UsuarioGestionService servicio =
                CrearServicio(
                    repository,
                    CrearUsuarioAutorizado(),
                    hasher);

            int idUsuario =
                servicio.Registrar(
                    CrearComandoRegistro());

            Assert.AreEqual(
                25,
                idUsuario);

            Assert.IsNotNull(
                repository.UsuarioInsertado);

            Assert.IsTrue(
                repository.UsuarioInsertado.Activo);

            Assert.AreEqual(
                "nuevo.usuario",
                repository.UsuarioInsertado.NombreUsuario);

            Assert.IsTrue(
                hasher.CrearHashFueInvocado);

            Assert.AreEqual(
                "Clave123",
                hasher.PasswordRecibido);
        }

        [TestMethod]
        public void Registrar_ConVariosGrupos_AsignaTodos()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "GERENTE"));

            RegistrarUsuarioCommand command =
                new RegistrarUsuarioCommand(
                    20,
                    "nuevo.usuario",
                    "Clave123",
                    "Clave123",
                    new[] { 1, 2 });

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Registrar(
                command);

            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                repository.IdsGruposInsertados.ToArray());

            Assert.AreEqual(
                2,
                repository.UsuarioInsertado.Grupos.Count);
        }

        [TestMethod]
        public void Modificar_UsuarioInexistente_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void Modificar_ConMismoNombre_ExcluyeUsuarioActual()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.actual");

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarUsuarioCommand(
                    10,
                    " USUARIO.ACTUAL ",
                    new[] { 1 }));

            Assert.AreEqual(
                10,
                repository.UltimoIdUsuarioExcluido);

            Assert.AreEqual(
                "usuario.actual",
                repository.UsuarioActualizado.NombreUsuario);
        }

        [TestMethod]
        public void Modificar_ConNombreDeOtroUsuario_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.actual");

            repository.NombreUsuarioExistente = true;

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void Modificar_ConGruposValidos_ReemplazaAsignaciones()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.actual");

            repository.UsuarioObtenido.AgregarGrupo(
                CrearGrupo(
                    3,
                    "COMERCIAL"));

            repository.GruposDisponibles.Clear();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    1,
                    "ADMINISTRATIVO"));

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    2,
                    "GERENTE"));

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarUsuarioCommand(
                    10,
                    "usuario.modificado",
                    new[] { 1, 2 }));

            Assert.AreEqual(
                "usuario.modificado",
                repository.UsuarioActualizado.NombreUsuario);

            CollectionAssert.AreEquivalent(
                new[] { 1, 2 },
                repository.IdsGruposActualizados.ToArray());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "ADMINISTRATIVO",
                    "GERENTE"
                },
                repository.UsuarioActualizado
                    .Grupos
                    .Select(
                        grupo => grupo.Codigo)
                    .ToArray());
        }

        [TestMethod]
        public void Modificar_SinGrupos_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.actual");

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarUsuarioCommand(
                        10,
                        "usuario.modificado",
                        new int[0])));
        }

        [TestMethod]
        public void Modificar_UsuarioAutenticado_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            Usuario usuarioActual =
                CrearUsuarioAutorizado(
                    99);

            UsuarioGestionService servicio =
                CrearServicio(
                    repository,
                    usuarioActual);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarUsuarioCommand(
                        99,
                        "usuario.actual",
                        new[] { 1 })));
        }

        [TestMethod]
        public void Modificar_UltimoAdministradorNoPuedePerderGrupo()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "administrador");

            repository.UsuarioObtenido.AgregarGrupo(
                CrearGrupo(
                    4,
                    UsuarioGestionService
                        .CodigoGrupoAdministrador));

            repository.GruposDisponibles.Clear();

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            repository.ExisteOtroAdministrador = false;

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarUsuarioCommand(
                        10,
                        "administrador",
                        new[] { 1 })));
        }

        [TestMethod]
        public void Desactivar_UsuarioExistente_CambiaEstado()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.prueba");

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Desactivar(
                10);

            Assert.IsFalse(
                repository.UsuarioObtenido.Activo);

            Assert.AreEqual(
                10,
                repository.IdUsuarioDesactivado);
        }

        [TestMethod]
        public void Activar_UsuarioInactivo_CambiaEstado()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "usuario.prueba");

            repository.UsuarioObtenido.Desactivar();

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Activar(
                10);

            Assert.IsTrue(
                repository.UsuarioObtenido.Activo);

            Assert.AreEqual(
                10,
                repository.IdUsuarioActivado);
        }

        [TestMethod]
        public void Desactivar_UsuarioAutenticado_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            Usuario usuarioActual =
                CrearUsuarioAutorizado(
                    99);

            UsuarioGestionService servicio =
                CrearServicio(
                    repository,
                    usuarioActual);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Desactivar(
                    99));
        }

        [TestMethod]
        public void Desactivar_UltimoAdministradorActivo_RechazaOperacion()
        {
            UsuarioGestionRepositoryFalso repository =
                CrearRepositoryConDatosValidos();

            repository.UsuarioObtenido =
                CrearUsuario(
                    10,
                    "administrador");

            repository.UsuarioObtenido.AgregarGrupo(
                CrearGrupo(
                    4,
                    UsuarioGestionService
                        .CodigoGrupoAdministrador));

            repository.ExisteOtroAdministrador = false;

            UsuarioGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Desactivar(
                    10));
        }

        private static UsuarioGestionRepositoryFalso
            CrearRepositoryConDatosValidos()
        {
            UsuarioGestionRepositoryFalso repository =
                new UsuarioGestionRepositoryFalso();

            repository.PersonaObtenida =
                new Persona(
                    20,
                    "Persona",
                    "Prueba",
                    "persona@sigevip.test");

            repository.GruposDisponibles.Add(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            return repository;
        }

        private static UsuarioGestionService
            CrearServicioAutorizado(
                UsuarioGestionRepositoryFalso repository)
        {
            return CrearServicio(
                repository,
                CrearUsuarioAutorizado());
        }

        private static UsuarioGestionService CrearServicio(
            UsuarioGestionRepositoryFalso repository,
            Usuario usuarioActual,
            PasswordHasherFalso hasher = null)
        {
            return new UsuarioGestionService(
                repository,
                hasher
                    ?? new PasswordHasherFalso(),
                new SesionActualFalsa(
                    usuarioActual),
                new AutorizacionService());
        }

        private static RegistrarUsuarioCommand
            CrearComandoRegistro()
        {
            return new RegistrarUsuarioCommand(
                20,
                " NUEVO.USUARIO ",
                "Clave123",
                "Clave123",
                new[] { 1 });
        }

        private static ModificarUsuarioCommand
            CrearComandoModificacion()
        {
            return new ModificarUsuarioCommand(
                10,
                "usuario.modificado",
                new[] { 1 });
        }

        private static Usuario CrearUsuarioAutorizado(
            int idUsuario = 99)
        {
            Usuario usuario =
                CrearUsuario(
                    idUsuario,
                    "administrador.prueba");

            Grupo grupo =
                CrearGrupo(
                    900,
                    "GRUPO_GESTION_USUARIOS");

            grupo.AgregarComponente(
                new Permiso(
                    901,
                    UsuarioGestionService.PermisoGestionar,
                    "Gestionar usuarios",
                    string.Empty));

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
                string.Empty);
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

        private sealed class PasswordHasherFalso
            : IPasswordHasher
        {
            public PasswordHasherFalso()
            {
                Resultado =
                    new PasswordHashResult(
                        new byte[] { 10, 20, 30 },
                        new byte[] { 40, 50, 60 },
                        100000);
            }

            public bool CrearHashFueInvocado
            {
                get;
                private set;
            }

            public string PasswordRecibido
            {
                get;
                private set;
            }

            public PasswordHashResult Resultado
            {
                get;
                private set;
            }

            public PasswordHashResult CrearHash(
                string password)
            {
                CrearHashFueInvocado = true;
                PasswordRecibido = password;

                return Resultado;
            }

            public bool Verificar(
                string password,
                byte[] hashEsperado,
                byte[] salt,
                int iteraciones)
            {
                return false;
            }
        }

        private sealed class UsuarioGestionRepositoryFalso
            : IUsuarioGestionRepository
        {
            public UsuarioGestionRepositoryFalso()
            {
                Resultados =
                    new List<UsuarioListadoDto>();

                PersonasDisponibles =
                    new List<PersonaSeleccionUsuarioDto>();

                GruposDisponibles =
                    new List<Grupo>();

                IdInsertado = 25;
                ExisteOtroAdministrador = true;
            }

            public List<UsuarioListadoDto> Resultados
            {
                get;
                private set;
            }

            public List<PersonaSeleccionUsuarioDto>
                PersonasDisponibles
            {
                get;
                private set;
            }

            public List<Grupo> GruposDisponibles
            {
                get;
                private set;
            }

            public Persona PersonaObtenida
            {
                get;
                set;
            }

            public Usuario UsuarioObtenido
            {
                get;
                set;
            }

            public bool NombreUsuarioExistente
            {
                get;
                set;
            }

            public bool PersonaConUsuario
            {
                get;
                set;
            }

            public bool ExisteOtroAdministrador
            {
                get;
                set;
            }

            public int IdInsertado
            {
                get;
                set;
            }

            public UsuarioFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public int? UltimoIdUsuarioExcluido
            {
                get;
                private set;
            }

            public Usuario UsuarioInsertado
            {
                get;
                private set;
            }

            public Usuario UsuarioActualizado
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

            public int? IdUsuarioActivado
            {
                get;
                private set;
            }

            public int? IdUsuarioDesactivado
            {
                get;
                private set;
            }

            public IReadOnlyCollection<UsuarioListadoDto> Listar(
                UsuarioFiltro filtro)
            {
                UltimoFiltro =
                    filtro;

                return Resultados.AsReadOnly();
            }

            public UsuarioDetalleDto ObtenerDetallePorId(
                int idUsuario)
            {
                return null;
            }

            public Usuario ObtenerPorId(
                int idUsuario)
            {
                if (UsuarioObtenido == null ||
                    UsuarioObtenido.IdUsuario != idUsuario)
                {
                    return null;
                }

                return UsuarioObtenido;
            }

            public Persona ObtenerPersonaPorId(
                int idPersona)
            {
                if (PersonaObtenida == null ||
                    PersonaObtenida.IdPersona != idPersona)
                {
                    return null;
                }

                return PersonaObtenida;
            }

            public IReadOnlyCollection<PersonaSeleccionUsuarioDto>
                ListarPersonasDisponibles()
            {
                return PersonasDisponibles.AsReadOnly();
            }

            public IReadOnlyCollection<GrupoSeleccionUsuarioDto>
                ListarGruposActivos(
                    IReadOnlyCollection<int> idsSeleccionados)
            {
                HashSet<int> seleccionados =
                    new HashSet<int>(
                        idsSeleccionados
                        ?? new int[0]);

                return GruposDisponibles
                    .Where(
                        grupo => grupo.Activo)
                    .Select(
                        grupo =>
                            new GrupoSeleccionUsuarioDto(
                                grupo.IdGrupo,
                                grupo.Codigo,
                                grupo.Nombre,
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

            public bool ExisteNombreUsuario(
                string nombreUsuario,
                int? idUsuarioExcluido)
            {
                UltimoIdUsuarioExcluido =
                    idUsuarioExcluido;

                return NombreUsuarioExistente;
            }

            public bool PersonaTieneUsuario(
                int idPersona)
            {
                return PersonaConUsuario;
            }

            public bool ExisteOtroAdministradorActivo(
                int idUsuarioExcluido)
            {
                return ExisteOtroAdministrador;
            }

            public int Insertar(
                Usuario usuario,
                IReadOnlyCollection<int> idsGrupos)
            {
                UsuarioInsertado =
                    usuario;

                IdsGruposInsertados =
                    idsGrupos.ToList().AsReadOnly();

                return IdInsertado;
            }

            public void Actualizar(
                Usuario usuario,
                IReadOnlyCollection<int> idsGrupos)
            {
                UsuarioActualizado =
                    usuario;

                IdsGruposActualizados =
                    idsGrupos.ToList().AsReadOnly();
            }

            public void Activar(
                int idUsuario)
            {
                IdUsuarioActivado =
                    idUsuario;
            }

            public void Desactivar(
                int idUsuario)
            {
                IdUsuarioDesactivado =
                    idUsuario;
            }
        }
    }
}
