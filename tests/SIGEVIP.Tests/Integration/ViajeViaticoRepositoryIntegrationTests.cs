using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Viaticos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ViajeViaticoRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerPorId_ViajeConDosViaticos_ReconstruyeColeccion()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    1000m);

            try
            {
                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            0,
                            600m,
                            CategoriaGasto.Alimentacion,
                            null)));

                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            1,
                            900m,
                            CategoriaGasto.Alojamiento,
                            datos.CrearComprobante())));

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    2,
                    recuperado.Viaticos.Count);

                CollectionAssert.AreEquivalent(
                    datos.IdsViaticos,
                    recuperado.Viaticos
                        .Select(
                            viatico =>
                                viatico.IdViatico)
                        .ToList());
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_ViaticoConComprobante_ReconstruyeComprobante()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    0m);

            try
            {
                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            0,
                            1500m,
                            CategoriaGasto.Alojamiento,
                            datos.CrearComprobante())));

                Viatico recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje)
                        .Viaticos
                        .Single();

                Assert.IsTrue(
                    recuperado.TieneComprobante);

                Assert.IsNotNull(
                    recuperado.Comprobante);

                Assert.IsTrue(
                    recuperado
                        .Comprobante
                        .IdComprobante > 0);

                Assert.AreEqual(
                    TipoComprobante.FacturaB,
                    recuperado
                        .Comprobante
                        .Tipo);

                Assert.AreEqual(
                    "30712345678",
                    recuperado
                        .Comprobante
                        .CuitProveedor);

                Assert.AreEqual(
                    1500m,
                    recuperado
                        .Comprobante
                        .Total);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_PagadorHistoricoInactivo_ConservaEstado()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    0m);

            try
            {
                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoPagoPersonal(
                            0,
                            800m)));

                DesactivarPersona(
                    datos.IdPersona);

                Viatico recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje)
                        .Viaticos
                        .Single();

                Assert.IsNotNull(
                    recuperado.PagadoPor);

                Assert.AreEqual(
                    datos.IdPersona,
                    recuperado
                        .PagadoPor
                        .IdPersona);

                Assert.IsFalse(
                    recuperado
                        .PagadoPor
                        .Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_ViaticosVigentes_CalculaTotalYSaldo()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    1000m);

            try
            {
                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            0,
                            600m,
                            CategoriaGasto.Alimentacion,
                            null)));

                datos.IdsViaticos.Add(
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            1,
                            900m,
                            CategoriaGasto.Transporte,
                            null)));

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    1500m,
                    recuperado.TotalGastado);

                Assert.AreEqual(
                    500m,
                    recuperado.SaldoPendiente);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }


        [TestMethod]
        public void ObtenerPorId_ViajeEnRendicion_ConservaAuditoriaEnvio()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    0m);

            try
            {
                int idUsuario =
                    ObtenerIdUsuarioExistente();

                DateTime fechaEnvio =
                    new DateTime(
                        2033,
                        4,
                        16,
                        10,
                        30,
                        0);

                MarcarViajeEnRendicion(
                    datos.IdViaje,
                    idUsuario,
                    fechaEnvio);

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    idUsuario,
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
        public void ObtenerPorId_ViaticoExcluido_ConservaAuditoriaYNoSumaTotal()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    1000m);

            try
            {
                int idViaticoVigente =
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            0,
                            600m,
                            CategoriaGasto.Alimentacion,
                            null));

                datos.IdsViaticos.Add(
                    idViaticoVigente);

                int idViaticoExcluido =
                    InsertarViatico(
                        datos,
                        datos.CrearViaticoEfectivo(
                            1,
                            900m,
                            CategoriaGasto.Transporte,
                            null));

                datos.IdsViaticos.Add(
                    idViaticoExcluido);

                int idUsuario =
                    ObtenerIdUsuarioExistente();

                DateTime fechaEnvio =
                    new DateTime(
                        2033,
                        4,
                        16,
                        9,
                        0,
                        0);

                DateTime fechaExclusion =
                    new DateTime(
                        2033,
                        4,
                        16,
                        11,
                        15,
                        0);

                const string motivo =
                    "Comprobante observado durante la revisión.";

                MarcarViajeEnRendicion(
                    datos.IdViaje,
                    idUsuario,
                    fechaEnvio);

                ExcluirViaticoDirectamente(
                    idViaticoExcluido,
                    idUsuario,
                    fechaExclusion,
                    motivo);

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Viatico excluido =
                    recuperado
                        .Viaticos
                        .Single(
                            viatico =>
                                viatico.IdViatico ==
                                idViaticoExcluido);

                Assert.AreEqual(
                    EstadoViatico.Excluido,
                    excluido.Estado);

                Assert.AreEqual(
                    motivo,
                    excluido.MotivoExclusion);

                Assert.AreEqual(
                    idUsuario,
                    excluido.IdUsuarioExclusion);

                Assert.AreEqual(
                    fechaExclusion,
                    excluido.FechaExclusion);

                Assert.AreEqual(
                    600m,
                    recuperado.TotalGastado);

                Assert.AreEqual(
                    -400m,
                    recuperado.SaldoPendiente);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
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

        private static DatosPrueba CrearDatosPrueba(
            decimal montoAnticipado)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba(
                    "Pagador",
                    "ViajeViatico" +
                        sufijo.Substring(
                            0,
                            8),
                    "pagador_viaje_" +
                        sufijo +
                        "@sigevip.test",
                    new DateTime(
                        2033,
                        4,
                        10),
                    new DateTime(
                        2033,
                        4,
                        15),
                    montoAnticipado);

            datos.IdPersona =
                InsertarPersona(
                    datos.Nombre,
                    datos.Apellido,
                    datos.Email);

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje con viáticos " +
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

            return datos;
        }

        private static int InsertarViatico(
            DatosPrueba datos,
            Viatico viatico)
        {
            Viaje viaje =
                datos.CrearViajeDominio();

            viaje.AgregarViatico(
                viatico);

            return CrearViaticoRepository()
                .Insertar(
                    viatico);
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


        private static int ObtenerIdUsuarioExistente()
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
                    throw new AssertFailedException(
                        "La base de integración debe contener al menos un usuario activo.");
                }

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static void MarcarViajeEnRendicion(
            int idViaje,
            int idUsuario,
            DateTime fechaEnvio)
        {
            const string sql = @"
UPDATE dbo.Viaje
SET
    EstadoViaje = @EstadoViaje,
    IdUsuarioEnvioRendicion = @IdUsuario,
    FechaEnvioRendicion = @FechaEnvio
WHERE IdViaje = @IdViaje;";

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
                    "@EstadoViaje",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            EstadoViaje.EnRendicion);

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.Parameters.Add(
                    "@FechaEnvio",
                    SqlDbType.DateTime2).Value =
                        fechaEnvio;

                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        idViaje;

                connection.Open();

                Assert.AreEqual(
                    1,
                    command.ExecuteNonQuery());
            }
        }

        private static void ExcluirViaticoDirectamente(
            int idViatico,
            int idUsuario,
            DateTime fechaExclusion,
            string motivo)
        {
            const string sql = @"
UPDATE dbo.Viatico
SET
    EstadoViatico = @EstadoViatico,
    MotivoExclusion = @MotivoExclusion,
    IdUsuarioExclusion = @IdUsuarioExclusion,
    FechaExclusion = @FechaExclusion
WHERE IdViatico = @IdViatico;";

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
                    "@EstadoViatico",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            EstadoViatico.Excluido);

                command.Parameters.Add(
                    "@MotivoExclusion",
                    SqlDbType.NVarChar,
                    500).Value =
                        motivo;

                command.Parameters.Add(
                    "@IdUsuarioExclusion",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.Parameters.Add(
                    "@FechaExclusion",
                    SqlDbType.DateTime2).Value =
                        fechaExclusion;

                command.Parameters.Add(
                    "@IdViatico",
                    SqlDbType.Int).Value =
                        idViatico;

                connection.Open();

                Assert.AreEqual(
                    1,
                    command.ExecuteNonQuery());
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
                string nombre,
                string apellido,
                string email,
                DateTime fechaInicio,
                DateTime fechaFin,
                decimal montoAnticipado)
            {
                Nombre = nombre;
                Apellido = apellido;
                Email = email;
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                MontoAnticipado =
                    montoAnticipado;

                IdsViaticos =
                    new List<int>();
            }

            public int IdPersona { get; set; }

            public int IdViaje { get; set; }

            public string Nombre { get; private set; }

            public string Apellido { get; private set; }

            public string Email { get; private set; }

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

            public List<int> IdsViaticos
            {
                get;
                private set;
            }

            public Persona CrearPersona()
            {
                return new Persona(
                    IdPersona,
                    Nombre,
                    Apellido,
                    Email);
            }

            public Viaje CrearViajeDominio()
            {
                var viaje =
                    new Viaje(
                        IdViaje,
                        FechaInicio,
                        FechaFin,
                        "Viaje reconstruido para prueba",
                        TipoViaje.Desplazamiento,
                        MontoAnticipado);

                viaje.ReemplazarParticipantes(
                    new[]
                    {
                        CrearPersona()
                    });

                return viaje;
            }

            public Comprobante CrearComprobante()
            {
                return new Comprobante(
                    0,
                    TipoComprobante.FacturaB,
                    "30712345678",
                    "Proveedor Integración",
                    SituacionFiscal.ResponsableInscripto,
                    "0001",
                    "00000001",
                    1200m,
                    300m);
            }

            public Viatico CrearViaticoEfectivo(
                int desplazamientoDias,
                decimal monto,
                CategoriaGasto categoria,
                Comprobante comprobante)
            {
                return new Viatico(
                    0,
                    FechaInicio.AddDays(
                        desplazamientoDias + 1),
                    categoria,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    monto,
                    comprobante == null
                        ? "Gasto sin comprobante"
                        : string.Empty,
                    comprobante);
            }

            public Viatico CrearViaticoPagoPersonal(
                int desplazamientoDias,
                decimal monto)
            {
                return new Viatico(
                    0,
                    FechaInicio.AddDays(
                        desplazamientoDias + 1),
                    CategoriaGasto.Transporte,
                    MetodoPago.PagoPersonal,
                    CrearPersona(),
                    monto,
                    "Traslado personal",
                    null);
            }
        }
    }
}
