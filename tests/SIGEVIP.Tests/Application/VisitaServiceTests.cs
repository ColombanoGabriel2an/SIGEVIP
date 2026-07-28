using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class VisitaServiceTests
    {
        [TestMethod]
        public void Registrar_SinSesion_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_UsuarioInactivo_RechazaOperacion()
        {
            Usuario usuario =
                CrearUsuario(
                    VisitaService.PermisoRegistrar);

            usuario.Desactivar();

            VisitaService servicio =
                CrearServicio(
                    usuario);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_SinPermiso_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_IdViajeInvalido_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar));

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.Registrar(
                    0,
                    FechaVisita,
                    "Reunión",
                    "Rosario",
                    new[]
                    {
                        1
                    }));
        }

        [TestMethod]
        public void Registrar_ViajeInexistente_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_ViajeNoAbierto_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .EnviarARendicion();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    viajes,
                    CrearClientes());

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_FechaFueraDelViaje_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    viajes,
                    CrearClientes());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    1,
                    new DateTime(
                        2026,
                        7,
                        20),
                    "Reunión",
                    "Rosario",
                    new[]
                    {
                        1
                    }));
        }

        [TestMethod]
        public void Registrar_ClientesNulos_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicioConViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    1,
                    FechaVisita,
                    "Reunión",
                    "Rosario",
                    null));
        }

        [TestMethod]
        public void Registrar_SinClientes_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicioConViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    1,
                    FechaVisita,
                    "Reunión",
                    "Rosario",
                    new int[0]));
        }

        [TestMethod]
        public void Registrar_IdClienteInvalido_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicioConViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    1,
                    FechaVisita,
                    "Reunión",
                    "Rosario",
                    new[]
                    {
                        0
                    }));
        }

        [TestMethod]
        public void Registrar_ClientesDuplicados_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicioConViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    1,
                    FechaVisita,
                    "Reunión",
                    "Rosario",
                    new[]
                    {
                        1,
                        1
                    }));
        }

        [TestMethod]
        public void Registrar_ClienteInexistente_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    viajes,
                    new ClienteRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_ClienteInactivo_RechazaOperacion()
        {
            ClienteRepositoryFalso clientes =
                CrearClientes();

            clientes.Clientes[0]
                .Desactivar();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    clientes);

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_UnCliente_InsertaVisitaAsociada()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViaje();

            VisitaRepositoryFalso visitas =
                new VisitaRepositoryFalso();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    viajes,
                    CrearClientes(),
                    visitas);

            int idVisita =
                Registrar(
                    servicio);

            Assert.AreEqual(
                50,
                idVisita);

            Assert.IsNotNull(
                visitas.Insertada);

            Assert.AreEqual(
                1,
                visitas.Insertada.IdViaje);

            Assert.AreEqual(
                1,
                visitas.Insertada.Clientes.Count);

            Assert.AreEqual(
                1,
                viajes.Obtenido.Visitas.Count);

            Assert.IsNotNull(
                visitas.AuditoriaInsertada);

            Assert.AreEqual(
                1,
                visitas.AuditoriaInsertada.IdUsuario);

            Assert.AreEqual(
                "usuario",
                visitas.AuditoriaInsertada.NombreUsuario);

            Assert.AreEqual(
                "Viajes",
                visitas.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                visitas.AuditoriaInsertada.Accion);

            Assert.AreEqual(
                "Visita",
                visitas.AuditoriaInsertada.Entidad);

            Assert.IsNull(
                visitas.AuditoriaInsertada.IdEntidad);
        }

        [TestMethod]
        public void Registrar_VariosClientes_ConservaOrden()
        {
            ClienteRepositoryFalso clientes =
                CrearClientes();

            clientes.Clientes.Add(
                CrearCliente(
                    2));

            VisitaRepositoryFalso visitas =
                new VisitaRepositoryFalso();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    clientes,
                    visitas);

            servicio.Registrar(
                1,
                FechaVisita,
                "Reunión",
                "Rosario",
                new[]
                {
                    2,
                    1
                });

            int[] ids =
                visitas.Insertada
                    .Clientes
                    .Select(
                        cliente =>
                            cliente.IdCliente)
                    .ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    2,
                    1
                },
                ids);
        }

        [TestMethod]
        public void ListarClientesDisponibles_DevuelveActivos()
        {
            ClienteRepositoryFalso clientes =
                CrearClientes();

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar),
                    new ViajeRepositoryFalso(),
                    clientes);

            IReadOnlyCollection<ClienteSeleccionVisitaDto>
                resultado =
                    servicio
                        .ListarClientesDisponibles();

            Assert.AreEqual(
                1,
                resultado.Count);
        }

        [TestMethod]
        public void ListarPorViaje_SinPermisoConsultar_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.ListarPorViaje(
                    1));
        }

        [TestMethod]
        public void ListarPorViaje_ConPermiso_DevuelveResultados()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViaje();

            VisitaRepositoryFalso visitas =
                new VisitaRepositoryFalso();

            visitas.PorViaje.Add(
                CrearDto());

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    viajes,
                    CrearClientes(),
                    visitas);

            IReadOnlyCollection<VisitaListadoDto>
                resultado =
                    servicio.ListarPorViaje(
                        1);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.AreEqual(
                1,
                visitas.UltimoIdViaje);
        }

        [TestMethod]
        public void ListarPorCliente_SinPermisoConsultar_RechazaOperacion()
        {
            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        VisitaService.PermisoRegistrar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.ListarPorCliente(
                    1));
        }

        [TestMethod]
        public void ListarPorCliente_ConPermiso_DevuelveResultados()
        {
            VisitaRepositoryFalso visitas =
                new VisitaRepositoryFalso();

            visitas.PorCliente.Add(
                CrearDto());

            VisitaService servicio =
                CrearServicio(
                    CrearUsuario(
                        ClienteService.PermisoConsultar),
                    new ViajeRepositoryFalso(),
                    CrearClientes(),
                    visitas);

            IReadOnlyCollection<VisitaListadoDto>
                resultado =
                    servicio.ListarPorCliente(
                        1);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.AreEqual(
                1,
                visitas.UltimoIdCliente);
        }

        private static readonly DateTime FechaInicio =
            new DateTime(
                2026,
                7,
                10);

        private static readonly DateTime FechaFin =
            new DateTime(
                2026,
                7,
                12);

        private static readonly DateTime FechaVisita =
            new DateTime(
                2026,
                7,
                11);

        private static int Registrar(
            VisitaService servicio)
        {
            return servicio.Registrar(
                1,
                FechaVisita,
                "Reunión comercial",
                "Rosario",
                new[]
                {
                    1
                });
        }

        private static VisitaService
            CrearServicioConViaje()
        {
            return CrearServicio(
                CrearUsuario(
                    VisitaService.PermisoRegistrar),
                CrearViajesConViaje(),
                CrearClientes());
        }

        private static VisitaService CrearServicio(
            Usuario usuario,
            ViajeRepositoryFalso viajes = null,
            ClienteRepositoryFalso clientes = null,
            VisitaRepositoryFalso visitas = null)
        {
            return new VisitaService(
                visitas
                    ?? new VisitaRepositoryFalso(),
                clientes
                    ?? new ClienteRepositoryFalso(),
                viajes
                    ?? new ViajeRepositoryFalso(),
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static ViajeRepositoryFalso
            CrearViajesConViaje()
        {
            return new ViajeRepositoryFalso
            {
                Obtenido =
                    CrearViaje()
            };
        }

        private static ClienteRepositoryFalso
            CrearClientes()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.Clientes.Add(
                CrearCliente(
                    1));

            return repository;
        }

        private static Viaje CrearViaje()
        {
            return new Viaje(
                1,
                FechaInicio,
                FechaFin,
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                0m);
        }

        private static Cliente CrearCliente(
            int idCliente)
        {
            return new Cliente(
                idCliente,
                "Empresa " +
                    idCliente,
                "30-1234567" +
                    idCliente +
                    "-9",
                string.Empty,
                string.Empty,
                "Rosario",
                "Santa Fe");
        }

        private static VisitaListadoDto CrearDto()
        {
            return new VisitaListadoDto(
                10,
                1,
                FechaVisita,
                "Reunión",
                "Rosario",
                "Empresa 1");
        }

        private static Usuario CrearUsuario(
            params string[] permisos)
        {
            Usuario usuario =
                new Usuario(
                    1,
                    1,
                    "usuario",
                    new byte[]
                    {
                        1
                    },
                    new byte[]
                    {
                        2
                    },
                    100000);

            Grupo grupo =
                new Grupo(
                    1,
                    "GRUPO_PRUEBA",
                    "Grupo de prueba",
                    string.Empty);

            int idPermiso = 1;

            foreach (
                string codigo
                in permisos)
            {
                grupo.AgregarComponente(
                    new Permiso(
                        idPermiso++,
                        codigo,
                        codigo,
                        string.Empty));
            }

            usuario.AgregarGrupo(
                grupo);

            return usuario;
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

        private sealed class ViajeRepositoryFalso
            : IViajeRepository
        {
            public Viaje Obtenido { get; set; }

            public Viaje ObtenerPorId(
                int idViaje)
            {
                if (Obtenido == null)
                {
                    return null;
                }

                return Obtenido.IdViaje == idViaje
                    ? Obtenido
                    : null;
            }

            public IReadOnlyCollection<ViajeListadoDto>
                Listar(
                    ViajeFiltro filtro)
            {
                return new List<ViajeListadoDto>()
                    .AsReadOnly();
            }

            public int Insertar(
                Viaje viaje,
                SIGEVIP.Application.Auditoria.AuditoriaRegistro auditoria)
            {
                return 0;
            }

            public void Actualizar(
                Viaje viaje,
                SIGEVIP.Application.Auditoria.AuditoriaRegistro auditoria)
            {
            }

            public void Cancelar(
                Viaje viaje,
                SIGEVIP.Application.Auditoria.AuditoriaRegistro auditoria)
            {
            }
        }

        private sealed class ClienteRepositoryFalso
            : IClienteConsultaVisitaRepository
        {
            public List<Cliente> Clientes
            {
                get;
            } = new List<Cliente>();

            public IReadOnlyCollection<ClienteSeleccionVisitaDto>
                ListarActivos()
            {
                return Clientes
                    .Where(
                        cliente =>
                            cliente.Activo)
                    .Select(
                        cliente =>
                            new ClienteSeleccionVisitaDto(
                                cliente.IdCliente,
                                cliente.RazonSocial,
                                cliente.Cuit,
                                cliente.Activo))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<Cliente>
                ObtenerPorIds(
                    IReadOnlyCollection<int> idsClientes)
            {
                return Clientes
                    .Where(
                        cliente =>
                            idsClientes.Contains(
                                cliente.IdCliente))
                    .ToList()
                    .AsReadOnly();
            }
        }

        private sealed class VisitaRepositoryFalso
            : IVisitaRepository
        {
            public VisitaRepositoryFalso()
            {
                PorViaje =
                    new List<VisitaListadoDto>();

                PorCliente =
                    new List<VisitaListadoDto>();
            }

            public Visita Insertada
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaInsertada
            {
                get;
                private set;
            }

            public int UltimoIdViaje
            {
                get;
                private set;
            }

            public int UltimoIdCliente
            {
                get;
                private set;
            }

            public List<VisitaListadoDto>
                PorViaje
            {
                get;
                private set;
            }

            public List<VisitaListadoDto>
                PorCliente
            {
                get;
                private set;
            }

            public int Insertar(
                Visita visita,
                AuditoriaRegistro auditoria)
            {
                Insertada = visita;

                AuditoriaInsertada =
                    auditoria;

                return 50;
            }

            public IReadOnlyCollection<VisitaListadoDto>
                ListarPorViaje(
                    int idViaje)
            {
                UltimoIdViaje = idViaje;

                return PorViaje.AsReadOnly();
            }

            public IReadOnlyCollection<VisitaListadoDto>
                ListarPorCliente(
                    int idCliente)
            {
                UltimoIdCliente = idCliente;

                return PorCliente.AsReadOnly();
            }
        }
    }
}