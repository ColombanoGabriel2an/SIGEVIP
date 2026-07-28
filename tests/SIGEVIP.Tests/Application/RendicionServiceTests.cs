using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class RendicionServiceTests
    {
        [TestMethod]
        public void Enviar_SinSesion_RechazaOperacion()
        {
            RendicionService servicio =
                CrearServicio(
                    null);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Enviar(
                    new EnviarRendicionCommand(
                        1)));
        }

        [TestMethod]
        public void Enviar_SinPermiso_RechazaOperacion()
        {
            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Enviar(
                    new EnviarRendicionCommand(
                        1)));
        }

        [TestMethod]
        public void Enviar_ViajeInexistente_RechazaOperacion()
        {
            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoEnviar));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Enviar(
                    new EnviarRendicionCommand(
                        1)));
        }

        [TestMethod]
        public void Enviar_ViajeAbierto_RegistraAuditoriaYPersiste()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeAbierto();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoEnviar),
                    repository);

            DateTime antes =
                DateTime.Now;

            servicio.Enviar(
                new EnviarRendicionCommand(
                    1));

            DateTime despues =
                DateTime.Now;

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                repository.Enviado.EstadoActual);

            Assert.AreEqual(
                10,
                repository.Enviado
                    .IdUsuarioEnvioRendicion);

            Assert.IsTrue(
                repository.Enviado
                    .FechaEnvioRendicion
                    .HasValue);

            Assert.IsTrue(
                repository.Enviado
                    .FechaEnvioRendicion
                    .Value >= antes);

            Assert.IsTrue(
                repository.Enviado
                    .FechaEnvioRendicion
                    .Value <= despues);

            Assert.IsNotNull(
                repository.AuditoriaEnvio);

            Assert.AreEqual(
                10,
                repository.AuditoriaEnvio.IdUsuario);

            Assert.AreEqual(
                "usuario",
                repository.AuditoriaEnvio.NombreUsuario);

            Assert.AreEqual(
                "Rendiciones",
                repository.AuditoriaEnvio.Modulo);

            Assert.AreEqual(
                "EnvioARendicion",
                repository.AuditoriaEnvio.Accion);

            Assert.AreEqual(
                "Viaje",
                repository.AuditoriaEnvio.Entidad);

            Assert.AreEqual(
                1,
                repository.AuditoriaEnvio.IdEntidad);
        }

        [TestMethod]
        public void ListarPendientes_ConPermiso_DevuelveResultados()
        {
            RendicionRepositoryFalso repository =
                new RendicionRepositoryFalso();

            repository.Pendientes.Add(
                CrearListadoDto());

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar),
                    repository);

            IReadOnlyCollection<RendicionListadoDto>
                resultado =
                    servicio.ListarPendientes();

            Assert.AreEqual(
                1,
                resultado.Count);
        }

        [TestMethod]
        public void ListarPendientes_RepositorioDevuelveNull_DevuelveVacio()
        {
            RendicionRepositoryFalso repository =
                new RendicionRepositoryFalso();

            repository.DevolverPendientesNulos =
                true;

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar),
                    repository);

            Assert.AreEqual(
                0,
                servicio.ListarPendientes().Count);
        }

        [TestMethod]
        public void ObtenerDetalle_ConViaje_MapeaViaticos()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar),
                    repository);

            RendicionDetalleDto detalle =
                servicio.ObtenerDetalle(
                    1);

            Assert.AreEqual(
                1,
                detalle.IdViaje);

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                detalle.Estado);

            Assert.AreEqual(
                1,
                detalle.Viaticos.Count);

            Assert.AreEqual(
                1000m,
                detalle.TotalGastado);
        }

        [TestMethod]
        public void ExcluirViatico_DatosValidos_ExcluyeYAudita()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService
                            .PermisoExcluirViatico),
                    repository);

            servicio.ExcluirViatico(
                new ExcluirViaticoCommand(
                    1,
                    20,
                    "Comprobante inválido"));

            Assert.AreEqual(
                EstadoViatico.Excluido,
                repository.ViaticoExcluido.Estado);

            Assert.AreEqual(
                "Comprobante inválido",
                repository.ViaticoExcluido
                    .MotivoExclusion);

            Assert.AreEqual(
                10,
                repository.ViaticoExcluido
                    .IdUsuarioExclusion);
        }

        [TestMethod]
        public void ExcluirViatico_ViaticoAjeno_RechazaOperacion()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService
                            .PermisoExcluirViatico),
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.ExcluirViatico(
                    new ExcluirViaticoCommand(
                        1,
                        99,
                        "No corresponde")));
        }

        [TestMethod]
        public void ReactivarViatico_Excluido_ReactivaYAudita()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            Viatico viatico =
                repository.Obtenido
                    .Viaticos
                    .Single();

            repository.Obtenido
                .ExcluirViatico(
                    viatico,
                    "Error",
                    8,
                    DateTime.Now);

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService
                            .PermisoReactivarViatico),
                    repository);

            servicio.ReactivarViatico(
                new ReactivarViaticoCommand(
                    1,
                    20));

            Assert.AreEqual(
                EstadoViatico.Vigente,
                repository.ViaticoReactivado
                    .Estado);

            Assert.AreEqual(
                10,
                repository.ViaticoReactivado
                    .IdUsuarioReactivacion);
        }

        [TestMethod]
        public void AjustarAnticipo_EnRendicion_ActualizaYPersiste()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService
                            .PermisoAjustarAnticipo),
                    repository);

            servicio.AjustarAnticipo(
                new AjustarAnticipoCommand(
                    1,
                    600m));

            Assert.AreEqual(
                600m,
                repository.AnticipoAjustado
                    .MontoAnticipado);

            Assert.AreEqual(
                400m,
                repository.AnticipoAjustado
                    .SaldoPendiente);
        }

        [TestMethod]
        public void Aprobar_EnRendicion_ApruebaYAudita()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoAprobar),
                    repository);

            servicio.Aprobar(
                new AprobarRendicionCommand(
                    1));

            Assert.AreEqual(
                EstadoViaje.Aprobado,
                repository.Aprobado.EstadoActual);

            Assert.AreEqual(
                10,
                repository.Aprobado
                    .IdUsuarioAprobador);

            Assert.IsTrue(
                repository.Aprobado
                    .FechaAprobacion
                    .HasValue);
        }

        [TestMethod]
        public void Aprobar_ViajeAbierto_RechazaOperacion()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeAbierto();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoAprobar),
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Aprobar(
                    new AprobarRendicionCommand(
                        1)));

            Assert.IsNull(
                repository.Aprobado);
        }

        [TestMethod]
        public void Cancelar_EnRendicionSinVisitas_CancelaYAudita()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoCancelar),
                    repository);

            servicio.Cancelar(
                new CancelarRendicionCommand(
                    1,
                    "Viaje informado por error"));

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                repository.Cancelado.EstadoActual);

            Assert.AreEqual(
                "Viaje informado por error",
                repository.Cancelado
                    .MotivoCancelacion);

            Assert.AreEqual(
                10,
                repository.Cancelado
                    .IdUsuarioCancelacion);
        }

        [TestMethod]
        public void Cancelar_ConVisitas_RechazaOperacion()
        {
            RendicionRepositoryFalso repository =
                CrearRepositoryConViajeEnRendicion();

            Viaje viaje =
                repository.Obtenido;

            Visita visita =
                new Visita(
                    30,
                    new DateTime(
                        2026,
                        7,
                        11),
                    "Reunión",
                    "Rosario");

            visita.AgregarCliente(
                new Cliente(
                    1,
                    "Empresa",
                    "30-12345678-9",
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe"));

            Viaje reconstruido =
                Viaje.Reconstruir(
                    viaje.IdViaje,
                    viaje.FechaInicio,
                    viaje.FechaFin,
                    viaje.Descripcion,
                    viaje.TipoViaje,
                    viaje.MontoAnticipado,
                    EstadoViaje.EnRendicion,
                    viaje.Participantes,
                    new[]
                    {
                        visita
                    },
                    viaje.Viaticos,
                    viaje.IdUsuarioEnvioRendicion,
                    viaje.FechaEnvioRendicion,
                    null,
                    null,
                    null,
                    null,
                    null);

            repository.Obtenido =
                reconstruido;

            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoCancelar),
                    repository);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Cancelar(
                    new CancelarRendicionCommand(
                        1,
                        "Cancelar")));

            Assert.IsNull(
                repository.Cancelado);
        }

        [TestMethod]
        public void ExcluirViatico_SinPermiso_RechazaOperacion()
        {
            RendicionService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar),
                    CrearRepositoryConViajeEnRendicion());

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.ExcluirViatico(
                    new ExcluirViaticoCommand(
                        1,
                        20,
                        "Motivo")));
        }

        private static RendicionService CrearServicio(
            Usuario usuario,
            RendicionRepositoryFalso repository = null)
        {
            return new RendicionService(
                repository
                    ?? new RendicionRepositoryFalso(),
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static RendicionRepositoryFalso
            CrearRepositoryConViajeAbierto()
        {
            return new RendicionRepositoryFalso
            {
                Obtenido =
                    CrearViajeAbierto()
            };
        }

        private static RendicionRepositoryFalso
            CrearRepositoryConViajeEnRendicion()
        {
            Viaje viaje =
                CrearViajeAbierto();

            viaje.AgregarViatico(
                new Viatico(
                    20,
                    new DateTime(
                        2026,
                        7,
                        11),
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    1000m,
                    "Gasto",
                    null));

            viaje.EnviarARendicion(
                5,
                new DateTime(
                    2026,
                    7,
                    13));

            return new RendicionRepositoryFalso
            {
                Obtenido =
                    viaje
            };
        }

        private static Viaje CrearViajeAbierto()
        {
            Viaje viaje =
                new Viaje(
                    1,
                    new DateTime(
                        2026,
                        7,
                        10),
                    new DateTime(
                        2026,
                        7,
                        12),
                    "Viaje comercial",
                    TipoViaje.Desplazamiento,
                    500m);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    new Persona(
                        1,
                        "Ana",
                        "Pérez",
                        "ana@sigevip.local")
                });

            return viaje;
        }

        private static RendicionListadoDto
            CrearListadoDto()
        {
            return new RendicionListadoDto(
                1,
                new DateTime(
                    2026,
                    7,
                    10),
                new DateTime(
                    2026,
                    7,
                    12),
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                "Ana Pérez",
                500m,
                1000m,
                500m,
                new DateTime(
                    2026,
                    7,
                    13));
        }

        private static Usuario CrearUsuario(
            params string[] permisos)
        {
            Usuario usuario =
                new Usuario(
                    10,
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

        private sealed class RendicionRepositoryFalso
            : IRendicionRepository
        {
            public Viaje Obtenido
            {
                get;
                set;
            }

            public Viaje Enviado
            {
                get;
                private set;
            }

            public AuditoriaRegistro AuditoriaEnvio
            {
                get;
                private set;
            }

            public Viaje AnticipoAjustado
            {
                get;
                private set;
            }

            public Viaje Aprobado
            {
                get;
                private set;
            }

            public Viaje Cancelado
            {
                get;
                private set;
            }

            public Viatico ViaticoExcluido
            {
                get;
                private set;
            }

            public Viatico ViaticoReactivado
            {
                get;
                private set;
            }

            public bool DevolverPendientesNulos
            {
                get;
                set;
            }

            public List<RendicionListadoDto> Pendientes
            {
                get;
            } = new List<RendicionListadoDto>();

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

            public IReadOnlyCollection<RendicionListadoDto>
                ListarPendientes()
            {
                if (DevolverPendientesNulos)
                {
                    return null;
                }

                return Pendientes
                    .AsReadOnly();
            }

            public void Enviar(
                Viaje viaje,
                AuditoriaRegistro auditoria)
            {
                Enviado =
                    viaje;

                AuditoriaEnvio =
                    auditoria;
            }

            public void ExcluirViatico(
                Viaje viaje,
                Viatico viatico)
            {
                ViaticoExcluido =
                    viatico;
            }

            public void ReactivarViatico(
                Viaje viaje,
                Viatico viatico)
            {
                ViaticoReactivado =
                    viatico;
            }

            public void AjustarMontoAnticipado(
                Viaje viaje)
            {
                AnticipoAjustado =
                    viaje;
            }

            public void Aprobar(
                Viaje viaje)
            {
                Aprobado =
                    viaje;
            }

            public void Cancelar(
                Viaje viaje)
            {
                Cancelado =
                    viaje;
            }
        }
    }
}