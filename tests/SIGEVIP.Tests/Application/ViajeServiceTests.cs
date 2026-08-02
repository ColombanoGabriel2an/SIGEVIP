using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ViajeServiceTests
    {
        [TestMethod]
        public void Registrar_SinSesion_RechazaOperacion()
        {
            ViajeService servicio =
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
                    ViajeService.PermisoCrear);

            usuario.Desactivar();

            ViajeService servicio =
                CrearServicio(
                    usuario);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_SinPermisoCrear_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_SinParticipantes_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear));

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Registrar(
                    FechaInicio,
                    FechaFin,
                    "Viaje",
                    TipoViaje.Desplazamiento,
                    0m,
                    new int[0]));
        }

        [TestMethod]
        public void Registrar_ParticipanteInexistente_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            PersonaRepositoryFalso personas =
                new PersonaRepositoryFalso();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    personas);

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_ParticipanteInactivo_RechazaOperacion()
        {
            PersonaRepositoryFalso personas =
                CrearPersonas();

            personas.Personas[0]
                .Desactivar();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    new ViajeRepositoryFalso(),
                    personas);

            Assert.ThrowsException<ReglaNegocioException>(
                () => Registrar(
                    servicio));
        }

        [TestMethod]
        public void Registrar_DatosValidos_CreaViajeAbierto()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    CrearPersonas());

            int id =
                Registrar(
                    servicio);

            Assert.AreEqual(
                20,
                id);

            Assert.IsNotNull(
                viajes.Insertado);

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viajes.Insertado.EstadoActual);

            Assert.AreEqual(
                1,
                viajes.Insertado.Participantes.Count);

            Assert.IsNotNull(
                viajes.AuditoriaInsertada);

            Assert.AreEqual(
                "Viajes",
                viajes.AuditoriaInsertada.Modulo);

            Assert.AreEqual(
                "Alta",
                viajes.AuditoriaInsertada.Accion);

            Assert.AreEqual(
                "Viaje",
                viajes.AuditoriaInsertada.Entidad);

            Assert.IsNull(
                viajes.AuditoriaInsertada.IdEntidad);
        }

        [TestMethod]
        public void Registrar_ConAnticipo_ConservaMonto()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    CrearPersonas());

            servicio.Registrar(
                FechaInicio,
                FechaFin,
                "Viaje",
                TipoViaje.Desplazamiento,
                2500m,
                new[]
                {
                    1
                });

            Assert.AreEqual(
                2500m,
                viajes.Insertado.MontoAnticipado);
        }

        [TestMethod]
        public void Modificar_ViajeInexistente_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    new ViajeRepositoryFalso(),
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => Modificar(
                    servicio));
        }

        [TestMethod]
        public void Modificar_ViajeNoAbierto_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .EnviarARendicion();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => Modificar(
                    servicio));
        }

        [TestMethod]
        public void Modificar_ViajeAbierto_ActualizaDatos()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .ReemplazarParticipantes(
                    new[]
                    {
                        CrearPersona(1)
                    });

            PersonaRepositoryFalso personas =
                CrearPersonas();

            personas.Personas.Add(
                CrearPersona(2));

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    personas);

            servicio.Modificar(
                1,
                new DateTime(2026, 8, 1),
                new DateTime(2026, 8, 5),
                "Modificado",
                TipoViaje.EventoFeria,
                3000m,
                new[]
                {
                    2
                });

            Assert.AreSame(
                viajes.Obtenido,
                viajes.Actualizado);

            Assert.AreEqual(
                "Modificado",
                viajes.Actualizado.Descripcion);

            Assert.AreEqual(
                2,
                viajes.Actualizado
                    .Participantes
                    .Single()
                    .IdPersona);

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viajes.Actualizado.EstadoActual);

            Assert.IsNotNull(
                viajes.AuditoriaActualizacion);

            Assert.AreEqual(
                "Modificacion",
                viajes
                    .AuditoriaActualizacion
                    .Accion);

            Assert.AreEqual(
                1,
                viajes
                    .AuditoriaActualizacion
                    .IdEntidad);
        }

        [TestMethod]
        public void Modificar_ParticipanteHistoricoInactivo_PermiteConservarlo()
        {
            Persona participante =
                CrearPersona(
                    1);

            participante.Desactivar();

            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .ReemplazarParticipantes(
                    new[]
                    {
                        participante
                    });

            PersonaRepositoryFalso personas =
                new PersonaRepositoryFalso();

            personas.Personas.Add(
                participante);

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    personas);

            Modificar(
                servicio);

            Assert.IsNotNull(
                viajes.Actualizado);

            Assert.IsFalse(
                viajes.Actualizado
                    .Participantes
                    .Single()
                    .Activo);
        }

        [TestMethod]
        public void Modificar_NuevoParticipanteInactivo_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .ReemplazarParticipantes(
                    new[]
                    {
                        CrearPersona(1)
                    });

            Persona participanteInactivo =
                CrearPersona(
                    2);

            participanteInactivo.Desactivar();

            PersonaRepositoryFalso personas =
                CrearPersonas();

            personas.Personas.Add(
                participanteInactivo);

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    personas);

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    1,
                    FechaInicio,
                    FechaFin,
                    "Viaje",
                    TipoViaje.Desplazamiento,
                    0m,
                    new[]
                    {
                        2
                    }));
        }

        [TestMethod]
        public void Modificar_FechasInvalidas_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    viajes,
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Modificar(
                    1,
                    FechaFin,
                    FechaInicio,
                    "Inválido",
                    TipoViaje.Desplazamiento,
                    0m,
                    new[]
                    {
                        1
                    }));
        }

        [TestMethod]
        public void Listar_SinPermisoConsultar_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario());

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Listar(
                    null));
        }

        [TestMethod]
        public void Listar_ConPermisoConsultar_DevuelveResultados()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Resultados.Add(
                CrearDto());

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    viajes,
                    CrearPersonas());

            IReadOnlyCollection<ViajeListadoDto> resultado =
                servicio.Listar(
                    null);

            Assert.AreEqual(
                1,
                resultado.Count);
        }

        [TestMethod]
        public void Listar_TransfiereFiltro()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            ViajeFiltro filtro =
                new ViajeFiltro(
                    FechaInicio,
                    FechaFin,
                    EstadoViaje.Abierto,
                    1);

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    viajes,
                    CrearPersonas());

            servicio.Listar(
                filtro);

            Assert.AreSame(
                filtro,
                viajes.UltimoFiltro);
        }

        [TestMethod]
        public void Obtener_IdInvalido_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.Obtener(
                    0));
        }

        [TestMethod]
        public void Obtener_ViajeExistente_DevuelveAgregado()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    viajes,
                    CrearPersonas());

            Assert.AreSame(
                viajes.Obtenido,
                servicio.Obtener(
                    1));
        }

        [TestMethod]
        public void Cancelar_SinPermisoCancelar_RechazaOperacion()
        {
            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException<AccesoDenegadoException>(
                () => servicio.Cancelar(
                    1));
        }

        [TestMethod]
        public void Cancelar_ViajeAbierto_PersisteCancelacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCancelar),
                    viajes,
                    CrearPersonas());

            servicio.Cancelar(
                1);

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viajes.Cancelado.EstadoActual);

            Assert.IsNotNull(
                viajes.AuditoriaCancelacion);

            Assert.AreEqual(
                "Cancelacion",
                viajes
                    .AuditoriaCancelacion
                    .Accion);

            Assert.AreEqual(
                1,
                viajes
                    .AuditoriaCancelacion
                    .IdEntidad);
        }

        [TestMethod]
        public void Cancelar_ViajeEnRendicion_PersisteCancelacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .EnviarARendicion();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCancelar),
                    viajes,
                    CrearPersonas());

            servicio.Cancelar(
                1);

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viajes.Cancelado.EstadoActual);
        }

        [TestMethod]
        public void Cancelar_ViajeAprobado_RechazaOperacion()
        {
            ViajeRepositoryFalso viajes =
                new ViajeRepositoryFalso();

            viajes.Obtenido =
                CrearViaje();

            viajes.Obtenido
                .EnviarARendicion();

            viajes.Obtenido
                .Aprobar();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCancelar),
                    viajes,
                    CrearPersonas());

            Assert.ThrowsException<ReglaNegocioException>(
                () => servicio.Cancelar(
                    1));

            Assert.IsNull(
                viajes.Cancelado);
        }

        [TestMethod]
        public void ListarParticipantesDisponibles_DevuelveActivos()
        {
            PersonaRepositoryFalso personas =
                CrearPersonas();

            ViajeService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoCrear),
                    new ViajeRepositoryFalso(),
                    personas);

            IReadOnlyCollection<PersonaSeleccionDto> resultado =
                servicio
                    .ListarParticipantesDisponibles();

            Assert.AreEqual(
                1,
                resultado.Count);
        }

        private static readonly DateTime FechaInicio =
            new DateTime(2026, 7, 10);

        private static readonly DateTime FechaFin =
            new DateTime(2026, 7, 12);

        private static int Registrar(
            ViajeService servicio)
        {
            return servicio.Registrar(
                FechaInicio,
                FechaFin,
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                1000m,
                new[]
                {
                    1
                });
        }

        private static void Modificar(
            ViajeService servicio)
        {
            servicio.Modificar(
                1,
                FechaInicio,
                FechaFin,
                "Viaje modificado",
                TipoViaje.EnOficina,
                500m,
                new[]
                {
                    1
                });
        }

        private static ViajeService CrearServicio(
            Usuario usuario,
            ViajeRepositoryFalso viajes = null,
            PersonaRepositoryFalso personas = null)
        {
            return new ViajeService(
                viajes
                    ?? new ViajeRepositoryFalso(),
                personas
                    ?? new PersonaRepositoryFalso(),
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
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

            foreach (string codigo in permisos)
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

        private static PersonaRepositoryFalso CrearPersonas()
        {
            PersonaRepositoryFalso personas =
                new PersonaRepositoryFalso();

            personas.Personas.Add(
                CrearPersona(1));

            return personas;
        }

        private static Persona CrearPersona(
            int idPersona)
        {
            return new Persona(
                idPersona,
                "Persona",
                "Prueba " + idPersona,
                "persona" +
                idPersona +
                "@correo.com");
        }

        private static Viaje CrearViaje()
        {
            return new Viaje(
                1,
                FechaInicio,
                FechaFin,
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                1000m);
        }

        private static ViajeListadoDto CrearDto()
        {
            return new ViajeListadoDto(
                1,
                FechaInicio,
                FechaFin,
                "Viaje",
                TipoViaje.Desplazamiento,
                EstadoViaje.Abierto,
                1000m,
                "Persona Prueba");
        }

        private sealed class ViajeRepositoryFalso
            : IViajeRepository
        {
            public Viaje Obtenido { get; set; }

            public Viaje Insertado { get; private set; }

            public Viaje Actualizado { get; private set; }

            public Viaje Cancelado { get; private set; }

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

            public AuditoriaRegistro AuditoriaCancelacion
            {
                get;
                private set;
            }

            public ViajeFiltro UltimoFiltro { get; private set; }

            public List<ViajeListadoDto> Resultados
            {
                get;
            } = new List<ViajeListadoDto>();

            public Viaje ObtenerPorId(
                int idViaje)
            {
                return Obtenido;
            }

            public IReadOnlyCollection<ViajeListadoDto> Listar(
                ViajeFiltro filtro)
            {
                UltimoFiltro = filtro;

                return Resultados.AsReadOnly();
            }

            public int Insertar(
                Viaje viaje,
                AuditoriaRegistro auditoria)
            {
                Insertado = viaje;

                AuditoriaInsertada =
                    auditoria;

                return 20;
            }

            public void Actualizar(
                Viaje viaje,
                AuditoriaRegistro auditoria)
            {
                Actualizado = viaje;

                AuditoriaActualizacion =
                    auditoria;
            }

            public void Cancelar(
                Viaje viaje,
                AuditoriaRegistro auditoria)
            {
                Cancelado = viaje;

                AuditoriaCancelacion =
                    auditoria;
            }
        }

        private sealed class PersonaRepositoryFalso
            : IPersonaConsultaRepository
        {
            public List<Persona> Personas
            {
                get;
            } = new List<Persona>();

            public IReadOnlyCollection<PersonaSeleccionDto>
                ListarActivas()
            {
                return Personas
                    .Where(
                        persona =>
                            persona.Activo)
                    .Select(
                        persona =>
                            new PersonaSeleccionDto(
                                persona.IdPersona,
                                persona.Nombre +
                                " " +
                                persona.Apellido,
                                persona.Activo))
                    .ToList()
                    .AsReadOnly();
            }

            public IReadOnlyCollection<Persona>
                ObtenerPorIds(
                    IReadOnlyCollection<int> idsPersona)
            {
                return Personas
                    .Where(
                        persona =>
                            idsPersona.Contains(
                                persona.IdPersona))
                    .ToList()
                    .AsReadOnly();
            }
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
    }
}
