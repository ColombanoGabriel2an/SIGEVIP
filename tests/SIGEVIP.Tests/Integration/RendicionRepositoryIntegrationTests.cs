using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Rendiciones;
using SIGEVIP.Infrastructure.Viajes;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class RendicionRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerPorId_ViajeExistente_DevuelveAgregado()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                Viaje resultado =
                    CrearRendicionRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.IdViaje,
                    resultado.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Abierto,
                    resultado.EstadoActual);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Enviar_ViajeAbierto_PersisteEstadoYAuditoria()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                Viaje viaje =
                    CrearRendicionRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                DateTime fechaEnvio =
                    new DateTime(
                        2034,
                        5,
                        16,
                        10,
                        30,
                        0);

                viaje.EnviarARendicion(
                    datos.IdUsuario,
                    fechaEnvio);

                CrearRendicionRepository()
                    .Enviar(
                        viaje);

                Viaje recuperado =
                    CrearRendicionRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado
                        .IdUsuarioEnvioRendicion);

                Assert.AreEqual(
                    fechaEnvio,
                    recuperado
                        .FechaEnvioRendicion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Enviar_ViajeSinViaticos_PersisteCorrectamente()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                Viaje viaje =
                    CrearRendicionRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    0,
                    viaje.Viaticos.Count);

                viaje.EnviarARendicion(
                    datos.IdUsuario,
                    DateTime.Now);

                CrearRendicionRepository()
                    .Enviar(
                        viaje);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    CrearRendicionRepository()
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
        public void ListarPendientes_IncluyeViajeEnRendicion()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                Viaje viaje =
                    CrearRendicionRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                viaje.EnviarARendicion(
                    datos.IdUsuario,
                    new DateTime(
                        2034,
                        5,
                        16,
                        9,
                        0,
                        0));

                CrearRendicionRepository()
                    .Enviar(
                        viaje);

                RendicionListadoDto resultado =
                    CrearRendicionRepository()
                        .ListarPendientes()
                        .SingleOrDefault(
                            item =>
                                item.IdViaje ==
                                datos.IdViaje);

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.MontoAnticipado,
                    resultado.MontoAnticipado);

                Assert.AreEqual(
                    0m,
                    resultado.TotalGastado);

                Assert.AreEqual(
                    -datos.MontoAnticipado,
                    resultado.Saldo);

                Assert.IsTrue(
                    resultado
                        .FechaEnvioRendicion
                        .HasValue);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarPendientes_NoIncluyeViajeAbierto()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                bool encontrado =
                    CrearRendicionRepository()
                        .ListarPendientes()
                        .Any(
                            item =>
                                item.IdViaje ==
                                datos.IdViaje);

                Assert.IsFalse(
                    encontrado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Enviar_AgregadoDesactualizado_RechazaSegundoEnvio()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje primerAgregado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viaje segundoAgregado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                primerAgregado.EnviarARendicion(
                    datos.IdUsuario,
                    new DateTime(
                        2034,
                        5,
                        16,
                        10,
                        0,
                        0));

                repository.Enviar(
                    primerAgregado);

                segundoAgregado.EnviarARendicion(
                    datos.IdUsuario,
                    new DateTime(
                        2034,
                        5,
                        16,
                        11,
                        0,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () => repository.Enviar(
                            segundoAgregado));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    new DateTime(
                        2034,
                        5,
                        16,
                        10,
                        0,
                        0),
                    recuperado
                        .FechaEnvioRendicion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Enviar_ConAuditoriaValida_PersisteEstadoYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                DateTime fechaEnvio =
                    new DateTime(
                        2034,
                        5,
                        16,
                        14,
                        30,
                        0);

                viaje.EnviarARendicion(
                    datos.IdUsuario,
                    fechaEnvio);

                AuditoriaRegistro auditoria =
                    CrearAuditoriaValida(
                        datos);

                repository.Enviar(
                    viaje,
                    auditoria);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado
                        .IdUsuarioEnvioRendicion);

                Assert.AreEqual(
                    fechaEnvio,
                    recuperado
                        .FechaEnvioRendicion);

                Assert.AreEqual(
                    1,
                    ContarAuditoriasEnvio(
                        datos));

                Assert.AreEqual(
                    datos.IdUsuario,
                    ObtenerIdUsuarioAuditoria(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Enviar_ConActorInexistente_RevierteEstadoYDatosDeEnvio()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.EnviarARendicion(
                    datos.IdUsuario,
                    new DateTime(
                        2034,
                        5,
                        16,
                        15,
                        0,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () => repository.Enviar(
                            viaje,
                            CrearAuditoriaInvalida(
                                datos)));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    EstadoViaje.Abierto,
                    recuperado.EstadoActual);

                Assert.IsFalse(
                    recuperado
                        .IdUsuarioEnvioRendicion
                        .HasValue);

                Assert.IsFalse(
                    recuperado
                        .FechaEnvioRendicion
                        .HasValue);

                Assert.AreEqual(
                    0,
                    ContarAuditoriasEnvio(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static RendicionRepository
            CrearRendicionRepository()
        {
            return new RendicionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static ViajeRepository
            CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba
            CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba(
                    new DateTime(
                        2034,
                        5,
                        10),
                    new DateTime(
                        2034,
                        5,
                        15),
                    1250m);

            datos.Marca =
                "RENDICION_AUDIT_TEST_" +
                sufijo;

            datos.IdPersona =
                InsertarPersona(
                    "Participante",
                    "Rendicion" +
                        sufijo.Substring(
                            0,
                            8),
                    "participante_rendicion_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdUsuario =
                ObtenerIdUsuarioActivo();

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Rendición integración " +
                        sufijo,
                    TipoViaje.Desplazamiento,
                    datos.MontoAnticipado);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    new Persona(
                        datos.IdPersona,
                        "Participante",
                        "Rendicion",
                        "participante.rendicion@sigevip.test")
                });

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);

            return datos;
        }

        private static AuditoriaRegistro
            CrearAuditoriaValida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                datos.IdUsuario,
                "usuario_integracion_rendicion",
                "Rendiciones",
                "EnvioARendicion",
                "Viaje",
                datos.IdViaje,
                datos.Marca +
                    " envio valido");
        }

        private static AuditoriaRegistro
            CrearAuditoriaInvalida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "actor_rendicion_inexistente",
                "Rendiciones",
                "EnvioARendicion",
                "Viaje",
                datos.IdViaje,
                datos.Marca +
                    " envio invalido");
        }

        private static int ContarAuditoriasEnvio(
            DatosPrueba datos)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Accion = N'EnvioARendicion'
    AND Entidad = N'Viaje'
    AND IdEntidad = @IdViaje
    AND Descripcion LIKE @Marca;";

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
                        datos.IdViaje;

                command.Parameters.Add(
                    "@Marca",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "%" +
                        datos.Marca +
                        "%";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int ObtenerIdUsuarioAuditoria(
            DatosPrueba datos)
        {
            const string sql = @"
SELECT TOP (1)
    IdUsuario
FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Accion = N'EnvioARendicion'
    AND Entidad = N'Viaje'
    AND IdEntidad = @IdViaje
    AND Descripcion LIKE @Marca
ORDER BY IdAuditoria DESC;";

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
                        datos.IdViaje;

                command.Parameters.Add(
                    "@Marca",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "%" +
                        datos.Marca +
                        "%";

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                Assert.IsNotNull(
                    resultado);

                Assert.AreNotEqual(
                    DBNull.Value,
                    resultado);

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static int ObtenerIdUsuarioActivo()
        {
            const string sql = @"
SELECT TOP (1)
    IdUsuario
FROM dbo.Usuario
WHERE Activo = 1
ORDER BY IdUsuario;";

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
                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "La base de integración debe contener al menos un usuario activo.");
                }

                return Convert.ToInt32(
                    resultado);
            }
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

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
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
DELETE FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Accion = N'EnvioARendicion'
    AND Entidad = N'Viaje'
    AND IdEntidad = @Valor;

DELETE FROM dbo.Comprobante
WHERE IdViatico IN
(
    SELECT IdViatico
    FROM dbo.Viatico
    WHERE IdViaje = @Valor
);

DELETE FROM dbo.Viatico
WHERE IdViaje = @Valor;

DELETE FROM dbo.VisitaCliente
WHERE IdVisita IN
(
    SELECT IdVisita
    FROM dbo.Visita
    WHERE IdViaje = @Valor
);

DELETE FROM dbo.Visita
WHERE IdViaje = @Valor;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @Valor;

DELETE FROM dbo.Viaje
WHERE IdViaje = @Valor;",
                                datos.IdViaje);
                        }

                        if (datos.IdPersona > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.ViajeParticipante
WHERE IdPersona = @Valor;

DELETE FROM dbo.Persona
WHERE IdPersona = @Valor;",
                                datos.IdPersona);
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
                    "@Valor",
                    SqlDbType.Int).Value =
                        valor;

                command.ExecuteNonQuery();
            }
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings[
                        "SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                Assert.Fail(
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public DatosPrueba(
                DateTime fechaInicio,
                DateTime fechaFin,
                decimal montoAnticipado)
            {
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                MontoAnticipado =
                    montoAnticipado;
            }

            public int IdPersona { get; set; }

            public int IdUsuario { get; set; }

            public int IdViaje { get; set; }

            public string Marca { get; set; }

            public DateTime FechaInicio
            {
                get;
                private set;
            }

            public DateTime FechaFin
            {
                get;
                private set;
            }

            public decimal MontoAnticipado
            {
                get;
                private set;
            }
        }
    }
}
