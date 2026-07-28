using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Permisos;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class PermisoGestionServiceTests
    {
        [TestMethod]
        public void Listar_SinSesion_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicio(
                    new PermisoGestionRepositoryFalso(),
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

            PermisoGestionService servicio =
                CrearServicio(
                    new PermisoGestionRepositoryFalso(),
                    usuario);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_SinPermisoGestionar_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicio(
                    new PermisoGestionRepositoryFalso(),
                    CrearUsuario(
                        99,
                        "operador"));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConPermisoGestionar_DevuelveResultados()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            repository.Resultados.Add(
                new PermisoListadoDto(
                    1,
                    "CLIENTE_CONSULTAR",
                    "Consultar clientes",
                    "Permite consultar clientes.",
                    3,
                    true));

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            IReadOnlyCollection<PermisoListadoDto>
                resultado =
                    servicio.Listar(
                        null);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.IsNotNull(
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void Obtener_ConIdInvalido_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.Obtener(
                    0));
        }

        [TestMethod]
        public void Obtener_PermisoInexistente_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Obtener(
                    10));
        }

        [TestMethod]
        public void Obtener_PermisoExistente_DevuelveDetalle()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            repository.PermisoObtenido =
                CrearPermiso(
                    10,
                    "CLIENTE_CONSULTAR");

            repository.CantidadGruposDetalle =
                3;

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            PermisoDetalleDto detalle =
                servicio.Obtener(
                    10);

            Assert.AreEqual(
                10,
                detalle.IdPermiso);

            Assert.AreEqual(
                "CLIENTE_CONSULTAR",
                detalle.Codigo);

            Assert.AreEqual(
                3,
                detalle.CantidadGrupos);
        }

        [TestMethod]
        public void Registrar_CommandNulo_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ArgumentNullException>(
                () => servicio.Registrar(
                    null));
        }

        [TestMethod]
        public void Registrar_ConCodigoVacio_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarPermisoCommand(
                        " ",
                        "Permiso válido",
                        "Descripción")));
        }

        [TestMethod]
        public void Registrar_ConCodigoSinCaracteresValidos_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarPermisoCommand(
                        "---///",
                        "Permiso válido",
                        "Descripción")));
        }

        [TestMethod]
        public void Registrar_ConNombreVacio_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    new RegistrarPermisoCommand(
                        "CLIENTE_EXPORTAR",
                        " ",
                        "Descripción")));
        }

        [TestMethod]
        public void Registrar_ConCodigoDuplicado_RechazaOperacion()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            repository.CodigoExistente =
                true;

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_NormalizaCodigo()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Registrar(
                new RegistrarPermisoCommand(
                    " gestión / clientes - exportar ",
                    "Exportar clientes",
                    "Permite exportar clientes."));

            Assert.AreEqual(
                "GESTION_CLIENTES_EXPORTAR",
                repository
                    .PermisoInsertado
                    .Codigo);
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_NormalizaNombreYDescripcion()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Registrar(
                new RegistrarPermisoCommand(
                    "CLIENTE_EXPORTAR",
                    " Exportar clientes ",
                    " Permite exportar clientes. "));

            Assert.AreEqual(
                "Exportar clientes",
                repository
                    .PermisoInsertado
                    .Nombre);

            Assert.AreEqual(
                "Permite exportar clientes.",
                repository
                    .PermisoInsertado
                    .Descripcion);
        }

        [TestMethod]
        public void Registrar_ConDescripcionNula_UsaTextoVacio()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Registrar(
                new RegistrarPermisoCommand(
                    "CLIENTE_EXPORTAR",
                    "Exportar clientes",
                    null));

            Assert.AreEqual(
                string.Empty,
                repository
                    .PermisoInsertado
                    .Descripcion);
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_CreaPermisoActivo()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            int idPermiso =
                servicio.Registrar(
                    CrearComandoRegistro());

            Assert.AreEqual(
                25,
                idPermiso);

            Assert.IsNotNull(
                repository.PermisoInsertado);

            Assert.IsTrue(
                repository.PermisoInsertado.Activo);

            Assert.IsNotNull(
                repository.AuditoriaInsertada);

            Assert.AreEqual(
                "Seguridad",
                repository.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                repository.AuditoriaInsertada.Accion);

            Assert.AreEqual(
                "Permiso",
                repository.AuditoriaInsertada.Entidad);

            Assert.IsNull(
                repository.AuditoriaInsertada.IdEntidad);
        }

        [TestMethod]
        public void Modificar_CommandNulo_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ArgumentNullException>(
                () => servicio.Modificar(
                    null));
        }

        [TestMethod]
        public void Modificar_ConIdInvalido_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.Modificar(
                    new ModificarPermisoCommand(
                        0,
                        "Nombre",
                        "Descripción")));
        }

        [TestMethod]
        public void Modificar_PermisoInexistente_RechazaOperacion()
        {
            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    new PermisoGestionRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void Modificar_ConNombreVacio_RechazaOperacion()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    new ModificarPermisoCommand(
                        10,
                        " ",
                        "Descripción")));
        }

        [TestMethod]
        public void Modificar_ConDatosValidos_ActualizaNombreYDescripcion()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                CrearComandoModificacion());

            Assert.AreEqual(
                "Permiso modificado",
                repository
                    .PermisoActualizado
                    .Nombre);

            Assert.AreEqual(
                "Descripción modificada",
                repository
                    .PermisoActualizado
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
        public void Modificar_ConDatosValidos_ConservaCodigo()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                CrearComandoModificacion());

            Assert.AreEqual(
                "CODIGO_ESTABLE",
                repository
                    .PermisoActualizado
                    .Codigo);
        }

        [TestMethod]
        public void Modificar_ConDescripcionVacia_UsaTextoVacio()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Modificar(
                new ModificarPermisoCommand(
                    10,
                    "Permiso modificado",
                    " "));

            Assert.AreEqual(
                string.Empty,
                repository
                    .PermisoActualizado
                    .Descripcion);
        }

        [TestMethod]
        public void Activar_PermisoInactivo_CambiaEstado()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            repository.PermisoObtenido
                .Desactivar();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Activar(
                10);

            Assert.IsTrue(
                repository
                    .PermisoObtenido
                    .Activo);

            Assert.AreEqual(
                10,
                repository.IdPermisoActivado);

            Assert.IsNotNull(
                repository.AuditoriaActivacion);

            Assert.AreEqual(
                "Activacion",
                repository
                    .AuditoriaActivacion
                    .Accion);
        }

        [TestMethod]
        public void Desactivar_PermisoNoCritico_CambiaEstado()
        {
            PermisoGestionRepositoryFalso repository =
                CrearRepositoryConPermiso();

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            servicio.Desactivar(
                10);

            Assert.IsFalse(
                repository
                    .PermisoObtenido
                    .Activo);

            Assert.AreEqual(
                10,
                repository.IdPermisoDesactivado);

            Assert.IsNotNull(
                repository.AuditoriaDesactivacion);

            Assert.AreEqual(
                "Desactivacion",
                repository
                    .AuditoriaDesactivacion
                    .Accion);
        }

        [TestMethod]
        public void Desactivar_UsuarioGestionar_RechazaOperacion()
        {
            VerificarPermisoCritico(
                PermisoGestionService
                    .CodigoUsuarioGestionar);
        }

        [TestMethod]
        public void Desactivar_GrupoGestionar_RechazaOperacion()
        {
            VerificarPermisoCritico(
                PermisoGestionService
                    .CodigoGrupoGestionar);
        }

        [TestMethod]
        public void Desactivar_PermisoGestionar_RechazaOperacion()
        {
            VerificarPermisoCritico(
                PermisoGestionService
                    .CodigoPermisoGestionar);
        }

        [TestMethod]
        public void NormalizarCodigo_EliminaAcentosYSeparadores()
        {
            string codigo =
                PermisoGestionService
                    .NormalizarCodigo(
                        " gestión / clientes - región norte ");

            Assert.AreEqual(
                "GESTION_CLIENTES_REGION_NORTE",
                codigo);
        }

        [TestMethod]
        public void NormalizarCodigo_ConMasDeCienCaracteres_RechazaOperacion()
        {
            string codigoLargo =
                new string(
                    'A',
                    101);

            Assert.ThrowsException<ReglaNegocioException>(
                () =>
                    PermisoGestionService
                        .NormalizarCodigo(
                            codigoLargo));
        }

        private static void VerificarPermisoCritico(
            string codigo)
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            repository.PermisoObtenido =
                CrearPermiso(
                    10,
                    codigo);

            PermisoGestionService servicio =
                CrearServicioAutorizado(
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Desactivar(
                    10));

            Assert.IsNull(
                repository.IdPermisoDesactivado);

            Assert.IsTrue(
                repository
                    .PermisoObtenido
                    .Activo);
        }

        private static PermisoGestionRepositoryFalso
            CrearRepositoryConPermiso()
        {
            PermisoGestionRepositoryFalso repository =
                new PermisoGestionRepositoryFalso();

            repository.PermisoObtenido =
                CrearPermiso(
                    10,
                    "CODIGO_ESTABLE");

            return repository;
        }

        private static PermisoGestionService
            CrearServicioAutorizado(
                PermisoGestionRepositoryFalso repository)
        {
            return CrearServicio(
                repository,
                CrearUsuarioAutorizado());
        }

        private static PermisoGestionService CrearServicio(
            PermisoGestionRepositoryFalso repository,
            Usuario usuario)
        {
            return new PermisoGestionService(
                repository,
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static RegistrarPermisoCommand
            CrearComandoRegistro()
        {
            return new RegistrarPermisoCommand(
                " cliente_exportar ",
                " Exportar clientes ",
                " Permite exportar clientes. ");
        }

        private static ModificarPermisoCommand
            CrearComandoModificacion()
        {
            return new ModificarPermisoCommand(
                10,
                " Permiso modificado ",
                " Descripción modificada ");
        }

        private static Usuario CrearUsuarioAutorizado()
        {
            Usuario usuario =
                CrearUsuario(
                    99,
                    "administrador.permisos");

            Grupo grupo =
                CrearGrupo(
                    900,
                    "GESTORES_DE_PERMISOS");

            grupo.AgregarComponente(
                CrearPermiso(
                    901,
                    PermisoGestionService
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

        private sealed class PermisoGestionRepositoryFalso
            : IPermisoGestionRepository
        {
            public PermisoGestionRepositoryFalso()
            {
                Resultados =
                    new List<PermisoListadoDto>();

                IdInsertado =
                    25;
            }

            public List<PermisoListadoDto> Resultados
            {
                get;
                private set;
            }

            public Permiso PermisoObtenido
            {
                get;
                set;
            }

            public bool CodigoExistente
            {
                get;
                set;
            }

            public int IdInsertado
            {
                get;
                set;
            }

            public int CantidadGruposDetalle
            {
                get;
                set;
            }

            public PermisoFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public int? UltimoIdPermisoExcluido
            {
                get;
                private set;
            }

            public Permiso PermisoInsertado
            {
                get;
                private set;
            }

            public Permiso PermisoActualizado
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

            public int? IdPermisoActivado
            {
                get;
                private set;
            }

            public int? IdPermisoDesactivado
            {
                get;
                private set;
            }

            public IReadOnlyCollection<PermisoListadoDto>
                Listar(
                    PermisoFiltro filtro)
            {
                UltimoFiltro =
                    filtro;

                return Resultados.AsReadOnly();
            }

            public PermisoDetalleDto ObtenerDetallePorId(
                int idPermiso)
            {
                if (PermisoObtenido == null ||
                    PermisoObtenido.IdPermiso !=
                        idPermiso)
                {
                    return null;
                }

                return new PermisoDetalleDto(
                    PermisoObtenido.IdPermiso,
                    PermisoObtenido.Codigo,
                    PermisoObtenido.Nombre,
                    PermisoObtenido.Descripcion,
                    CantidadGruposDetalle,
                    PermisoObtenido.Activo);
            }

            public Permiso ObtenerPorId(
                int idPermiso)
            {
                if (PermisoObtenido == null ||
                    PermisoObtenido.IdPermiso !=
                        idPermiso)
                {
                    return null;
                }

                return PermisoObtenido;
            }

            public bool ExisteCodigo(
                string codigo,
                int? idPermisoExcluido)
            {
                UltimoIdPermisoExcluido =
                    idPermisoExcluido;

                return CodigoExistente;
            }

            public int Insertar(
                Permiso permiso,
                AuditoriaRegistro auditoria)
            {
                PermisoInsertado =
                    permiso;

                AuditoriaInsertada =
                    auditoria;

                return IdInsertado;
            }

            public void Actualizar(
                Permiso permiso,
                AuditoriaRegistro auditoria)
            {
                PermisoActualizado =
                    permiso;

                AuditoriaActualizacion =
                    auditoria;
            }

            public void Activar(
                int idPermiso,
                AuditoriaRegistro auditoria)
            {
                IdPermisoActivado =
                    idPermiso;

                AuditoriaActivacion =
                    auditoria;
            }

            public void Desactivar(
                int idPermiso,
                AuditoriaRegistro auditoria)
            {
                IdPermisoDesactivado =
                    idPermiso;

                AuditoriaDesactivacion =
                    auditoria;
            }
        }
    }
}