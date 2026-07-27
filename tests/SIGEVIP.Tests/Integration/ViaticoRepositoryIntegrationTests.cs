using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Viaticos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ViaticoRepositoryIntegrationTests
    {
        [TestMethod]
        public void InsertarSinComprobante_PersisteViatico()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                Viatico viatico =
                    datos.CrearViaticoSinComprobante();

                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            viatico);

                Assert.IsTrue(
                    datos.IdViatico > 0);

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    datos.IdViaje,
                    recuperado.IdViaje);

                Assert.AreEqual(
                    1250m,
                    recuperado.Monto);

                Assert.IsFalse(
                    recuperado.TieneComprobante);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void InsertarConComprobante_PersisteDatosCompletos()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoConComprobante());

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNotNull(
                    recuperado.Comprobante);

                Assert.IsTrue(
                    recuperado.Comprobante
                        .IdComprobante > 0);

                Assert.AreEqual(
                    TipoComprobante.FacturaB,
                    recuperado.Comprobante.Tipo);

                Assert.AreEqual(
                    1500m,
                    recuperado.Comprobante.Total);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void InsertarPagoPersonal_ReconstruyePagador()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoPagoPersonal());

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNotNull(
                    recuperado.PagadoPor);

                Assert.AreEqual(
                    datos.IdPersona,
                    recuperado.PagadoPor.IdPersona);

                Assert.AreEqual(
                    MetodoPago.PagoPersonal,
                    recuperado.MetodoPago);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_Inexistente_DevuelveNull()
        {
            Viatico resultado =
                CrearRepository()
                    .ObtenerPorId(
                        int.MaxValue);

            Assert.IsNull(
                resultado);
        }

        [TestMethod]
        public void ObtenerPorId_IdInvalido_LanzaExcepcion()
        {
            Assert.ThrowsException<
                ArgumentOutOfRangeException>(
                    () => CrearRepository()
                        .ObtenerPorId(
                            0));
        }

        [TestMethod]
        public void ListarPorViaje_AplicaFiltroCategoria()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoSinComprobante());

                int segundoId =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoConComprobante());

                datos.IdsViaticos.Add(
                    segundoId);

                IReadOnlyCollection<ViaticoListadoDto>
                    resultados =
                        CrearRepository()
                            .ListarPorViaje(
                                datos.IdViaje,
                                new ViaticoFiltro(
                                    null,
                                    null,
                                    CategoriaGasto.Alimentacion,
                                    null));

                Assert.AreEqual(
                    1,
                    resultados.Count);

                Assert.AreEqual(
                    datos.IdViatico,
                    resultados.Single()
                        .IdViatico);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_AgregaComprobante()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoSinComprobante());

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                datos.CrearViajeDominio(
                        recuperado)
                    .ModificarViatico(
                        recuperado,
                        datos.FechaViatico,
                        CategoriaGasto.Alojamiento,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        2200m,
                        "Alojamiento actualizado",
                        datos.CrearComprobante());

                CrearRepository()
                    .Actualizar(
                        recuperado);

                Viatico actualizado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.AreEqual(
                    2200m,
                    actualizado.Monto);

                Assert.IsNotNull(
                    actualizado.Comprobante);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_QuitaComprobante()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            datos.CrearViaticoConComprobante());

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                datos.CrearViajeDominio(
                        recuperado)
                    .ModificarViatico(
                        recuperado,
                        datos.FechaViatico,
                        CategoriaGasto.Otros,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        1750m,
                        "Gasto sin comprobante",
                        null);

                CrearRepository()
                    .Actualizar(
                        recuperado);

                Viatico actualizado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNull(
                    actualizado.Comprobante);

                Assert.AreEqual(
                    0,
                    ContarComprobantes(
                        datos.IdViatico));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ViajeInexistente_RevierteViatico()
        {
            DatosViaticoPrueba datos =
                CrearDatosPrueba();

            try
            {
                string descripcion =
                    "Rollback " +
                    Guid.NewGuid()
                        .ToString("N");

                var viajeInexistente =
                    new Viaje(
                        int.MaxValue,
                        datos.FechaInicio,
                        datos.FechaFin,
                        "Viaje inexistente",
                        TipoViaje.Desplazamiento,
                        0m);

                var viatico =
                    new Viatico(
                        0,
                        datos.FechaViatico,
                        CategoriaGasto.Otros,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        900m,
                        descripcion,
                        null);

                viajeInexistente
                    .AgregarViatico(
                        viatico);

                Assert.ThrowsException<
                    PersistenciaException>(
                        () => CrearRepository()
                            .Insertar(
                                viatico));

                Assert.AreEqual(
                    0,
                    ContarViaticosPorDescripcion(
                        descripcion));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static ViaticoRepository
            CrearRepository()
        {
            return new ViaticoRepository(
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

        private static DatosViaticoPrueba
            CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosViaticoPrueba(
                    new DateTime(
                        2032,
                        8,
                        10),
                    new DateTime(
                        2032,
                        8,
                        15),
                    new DateTime(
                        2032,
                        8,
                        12));

            datos.IdPersona =
                InsertarPersona(
                    "Pagador",
                    "Integracion" +
                        sufijo.Substring(
                            0,
                            8),
                    "pagador_" +
                        sufijo +
                        "@sigevip.test");

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje viáticos " +
                        sufijo,
                    TipoViaje.Desplazamiento,
                    0m);

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

        private static int ContarComprobantes(
            int idViatico)
        {
            return EjecutarConteo(
                @"
SELECT COUNT(*)
FROM dbo.Comprobante
WHERE IdViatico = @Valor;",
                idViatico);
        }

        private static int ContarViaticosPorDescripcion(
            string descripcion)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Viatico
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
                    1000).Value =
                        descripcion;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int EjecutarConteo(
            string sql,
            int valor)
        {
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
                    "@Valor",
                    SqlDbType.Int).Value =
                        valor;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void EliminarDatosPrueba(
            DatosViaticoPrueba datos)
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

        private sealed class DatosViaticoPrueba
        {
            public DatosViaticoPrueba(
                DateTime fechaInicio,
                DateTime fechaFin,
                DateTime fechaViatico)
            {
                FechaInicio =
                    fechaInicio;

                FechaFin =
                    fechaFin;

                FechaViatico =
                    fechaViatico;

                IdsViaticos =
                    new List<int>();
            }

            public int IdPersona { get; set; }

            public int IdViaje { get; set; }

            public int IdViatico
            {
                get
                {
                    return IdsViaticos.Count == 0
                        ? 0
                        : IdsViaticos[0];
                }
                set
                {
                    if (IdsViaticos.Count == 0)
                    {
                        IdsViaticos.Add(
                            value);
                    }
                    else
                    {
                        IdsViaticos[0] =
                            value;
                    }
                }
            }

            public List<int> IdsViaticos
            {
                get;
                private set;
            }

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

            public DateTime FechaViatico
            {
                get;
                private set;
            }

            public Persona CrearPersona()
            {
                return new Persona(
                    IdPersona,
                    "Pagador",
                    "Integracion",
                    "pagador.integracion@sigevip.test");
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

            public Viatico CrearViaticoSinComprobante()
            {
                var viatico =
                    new Viatico(
                        0,
                        FechaViatico,
                        CategoriaGasto.Alimentacion,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        1250m,
                        "Alimentación sin comprobante",
                        null);

                CrearViajeDominio(
                        null)
                    .AgregarViatico(
                        viatico);

                return viatico;
            }

            public Viatico CrearViaticoConComprobante()
            {
                var viatico =
                    new Viatico(
                        0,
                        FechaViatico.AddDays(1),
                        CategoriaGasto.Alojamiento,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        1500m,
                        string.Empty,
                        CrearComprobante());

                CrearViajeDominio(
                        null)
                    .AgregarViatico(
                        viatico);

                return viatico;
            }

            public Viatico CrearViaticoPagoPersonal()
            {
                var viatico =
                    new Viatico(
                        0,
                        FechaViatico,
                        CategoriaGasto.Transporte,
                        MetodoPago.PagoPersonal,
                        CrearPersona(),
                        800m,
                        "Taxi",
                        null);

                CrearViajeDominio(
                        null)
                    .AgregarViatico(
                        viatico);

                return viatico;
            }

            public Viaje CrearViajeDominio(
                Viatico viatico)
            {
                var viaje =
                    new Viaje(
                        IdViaje,
                        FechaInicio,
                        FechaFin,
                        "Viaje reconstruido para prueba",
                        TipoViaje.Desplazamiento,
                        0m);

                viaje.ReemplazarParticipantes(
                    new[]
                    {
                        CrearPersona()
                    });

                if (viatico != null)
                {
                    viaje.AgregarViatico(
                        viatico);
                }

                return viaje;
            }
        }
    }
}
