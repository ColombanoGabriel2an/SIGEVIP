using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ClienteServiceTests
    {
        [TestMethod]
        public void Listar_SinSesion_RechazaOperacion()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConUsuarioInactivo_RechazaOperacion()
        {
            Usuario usuario =
                CrearUsuario(
                    ClienteService.PermisoConsultar);

            usuario.Desactivar();

            ClienteService servicio =
                CrearServicio(
                    new ClienteRepositoryFalso(),
                    usuario);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_SinPermisoConsultar_RechazaOperacion()
        {
            ClienteService servicio =
                CrearServicio(
                    new ClienteRepositoryFalso(),
                    CrearUsuario());

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(null));
        }

        [TestMethod]
        public void Listar_ConPermisoConsultar_DevuelveResultados()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.Resultados.Add(
                CrearDto());

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoConsultar));

            IReadOnlyCollection<ClienteListadoDto> resultado =
                servicio.Listar(null);

            Assert.AreEqual(
                1,
                resultado.Count);
        }

        [TestMethod]
        public void Listar_TransfiereFiltroAlRepositorio()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            ClienteFiltro filtro =
                new ClienteFiltro(
                    "empresa",
                    "301",
                    "Rosario",
                    "Santa Fe",
                    true);

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoConsultar));

            servicio.Listar(filtro);

            Assert.AreSame(
                filtro,
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void Registrar_SinPermisoGestionar_RechazaOperacion()
        {
            ClienteService servicio =
                CrearServicio(
                    new ClienteRepositoryFalso(),
                    CrearUsuario(
                        ClienteService.PermisoConsultar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => RegistrarCliente(servicio));
        }

        [TestMethod]
        public void Registrar_ConDatosValidos_InsertaClienteActivo()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            int idCliente =
                RegistrarCliente(servicio);

            Assert.AreEqual(
                10,
                idCliente);

            Assert.IsNotNull(
                repository.ClienteInsertado);

            Assert.IsTrue(
                repository.ClienteInsertado.Activo);

            Assert.AreEqual(
                "30123456789",
                repository.ClienteInsertado.Cuit);

            Assert.IsNotNull(
                repository.AuditoriaInsertada);

            Assert.AreEqual(
                "Clientes",
                repository.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                repository.AuditoriaInsertada.Accion);

            Assert.IsNull(
                repository.AuditoriaInsertada.IdEntidad);

            Assert.IsFalse(
                repository.AuditoriaInsertada
                    .Descripcion
                    .Contains(
                        "30123456789"));
        }

        [TestMethod]
        public void Registrar_ConCuitDuplicado_RechazaOperacion()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.CuitExistente = true;

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => RegistrarCliente(servicio));
        }

        [TestMethod]
        public void Modificar_ClienteInexistente_RechazaOperacion()
        {
            ClienteService servicio =
                CrearServicio(
                    new ClienteRepositoryFalso(),
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    50,
                    "Empresa",
                    "30-12345678-9",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
        }

        [TestMethod]
        public void Modificar_MismoCuit_ExcluyeIdActual()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                CrearCliente();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            servicio.Modificar(
                1,
                "Empresa modificada",
                "30-12345678-9",
                "nuevo@correo.com",
                "3415559999",
                "Funes",
                "Santa Fe");

            Assert.AreEqual(
                1,
                repository.UltimoIdExcluido);

            Assert.AreEqual(
                "Empresa modificada",
                repository.ClienteActualizado.RazonSocial);

            Assert.IsNotNull(
                repository.AuditoriaActualizacion);

            Assert.AreEqual(
                "Modificacion",
                repository
                    .AuditoriaActualizacion
                    .Accion);

            Assert.AreEqual(
                1,
                repository
                    .AuditoriaActualizacion
                    .IdEntidad);
        }

        [TestMethod]
        public void Modificar_CuitDeOtroCliente_RechazaOperacion()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                CrearCliente();

            repository.CuitExistente = true;

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    1,
                    "Empresa modificada",
                    "30-99999999-1",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
        }

        [TestMethod]
        public void Desactivar_ClienteExistente_ActualizaEstado()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                CrearCliente();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            servicio.Desactivar(1);

            Assert.IsFalse(
                repository.ClienteObtenido.Activo);

            Assert.AreEqual(
                1,
                repository.IdDesactivado);

            Assert.IsNotNull(
                repository.AuditoriaDesactivacion);

            Assert.AreEqual(
                "Desactivacion",
                repository
                    .AuditoriaDesactivacion
                    .Accion);
        }

        [TestMethod]
        public void Activar_ClienteInactivo_ActualizaEstado()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                CrearCliente();

            repository.ClienteObtenido.Desactivar();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoGestionar));

            servicio.Activar(1);

            Assert.IsTrue(
                repository.ClienteObtenido.Activo);

            Assert.AreEqual(
                1,
                repository.IdActivado);

            Assert.IsNotNull(
                repository.AuditoriaActivacion);

            Assert.AreEqual(
                "Activacion",
                repository
                    .AuditoriaActivacion
                    .Accion);
        }

        [TestMethod]
        public void Obtener_ConIdInvalido_RechazaOperacion()
        {
            ClienteService servicio =
                CrearServicio(
                    new ClienteRepositoryFalso(),
                    CrearUsuario(
                        ClienteService.PermisoConsultar));

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.Obtener(0));
        }

        [TestMethod]
        public void Obtener_ClienteExistente_DevuelveCliente()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                CrearCliente();

            ClienteService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario(
                        ClienteService.PermisoConsultar));

            Cliente resultado =
                servicio.Obtener(1);

            Assert.AreSame(
                repository.ClienteObtenido,
                resultado);
        }

        private static ClienteService CrearServicio(
            ClienteRepositoryFalso repository,
            Usuario usuario)
        {
            ISesionActual sesion =
                new SesionActualFalsa(
                    usuario);

            return new ClienteService(
                repository,
                sesion,
                new AutorizacionService());
        }

        private static Usuario CrearUsuario(
            params string[] permisos)
        {
            Usuario usuario =
                new Usuario(
                    1,
                    1,
                    "usuario.prueba",
                    new byte[] { 1, 2, 3 },
                    new byte[] { 4, 5, 6 },
                    100000);

            Grupo grupo =
                new Grupo(
                    1,
                    "GRUPO_PRUEBA",
                    "Grupo de prueba",
                    string.Empty);

            int idPermiso = 1;

            foreach (string codigo in permisos)
            {
                grupo.AgregarComponente(
                    new Permiso(
                        idPermiso,
                        codigo,
                        codigo,
                        string.Empty));

                idPermiso++;
            }

            usuario.AgregarGrupo(grupo);

            return usuario;
        }

        private static int RegistrarCliente(
            ClienteService servicio)
        {
            return servicio.Registrar(
                "Empresa de prueba",
                "30-12345678-9",
                "empresa@prueba.com",
                "3415550000",
                "Rosario",
                "Santa Fe");
        }

        private static Cliente CrearCliente()
        {
            return new Cliente(
                1,
                "Empresa de prueba",
                "30-12345678-9",
                "empresa@prueba.com",
                "3415550000",
                "Rosario",
                "Santa Fe");
        }

        private static ClienteListadoDto CrearDto()
        {
            return new ClienteListadoDto(
                1,
                "Empresa de prueba",
                "30123456789",
                "empresa@prueba.com",
                "3415550000",
                "Rosario",
                "Santa Fe",
                true);
        }

        private sealed class SesionActualFalsa
            : ISesionActual
        {
            public SesionActualFalsa(
                Usuario usuario)
            {
                UsuarioActual = usuario;
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
                UsuarioActual = usuario;
            }

            public void Cerrar()
            {
                UsuarioActual = null;
            }
        }

        private sealed class ClienteRepositoryFalso
            : IClienteRepository
        {
            public ClienteRepositoryFalso()
            {
                Resultados =
                    new List<ClienteListadoDto>();

                IdInsertado = 10;
            }

            public Cliente ClienteObtenido { get; set; }

            public bool CuitExistente { get; set; }

            public int IdInsertado { get; set; }

            public Cliente ClienteInsertado { get; private set; }

            public Cliente ClienteActualizado { get; private set; }

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

            public int? UltimoIdExcluido { get; private set; }

            public ClienteFiltro UltimoFiltro { get; private set; }

            public int? IdActivado { get; private set; }

            public int? IdDesactivado { get; private set; }

            public List<ClienteListadoDto> Resultados { get; private set; }

            public Cliente ObtenerPorId(
                int idCliente)
            {
                if (ClienteObtenido == null)
                {
                    return null;
                }

                return ClienteObtenido.IdCliente == idCliente
                    ? ClienteObtenido
                    : null;
            }

            public bool ExisteCuit(
                string cuit,
                int? idClienteExcluido)
            {
                UltimoIdExcluido =
                    idClienteExcluido;

                return CuitExistente;
            }

            public IReadOnlyCollection<ClienteListadoDto> Listar(
                ClienteFiltro filtro)
            {
                UltimoFiltro = filtro;

                return Resultados.AsReadOnly();
            }

            public int Insertar(
                Cliente cliente,
                AuditoriaRegistro auditoria)
            {
                ClienteInsertado =
                    cliente;

                AuditoriaInsertada =
                    auditoria;

                return IdInsertado;
            }

            public void Actualizar(
                Cliente cliente,
                AuditoriaRegistro auditoria)
            {
                ClienteActualizado =
                    cliente;

                AuditoriaActualizacion =
                    auditoria;
            }

            public void Activar(
                int idCliente,
                AuditoriaRegistro auditoria)
            {
                IdActivado =
                    idCliente;

                AuditoriaActivacion =
                    auditoria;
            }

            public void Desactivar(
                int idCliente,
                AuditoriaRegistro auditoria)
            {
                IdDesactivado =
                    idCliente;

                AuditoriaDesactivacion =
                    auditoria;
            }
        }
    }
}
