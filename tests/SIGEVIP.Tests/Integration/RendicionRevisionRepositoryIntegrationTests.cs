using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Rendiciones;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Viaticos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class RendicionRevisionRepositoryIntegrationTests
    {
        [TestMethod]
        public void ExcluirViatico_PersisteEstadoYAuditoria()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                DateTime fecha =
                    new DateTime(
                        2035,
                        6,
                        16,
                        11,
                        0,
                        0);

                viaje.ExcluirViatico(
                    viatico,
                    "Comprobante observado",
                    datos.IdUsuario,
                    fecha);

                repository.ExcluirViatico(
                    viaje,
                    viatico);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico excluido =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Excluido,
                    excluido.Estado);

                Assert.AreEqual(
                    "Comprobante observado",
                    excluido.MotivoExclusion);

                Assert.AreEqual(
                    datos.IdUsuario,
                    excluido.IdUsuarioExclusion);

                Assert.AreEqual(
                    fecha,
                    excluido.FechaExclusion);

                Assert.AreEqual(
                    0m,
                    recuperado.TotalGastado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ReactivarViatico_PersisteAuditoriaYRestauraTotal()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                viaje.ExcluirViatico(
                    viatico,
                    "Observado",
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        10,
                        0,
                        0));

                repository.ExcluirViatico(
                    viaje,
                    viatico);

                Viaje viajeActualizado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico excluido =
                    viajeActualizado
                        .Viaticos
                        .Single();

                DateTime fechaReactivacion =
                    new DateTime(
                        2035,
                        6,
                        16,
                        12,
                        0,
                        0);

                viajeActualizado
                    .ReactivarViatico(
                        excluido,
                        datos.IdUsuario,
                        fechaReactivacion);

                repository.ReactivarViatico(
                    viajeActualizado,
                    excluido);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico reactivado =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Vigente,
                    reactivado.Estado);

                Assert.AreEqual(
                    datos.IdUsuario,
                    reactivado
                        .IdUsuarioReactivacion);

                Assert.AreEqual(
                    fechaReactivacion,
                    reactivado
                        .FechaReactivacion);

                Assert.AreEqual(
                    datos.MontoViatico,
                    recuperado.TotalGastado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void AjustarMontoAnticipado_PersisteNuevoMonto()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                viaje.AjustarMontoAnticipado(
                    1750m);

                repository
                    .AjustarMontoAnticipado(
                        viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    1750m,
                    recuperado.MontoAnticipado);

                Assert.AreEqual(
                    datos.MontoViatico -
                        1750m,
                    recuperado.SaldoPendiente);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Aprobar_PersisteEstadoYAuditoria()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                DateTime fechaAprobacion =
                    new DateTime(
                        2035,
                        6,
                        16,
                        14,
                        0,
                        0);

                viaje.Aprobar(
                    datos.IdUsuario,
                    fechaAprobacion);

                repository.Aprobar(
                    viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Aprobado,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado.IdUsuarioAprobador);

                Assert.AreEqual(
                    fechaAprobacion,
                    recuperado.FechaAprobacion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_SinVisitas_PersisteMotivoYAuditoria()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                DateTime fechaCancelacion =
                    new DateTime(
                        2035,
                        6,
                        16,
                        15,
                        0,
                        0);

                viaje.Cancelar(
                    "Rendición rechazada",
                    datos.IdUsuario,
                    fechaCancelacion);

                repository.Cancelar(
                    viaje);

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Cancelado,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    "Rendición rechazada",
                    recuperado.MotivoCancelacion);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado.IdUsuarioCancelacion);

                Assert.AreEqual(
                    fechaCancelacion,
                    recuperado.FechaCancelacion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ExcluirViatico_AgregadoDesactualizado_RechazaOperacion()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                viaje.ExcluirViatico(
                    viatico,
                    "Primera exclusión",
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        10,
                        0,
                        0));

                repository.ExcluirViatico(
                    viaje,
                    viatico);

                Viaje agregadoDesactualizado =
                    Viaje.Reconstruir(
                        viaje.IdViaje,
                        viaje.FechaInicio,
                        viaje.FechaFin,
                        viaje.Descripcion,
                        viaje.TipoViaje,
                        viaje.MontoAnticipado,
                        EstadoViaje.EnRendicion,
                        viaje.Participantes,
                        viaje.Visitas,
                        new[]
                        {
                            Viatico.Reconstruir(
                                viatico.IdViatico,
                                viaje.IdViaje,
                                viatico.Fecha,
                                viatico.Categoria,
                                viatico.MetodoPago,
                                viatico.PagadoPor,
                                viatico.Monto,
                                viatico.Descripcion,
                                viatico.Comprobante,
                                EstadoViatico.Vigente,
                                null,
                                null,
                                null,
                                null,
                                null)
                        },
                        viaje.IdUsuarioEnvioRendicion,
                        viaje.FechaEnvioRendicion,
                        null,
                        null,
                        null,
                        null,
                        null);

                Viatico viaticoDesactualizado =
                    agregadoDesactualizado
                        .Viaticos
                        .Single();

                agregadoDesactualizado
                    .ExcluirViatico(
                        viaticoDesactualizado,
                        "Segunda exclusión",
                        datos.IdUsuario,
                        new DateTime(
                            2035,
                            6,
                            16,
                            11,
                            0,
                            0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.ExcluirViatico(
                                agregadoDesactualizado,
                                viaticoDesactualizado));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static Viaje EnviarARendicion(
            RendicionRepository repository,
            DatosPrueba datos)
        {
            Viaje viaje =
                repository.ObtenerPorId(
                    datos.IdViaje);

            viaje.EnviarARendicion(
                datos.IdUsuario,
                new DateTime(
                    2035,
                    6,
                    16,
                    9,
                    0,
                    0));

            repository.Enviar(
                viaje);

            return repository.ObtenerPorId(
                datos.IdViaje);
        }

        private static DatosPrueba
            CrearDatosConViatico()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba(
                    new DateTime(
                        2035,
                        6,
                        10),
                    new DateTime(
                        2035,
                        6,
                        15),
                    1000m,
                    1400m);

            datos.IdPersona =
                InsertarPersona(
                    "Participante",
                    "Revision" +
                        sufijo.Substring(
                            0,
                            8),
                    "revision_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdUsuario =
                ObtenerIdUsuarioActivo();

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Revisión rendición " +
                        sufijo,
                    TipoViaje.Desplazamiento,
                    datos.MontoAnticipado);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    datos.CrearPersona()
                });

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);

            var viajePersistido =
                new Viaje(
                    datos.IdViaje,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Revisión rendición",
                    TipoViaje.Desplazamiento,
                    datos.MontoAnticipado);

            viajePersistido
                .ReemplazarParticipantes(
                    new[]
                    {
                        datos.CrearPersona()
                    });

            var viatico =
                new Viatico(
                    0,
                    datos.FechaInicio.AddDays(1),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    datos.MontoViatico,
                    "Alimentación",
                    null);

            viajePersistido
                .AgregarViatico(
                    viatico);

            datos.IdViatico =
                CrearViaticoRepository()
                    .Insertar(
                        viatico);

            return datos;
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

        private static ViaticoRepository
            CrearViaticoRepository()
        {
            return new ViaticoRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
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
                decimal montoAnticipado,
                decimal montoViatico)
            {
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                MontoAnticipado =
                    montoAnticipado;
                MontoViatico =
                    montoViatico;
            }

            public int IdPersona { get; set; }

            public int IdUsuario { get; set; }

            public int IdViaje { get; set; }

            public int IdViatico { get; set; }

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

            public decimal MontoViatico
            {
                get;
                private set;
            }

            public Persona CrearPersona()
            {
                return new Persona(
                    IdPersona,
                    "Participante",
                    "Revision",
                    "participante.revision@sigevip.test");
            }
        }
    }
}
