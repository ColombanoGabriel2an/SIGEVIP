using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Viajes;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ViajeRepositoryIntegrationTests
    {
        [TestMethod]
        public void PersonaConsulta_ListarActivas_DevuelvePersonaActiva()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                PersonaSeleccionDto persona =
                    CrearPersonaRepository()
                        .ListarActivas()
                        .SingleOrDefault(
                            actual =>
                                actual.IdPersona ==
                                datos.IdPersonas[0]);

                Assert.IsNotNull(
                    persona);

                Assert.IsTrue(
                    persona.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void PersonaConsulta_ObtenerPorIds_ConservaPersonaInactiva()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                DesactivarPersona(
                    datos.IdPersonas[0]);

                Persona persona =
                    CrearPersonaRepository()
                        .ObtenerPorIds(
                            new[]
                            {
                                datos.IdPersonas[0]
                            })
                        .Single();

                Assert.IsFalse(
                    persona.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void InsertarYObtener_ViajeValido_PersisteDatos()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    datos.CrearViaje(
                        new[]
                        {
                            0
                        });

                datos.IdViaje =
                    repository.Insertar(
                        viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    datos.Descripcion,
                    recuperado.Descripcion);

                Assert.AreEqual(
                    datos.FechaInicio,
                    recuperado.FechaInicio);

                Assert.AreEqual(
                    datos.FechaFin,
                    recuperado.FechaFin);

                Assert.AreEqual(
                    EstadoViaje.Abierto,
                    recuperado.EstadoActual);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Obtener_ViajePersistido_RecuperaMultiplesParticipantes()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    2);

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                datos.IdViaje =
                    repository.Insertar(
                        datos.CrearViaje(
                            new[]
                            {
                                0,
                                1
                            }));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    2,
                    recuperado.Participantes.Count);

                CollectionAssert.AreEquivalent(
                    datos.IdPersonas,
                    recuperado.Participantes
                        .Select(
                            persona =>
                                persona.IdPersona)
                        .ToList());
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ViajePersistido_ModificaDatosGenerales()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                datos.IdViaje =
                    repository.Insertar(
                        datos.CrearViaje(
                            new[]
                            {
                                0
                            }));

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.ActualizarDatos(
                    datos.FechaInicio.AddDays(1),
                    datos.FechaFin.AddDays(2),
                    datos.Descripcion +
                        " Actualizado",
                    TipoViaje.EventoFeria,
                    3500m,
                    viaje.Participantes);

                repository.Actualizar(
                    viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    datos.Descripcion +
                        " Actualizado",
                    recuperado.Descripcion);

                Assert.AreEqual(
                    TipoViaje.EventoFeria,
                    recuperado.TipoViaje);

                Assert.AreEqual(
                    3500m,
                    recuperado.MontoAnticipado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ViajePersistido_ReemplazaParticipantes()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    2);

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                datos.IdViaje =
                    repository.Insertar(
                        datos.CrearViaje(
                            new[]
                            {
                                0
                            }));

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Persona nuevaPersona =
                    ObtenerPersona(
                        datos.IdPersonas[1]);

                viaje.ReemplazarParticipantes(
                    new[]
                    {
                        nuevaPersona
                    });

                repository.Actualizar(
                    viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    1,
                    recuperado.Participantes.Count);

                Assert.AreEqual(
                    datos.IdPersonas[1],
                    recuperado
                        .Participantes
                        .Single()
                        .IdPersona);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Obtener_ViajePersistido_ConservaMontoAnticipado()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                Viaje viaje =
                    datos.CrearViaje(
                        new[]
                        {
                            0
                        },
                        4875.50m);

                datos.IdViaje =
                    CrearViajeRepository()
                        .Insertar(
                            viaje);

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    4875.50m,
                    recuperado.MontoAnticipado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Obtener_ViajePersistido_ConservaTipoViaje()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                Viaje viaje =
                    datos.CrearViaje(
                        new[]
                        {
                            0
                        },
                        0m,
                        TipoViaje.EnOficina);

                datos.IdViaje =
                    CrearViajeRepository()
                        .Insertar(
                            viaje);

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    TipoViaje.EnOficina,
                    recuperado.TipoViaje);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Obtener_ViajeCancelado_ConservaEstado()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                datos.IdViaje =
                    repository.Insertar(
                        datos.CrearViaje(
                            new[]
                            {
                                0
                            }));

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.Cancelar();

                repository.Cancelar(
                    viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Cancelado,
                    recuperado.EstadoActual);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_PorFechaDesde_DevuelveViaje()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                ViajeListadoDto resultado =
                    CrearViajeRepository()
                        .Listar(
                            new ViajeFiltro(
                                datos.FechaInicio,
                                null,
                                null,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdViaje ==
                                datos.IdViaje);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_PorFechaHasta_DevuelveViaje()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                ViajeListadoDto resultado =
                    CrearViajeRepository()
                        .Listar(
                            new ViajeFiltro(
                                null,
                                datos.FechaInicio,
                                null,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdViaje ==
                                datos.IdViaje);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_PorEstado_DevuelveViaje()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                ViajeListadoDto resultado =
                    CrearViajeRepository()
                        .Listar(
                            new ViajeFiltro(
                                null,
                                null,
                                EstadoViaje.Abierto,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdViaje ==
                                datos.IdViaje);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_PorParticipante_DevuelveViaje()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                ViajeListadoDto resultado =
                    CrearViajeRepository()
                        .Listar(
                            new ViajeFiltro(
                                null,
                                null,
                                null,
                                datos.IdPersonas[0]))
                        .SingleOrDefault(
                            actual =>
                                actual.IdViaje ==
                                datos.IdViaje);

                Assert.IsNotNull(
                    resultado);

                StringAssert.Contains(
                    resultado.ParticipantesResumen,
                    datos.ApellidoBase);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_ViajeAbierto_PersisteEstadoCancelado()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.Cancelar();

                repository.Cancelar(
                    viaje);

                Assert.AreEqual(
                    EstadoViaje.Cancelado,
                    repository
                        .ObtenerPorId(
                            datos.IdViaje)
                        .EstadoActual);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ViajeParticipante_AsociacionDuplicada_RechazaOperacion()
        {
            DatosViajePrueba datos =
                CrearEInsertarViaje();

            try
            {
                Assert.ThrowsException<SqlException>(
                    () => InsertarAsociacionDirecta(
                        datos.IdViaje,
                        datos.IdPersonas[0]));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ParticipanteInexistente_RevierteTodoElViaje()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            string descripcion =
                datos.Descripcion +
                " Rollback";

            try
            {
                Persona inexistente =
                    new Persona(
                        int.MaxValue,
                        "Persona",
                        "Inexistente",
                        "inexistente@sigevip.test");

                Viaje viaje =
                    new Viaje(
                        0,
                        datos.FechaInicio,
                        datos.FechaFin,
                        descripcion,
                        TipoViaje.Desplazamiento,
                        0m);

                viaje.ReemplazarParticipantes(
                    new[]
                    {
                        ObtenerPersona(
                            datos.IdPersonas[0]),
                        inexistente
                    });

                Assert.ThrowsException<PersistenciaException>(
                    () => CrearViajeRepository()
                        .Insertar(
                            viaje));

                Assert.AreEqual(
                    0,
                    ContarViajesPorDescripcion(
                        descripcion));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static ViajeRepository CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static PersonaConsultaRepository
            CrearPersonaRepository()
        {
            return new PersonaConsultaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosViajePrueba CrearEInsertarViaje()
        {
            DatosViajePrueba datos =
                CrearDatosPrueba(
                    1);

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        datos.CrearViaje(
                            new[]
                            {
                                0
                            }));

            return datos;
        }

        private static DatosViajePrueba CrearDatosPrueba(
            int cantidadPersonas)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosViajePrueba(
                    "Viaje Integración " +
                        sufijo,
                    "Apellido" +
                        sufijo.Substring(
                            0,
                            8),
                    new DateTime(
                        2030,
                        5,
                        10),
                    new DateTime(
                        2030,
                        5,
                        15));

            for (int indice = 0;
                indice < cantidadPersonas;
                indice++)
            {
                int idPersona =
                    InsertarPersona(
                        "Persona" +
                            indice,
                        datos.ApellidoBase +
                            indice,
                        "persona_" +
                            indice +
                            "_" +
                            sufijo +
                            "@sigevip.test");

                datos.IdPersonas.Add(
                    idPersona);
            }

            return datos;
        }

        private static int InsertarPersona(
            string nombre,
            string apellido,
            string email)
        {
            const string sql = @"
INSERT INTO dbo.Persona
(
    Nombre,
    Apellido,
    Email,
    Activo
)
VALUES
(
    @Nombre,
    @Apellido,
    @Email,
    1
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombre;

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        apellido;

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static Persona ObtenerPersona(
            int idPersona)
        {
            return CrearPersonaRepository()
                .ObtenerPorIds(
                    new[]
                    {
                        idPersona
                    })
                .Single();
        }

        private static void DesactivarPersona(
            int idPersona)
        {
            const string sql = @"
UPDATE dbo.Persona
SET Activo = 0
WHERE IdPersona = @IdPersona;";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void InsertarAsociacionDirecta(
            int idViaje,
            int idPersona)
        {
            const string sql = @"
INSERT INTO dbo.ViajeParticipante
(
    IdViaje,
    IdPersona
)
VALUES
(
    @IdViaje,
    @IdPersona
);";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        idViaje;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static int ContarViajesPorDescripcion(
            string descripcion)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Viaje
WHERE Descripcion = @Descripcion;";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        descripcion;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void EliminarDatosPrueba(
            DatosViajePrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            {
                connection.Open();

                using (
                    SqlTransaction transaction =
                        connection.BeginTransaction())
                {
                    try
                    {
                        if (datos.IdViaje > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.Viaje
WHERE IdViaje = @IdViaje;",
                                "@IdViaje",
                                datos.IdViaje);
                        }

                        foreach (
                            int idPersona
                            in datos.IdPersonas)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.ViajeParticipante
WHERE IdPersona = @IdPersona;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona;",
                                "@IdPersona",
                                idPersona);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void EjecutarEliminacion(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql,
            string nombreParametro,
            int valor)
        {
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    nombreParametro,
                    SqlDbType.Int).Value =
                        valor;

                command.ExecuteNonQuery();
            }
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings["SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                Assert.Fail(
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosViajePrueba
        {
            public DatosViajePrueba(
                string descripcion,
                string apellidoBase,
                DateTime fechaInicio,
                DateTime fechaFin)
            {
                Descripcion = descripcion;
                ApellidoBase = apellidoBase;
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                IdPersonas =
                    new List<int>();
            }

            public int IdViaje { get; set; }

            public string Descripcion { get; private set; }

            public string ApellidoBase { get; private set; }

            public DateTime FechaInicio { get; private set; }

            public DateTime FechaFin { get; private set; }

            public List<int> IdPersonas { get; private set; }

            public Viaje CrearViaje(
                IEnumerable<int> indicesPersonas,
                decimal montoAnticipado = 1250m,
                TipoViaje tipoViaje =
                    TipoViaje.Desplazamiento)
            {
                var viaje =
                    new Viaje(
                        0,
                        FechaInicio,
                        FechaFin,
                        Descripcion,
                        tipoViaje,
                        montoAnticipado);

                List<Persona> participantes =
                    indicesPersonas
                        .Select(
                            indice =>
                                ObtenerPersona(
                                    IdPersonas[
                                        indice]))
                        .ToList();

                viaje.ReemplazarParticipantes(
                    participantes);

                return viaje;
            }
        }
    }
}
