using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ViaticoServiceTests
    {
        [TestMethod]
        public void Registrar_SinSesion_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_SinPermiso_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_ViajeInexistente_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Registrar_DatosValidos_InsertaViaticoAsociado()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViaje();

            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    viajes,
                    viaticos,
                    CrearPersonas());

            int id =
                servicio.Registrar(
                    CrearComandoRegistro());

            Assert.AreEqual(
                30,
                id);

            Assert.IsNotNull(
                viaticos.Insertado);

            Assert.AreEqual(
                1,
                viaticos.Insertado.IdViaje);

            Assert.AreEqual(
                1,
                viajes.Obtenido.Viaticos.Count);

            Assert.AreEqual(
                1500m,
                viaticos.Insertado.Monto);

            Assert.IsNotNull(
                viaticos.AuditoriaInsertada);

            Assert.AreEqual(
                "Viaticos",
                viaticos.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                viaticos.AuditoriaInsertada.Accion);

            Assert.AreEqual(
                "Viatico",
                viaticos.AuditoriaInsertada.Entidad);

            Assert.IsNull(
                viaticos.AuditoriaInsertada.IdEntidad);
        }

        [TestMethod]
        public void Registrar_PagoPersonal_ResuelvePagador()
        {
            PersonaRepositoryFalso personas =
                CrearPersonas();

            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    viaticos,
                    personas);

            servicio.Registrar(
                CrearComandoRegistro(
                    MetodoPago.PagoPersonal,
                    1));

            Assert.AreSame(
                personas.Personas[0],
                viaticos.Insertado.PagadoPor);
        }

        [TestMethod]
        public void Registrar_PagadorNoParticipante_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    new ViaticoRepositoryFalso(),
                    new PersonaRepositoryFalso());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro(
                        MetodoPago.PagoPersonal,
                        99)));
        }

        [TestMethod]
        public void Registrar_PagadorInactivo_RechazaOperacion()
        {
            PersonaRepositoryFalso personas =
                CrearPersonas();

            personas.Personas[0]
                .Desactivar();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    new ViaticoRepositoryFalso(),
                    personas);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro(
                        MetodoPago.PagoPersonal,
                        1)));
        }

        [TestMethod]
        public void Registrar_ConComprobante_ConstruyeComprobante()
        {
            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    viaticos,
                    CrearPersonas());

            servicio.Registrar(
                CrearComandoRegistro(
                    MetodoPago.EfectivoEmpresa,
                    null,
                    CrearComprobanteInput()));

            Assert.IsNotNull(
                viaticos.Insertado.Comprobante);

            Assert.AreEqual(
                TipoComprobante.FacturaB,
                viaticos.Insertado
                    .Comprobante
                    .Tipo);

            Assert.AreEqual(
                1500m,
                viaticos.Insertado
                    .Comprobante
                    .Total);
        }

        [TestMethod]
        public void Registrar_ComprobanteConTotalDiferente_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    CrearViajesConViaje(),
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException
                <ReglaNegocioException>(
                    () => servicio.Registrar(
                        CrearComandoRegistro(
                            MetodoPago.EfectivoEmpresa,
                            null,
                            new ComprobanteInput(
                                0,
                                TipoComprobante.FacturaB,
                                "30-12345678-9",
                                "Proveedor de prueba",
                                SituacionFiscal.ResponsableInscripto,
                                "0001",
                                "00001234",
                                1000m,
                                210m))));
        }

        [TestMethod]
        public void Registrar_ViajeEnRendicion_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViaje();

            viajes.Obtenido
                .EnviarARendicion();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoRegistrar),
                    viajes,
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    CrearComandoRegistro()));
        }

        [TestMethod]
        public void Modificar_DatosValidos_ActualizaViatico()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViatico();

            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    viajes,
                    viaticos,
                    CrearPersonas());

            servicio.Modificar(
                new ModificarViaticoCommand(
                    1,
                    10,
                    new DateTime(2026, 7, 12),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.PagoPersonal,
                    1,
                    2000m,
                    "Cena comercial",
                    null));

            Assert.IsNotNull(
                viaticos.Actualizado);

            Assert.AreEqual(
                2000m,
                viaticos.Actualizado.Monto);

            Assert.AreEqual(
                CategoriaGasto.Alimentacion,
                viaticos.Actualizado.Categoria);

            Assert.AreEqual(
                "Cena comercial",
                viaticos.Actualizado.Descripcion);

            Assert.IsNotNull(
                viaticos.AuditoriaActualizacion);

            Assert.AreEqual(
                "Viaticos",
                viaticos.AuditoriaActualizacion.Modulo);

            Assert.AreEqual(
                "Modificacion",
                viaticos.AuditoriaActualizacion.Accion);

            Assert.AreEqual(
                "Viatico",
                viaticos.AuditoriaActualizacion.Entidad);

            Assert.AreEqual(
                10,
                viaticos.AuditoriaActualizacion.IdEntidad);
        }

        [TestMethod]
        public void Modificar_ComprobanteConTotalDiferente_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    CrearViajesConViatico(),
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException
                <ReglaNegocioException>(
                    () => servicio.Modificar(
                        new ModificarViaticoCommand(
                            1,
                            10,
                            new DateTime(
                                2026,
                                7,
                                11),
                            CategoriaGasto.Otros,
                            MetodoPago.EfectivoEmpresa,
                            null,
                            2000m,
                            "Gasto modificado",
                            CrearComprobanteInput())));
        }

        [TestMethod]
        public void Modificar_ViaticoAjenoAlViaje_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    CrearViajesConViaje(),
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void Modificar_PagadorNoParticipante_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    CrearViajesConViatico(),
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException
                <ReglaNegocioException>(
                    () => servicio.Modificar(
                        new ModificarViaticoCommand(
                            1,
                            10,
                            new DateTime(
                                2026,
                                7,
                                11),
                            CategoriaGasto.Otros,
                            MetodoPago.PagoPersonal,
                            99,
                            1800m,
                            "Traslado",
                            null)));
        }

        [TestMethod]
        public void Modificar_ViajeEnRendicion_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViatico();

            viajes.Obtenido
                .EnviarARendicion();

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    viajes,
                    new ViaticoRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    CrearComandoModificacion()));
        }

        [TestMethod]
        public void ListarPorViaje_ConPermiso_DevuelveResultados()
        {
            ViajeRepositoryFalso viajes =
                CrearViajesConViaje();

            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            viaticos.Resultados.Add(
                CrearDto());

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar),
                    viajes,
                    viaticos,
                    CrearPersonas());

            IReadOnlyCollection<ViaticoListadoDto>
                resultado =
                    servicio.ListarPorViaje(
                        1,
                        null);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.AreEqual(
                1,
                viaticos.UltimoIdViaje);

            Assert.IsNotNull(
                viaticos.UltimoFiltro);
        }

        [TestMethod]
        public void Obtener_ViaticoInexistente_RechazaOperacion()
        {
            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Obtener(
                    10));
        }

        [TestMethod]
        public void Obtener_ViaticoExistente_DevuelveEntidad()
        {
            ViaticoRepositoryFalso viaticos =
                new ViaticoRepositoryFalso();

            viaticos.Obtenido =
                CrearViatico(
                    10);

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar),
                    new ViajeRepositoryFalso(),
                    viaticos,
                    CrearPersonas());

            Assert.AreSame(
                viaticos.Obtenido,
                servicio.Obtener(
                    10));
        }

        [TestMethod]
        public void ListarPagadoresDisponibles_DevuelveSoloParticipantesActivos()
        {
            PersonaRepositoryFalso personas =
                CrearPersonas();

            personas.Personas.Add(
                new Persona(
                    2,
                    "Lucía",
                    "No participante",
                    "lucia@sigevip.local"));

            ViaticoService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoModificar),
                    CrearViajesConViaje(),
                    new ViaticoRepositoryFalso(),
                    personas);

            IReadOnlyCollection<PagadorSeleccionDto>
                resultado =
                    servicio
                        .ListarPagadoresDisponibles(
                            1);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.AreEqual(
                1,
                resultado
                    .Single()
                    .IdPersona);
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

        private static RegistrarViaticoCommand
            CrearComandoRegistro(
                MetodoPago metodoPago =
                    MetodoPago.EfectivoEmpresa,
                int? idPersonaPagadora = null,
                ComprobanteInput comprobante = null)
        {
            return new RegistrarViaticoCommand(
                1,
                new DateTime(2026, 7, 11),
                CategoriaGasto.Otros,
                metodoPago,
                idPersonaPagadora,
                1500m,
                "Gasto comercial",
                comprobante);
        }

        private static ModificarViaticoCommand
            CrearComandoModificacion()
        {
            return new ModificarViaticoCommand(
                1,
                10,
                new DateTime(2026, 7, 11),
                CategoriaGasto.Transporte,
                MetodoPago.EfectivoEmpresa,
                null,
                1800m,
                "Traslado",
                null);
        }

        private static ComprobanteInput
            CrearComprobanteInput()
        {
            return new ComprobanteInput(
                0,
                TipoComprobante.FacturaB,
                "30-12345678-9",
                "Proveedor de prueba",
                SituacionFiscal.ResponsableInscripto,
                "0001",
                "00001234",
                1290m,
                210m);
        }

        private static ViaticoService CrearServicio(
            Usuario usuario,
            ViajeRepositoryFalso viajes = null,
            ViaticoRepositoryFalso viaticos = null,
            PersonaRepositoryFalso personas = null)
        {
            return new ViaticoService(
                viaticos
                    ?? new ViaticoRepositoryFalso(),
                viajes
                    ?? new ViajeRepositoryFalso(),
                personas
                    ?? new PersonaRepositoryFalso(),
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

        private static ViajeRepositoryFalso
            CrearViajesConViatico()
        {
            Viaje viaje =
                CrearViaje();

            viaje.AgregarViatico(
                CrearViatico(
                    10));

            return new ViajeRepositoryFalso
            {
                Obtenido =
                    viaje
            };
        }

        private static Viaje CrearViaje()
        {
            var viaje =
                new Viaje(
                    1,
                    FechaInicio,
                    FechaFin,
                    "Viaje comercial",
                    TipoViaje.Desplazamiento,
                    0m);

            viaje.AgregarParticipante(
                new Persona(
                    1,
                    "Ana",
                    "Administrativa",
                    "ana@sigevip.local"));

            return viaje;
        }

        private static Viatico CrearViatico(
            int idViatico)
        {
            return new Viatico(
                idViatico,
                new DateTime(2026, 7, 11),
                CategoriaGasto.Otros,
                MetodoPago.EfectivoEmpresa,
                null,
                1000m,
                "Gasto",
                null);
        }

        private static PersonaRepositoryFalso
            CrearPersonas()
        {
            PersonaRepositoryFalso personas =
                new PersonaRepositoryFalso();

            personas.Personas.Add(
                new Persona(
                    1,
                    "Ana",
                    "Administrativa",
                    "ana@sigevip.local"));

            return personas;
        }

        private static ViaticoListadoDto CrearDto()
        {
            return new ViaticoListadoDto(
                10,
                1,
                new DateTime(2026, 7, 11),
                CategoriaGasto.Otros,
                MetodoPago.EfectivoEmpresa,
                string.Empty,
                1000m,
                "Gasto",
                EstadoViatico.Vigente,
                false);
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

            int idPermiso =
                1;

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

        private sealed class ViajeRepositoryFalso
            : IViajeRepository
        {
            public Viaje Obtenido
            {
                get;
                set;
            }

            public Viaje ObtenerPorId(
                int idViaje)
            {
                if (Obtenido == null)
                {
                    return null;
                }

                return Obtenido.IdViaje ==
                       idViaje
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

        private sealed class ViaticoRepositoryFalso
            : IViaticoRepository
        {
            public Viatico Obtenido
            {
                get;
                set;
            }

            public Viatico Insertado
            {
                get;
                private set;
            }

            public Viatico Actualizado
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

            public int UltimoIdViaje
            {
                get;
                private set;
            }

            public ViaticoFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public List<ViaticoListadoDto> Resultados
            {
                get;
            } = new List<ViaticoListadoDto>();

            public Viatico ObtenerPorId(
                int idViatico)
            {
                if (Obtenido == null)
                {
                    return null;
                }

                return Obtenido.IdViatico ==
                       idViatico
                    ? Obtenido
                    : null;
            }

            public IReadOnlyCollection<ViaticoListadoDto>
                ListarPorViaje(
                    int idViaje,
                    ViaticoFiltro filtro)
            {
                UltimoIdViaje =
                    idViaje;

                UltimoFiltro =
                    filtro;

                return Resultados
                    .AsReadOnly();
            }

            public int Insertar(
                Viatico viatico,
                AuditoriaRegistro auditoria)
            {
                Insertado =
                    viatico;

                AuditoriaInsertada =
                    auditoria;

                return 30;
            }

            public void Actualizar(
                Viatico viatico,
                AuditoriaRegistro auditoria)
            {
                Actualizado =
                    viatico;

                AuditoriaActualizacion =
                    auditoria;
            }
        }

        private sealed class PersonaRepositoryFalso
            : IPersonaConsultaViaticoRepository
        {
            public List<Persona> Personas
            {
                get;
            } = new List<Persona>();

            public IReadOnlyCollection<PagadorSeleccionDto>
                ListarActivas()
            {
                return Personas
                    .Where(
                        persona =>
                            persona.Activo)
                    .Select(
                        persona =>
                            new PagadorSeleccionDto(
                                persona.IdPersona,
                                persona.Nombre +
                                " " +
                                persona.Apellido,
                                persona.Activo))
                    .ToList()
                    .AsReadOnly();
            }

            public Persona ObtenerPorId(
                int idPersona)
            {
                return Personas
                    .FirstOrDefault(
                        persona =>
                            persona.IdPersona ==
                            idPersona);
            }
        }
    }
}