using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Reportes;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Reportes;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ReporteRepositoryIntegrationTests
    {
        [TestMethod]
        public void ConsultaProcesada_FiltrosCombinadosYResumen_DevuelveIndicadores()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                ReporteService servicio =
                    CrearServicio();

                ReporteViaticoResultadoDto
                    resultado =
                        servicio.ConsultarViaticos(
                            new ReporteViaticoFiltro(
                                datos.FechaViatico,
                                datos.FechaViatico,
                                datos.IdViaje,
                                datos.IdPersona,
                                CategoriaGasto.Transporte,
                                MetodoPago.PagoPersonal,
                                EstadoViatico.Vigente,
                                EstadoViaje.Abierto));

                Assert.AreEqual(
                    1,
                    resultado.Cantidad);

                Assert.AreEqual(
                    800m,
                    resultado.TotalRegistrado);

                Assert.AreEqual(
                    800m,
                    resultado.TotalVigente);

                Assert.AreEqual(
                    0m,
                    resultado.TotalExcluido);

                Assert.AreEqual(
                    CategoriaGasto.Transporte,
                    resultado.CategoriaMayorGasto);

                ReporteViaticoFilaDto fila =
                    resultado.Filas.Single();

                Assert.AreEqual(
                    datos.IdViaje,
                    fila.IdViaje);

                Assert.AreEqual(
                    datos.IdPersona,
                    fila.IdPersonaPagadora);

                Assert.AreEqual(
                    datos.DescripcionViaje,
                    fila.Viaje);

                ReporteViajeResumenDto resumen =
                    servicio.ObtenerResumenViaje(
                        datos.IdViaje);

                Assert.AreEqual(
                    1,
                    resumen.CantidadParticipantes);

                Assert.AreEqual(
                    0,
                    resumen.CantidadVisitas);

                Assert.AreEqual(
                    0,
                    resumen.CantidadClientesDistintos);

                Assert.AreEqual(
                    3,
                    resumen.CantidadViaticos);

                Assert.AreEqual(
                    1300m,
                    resumen.TotalRegistrado);

                Assert.AreEqual(
                    1300m,
                    resumen.TotalVigente);

                Assert.AreEqual(
                    1000m,
                    resumen.GastosComputablesSaldo);

                Assert.AreEqual(
                    500m,
                    resumen.DiferenciaAnticipo);

                Assert.AreEqual(
                    "Importe a devolver",
                    resumen.TipoDiferencia);

                ReporteAnalisisResultadoDto analisis =
                    servicio.ConsultarAnalisis(
                        new ReporteAnalisisFiltro(
                            datos.FechaViatico,
                            datos.FechaViatico,
                            ReporteAreaAnalisis.Gastos,
                            ReporteIndicadorAnalisis.Importe,
                            ReporteAgrupacionAnalisis.Categoria,
                            10));

                Assert.AreEqual(
                    1300m,
                    analisis.Total);

                Assert.AreEqual(
                    3,
                    analisis.Items.Count);

                Assert.AreEqual(
                    "Transporte",
                    analisis.ElementoDestacado);

                Assert.AreEqual(
                    800m,
                    analisis.ValorDestacado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static ReporteService
            CrearServicio()
        {
            return new ReporteService(
                new ReporteRepository(
                    new SqlConnectionFactory(
                        ObtenerConnectionString())),
                new SesionActualFalsa(
                    CrearUsuario()),
                new AutorizacionService());
        }

        private static Usuario CrearUsuario()
        {
            var usuario =
                new Usuario(
                    1,
                    1,
                    "reporte.integracion",
                    new byte[]
                    {
                        1
                    },
                    new byte[]
                    {
                        2
                    },
                    100000);

            var grupo =
                new Grupo(
                    1,
                    "REPORTES_INTEGRACION",
                    "Reportes integración",
                    string.Empty);

            grupo.AgregarComponente(
                new Permiso(
                    1,
                    ViajeService.PermisoConsultar,
                    "Consultar Viajes",
                    string.Empty));

            grupo.AgregarComponente(
                new Permiso(
                    2,
                    ViaticoService.PermisoConsultar,
                    "Consultar Viáticos",
                    string.Empty));

            usuario.AgregarGrupo(
                grupo);

            return usuario;
        }

        private static DatosPrueba
            CrearDatosPrueba()
        {
            string marca =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba
                {
                    DescripcionViaje =
                        "Reporte integración "
                        + marca,

                    EmailPersona =
                        "reporte_"
                        + marca
                        + "@sigevip.test",

                    FechaViatico =
                        new DateTime(
                            2038,
                            4,
                            12)
                };

            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO dbo.Persona
(
    Nombre,
    Apellido,
    Email,
    Activo
)
VALUES
(
    N'Persona',
    N'Reporte Integración',
    @Email,
    1
);

DECLARE @IdPersona INT =
    CAST(
        SCOPE_IDENTITY()
        AS INT
    );

INSERT INTO dbo.Viaje
(
    FechaInicio,
    FechaFin,
    Descripcion,
    TipoViaje,
    MontoAnticipado,
    EstadoViaje
)
VALUES
(
    @FechaInicio,
    @FechaFin,
    @DescripcionViaje,
    @TipoViaje,
    1500.00,
    @EstadoViaje
);

DECLARE @IdViaje INT =
    CAST(
        SCOPE_IDENTITY()
        AS INT
    );

INSERT INTO dbo.ViajeParticipante
(
    IdViaje,
    IdPersona
)
VALUES
(
    @IdViaje,
    @IdPersona
);

INSERT INTO dbo.Viatico
(
    IdViaje,
    Fecha,
    CategoriaGasto,
    MetodoPago,
    IdPersonaPagadora,
    Monto,
    Descripcion,
    EstadoViatico
)
VALUES
(
    @IdViaje,
    @FechaViatico,
    @CategoriaTransporte,
    @MetodoPersonal,
    @IdPersona,
    800.00,
    N'Traslado de integración',
    @EstadoVigente
),
(
    @IdViaje,
    @FechaViatico,
    @CategoriaOtros,
    @MetodoEmpresa,
    NULL,
    200.00,
    N'Gasto de empresa de integración',
    @EstadoVigente
),
(
    @IdViaje,
    @FechaViatico,
    @CategoriaAlojamiento,
    @MetodoCorporativo,
    @IdPersona,
    300.00,
    N'Gasto con tarjeta corporativa',
    @EstadoVigente
);

COMMIT TRANSACTION;

SELECT
    @IdPersona AS IdPersona,
    @IdViaje AS IdViaje;";

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
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        datos.EmailPersona;

                command.Parameters.Add(
                    "@FechaInicio",
                    SqlDbType.Date).Value =
                        datos.FechaViatico
                            .AddDays(-1);

                command.Parameters.Add(
                    "@FechaFin",
                    SqlDbType.Date).Value =
                        datos.FechaViatico
                            .AddDays(1);

                command.Parameters.Add(
                    "@DescripcionViaje",
                    SqlDbType.NVarChar,
                    500).Value =
                        datos.DescripcionViaje;

                command.Parameters.Add(
                    "@TipoViaje",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            TipoViaje
                                .Desplazamiento);

                command.Parameters.Add(
                    "@EstadoViaje",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            EstadoViaje
                                .Abierto);

                command.Parameters.Add(
                    "@FechaViatico",
                    SqlDbType.Date).Value =
                        datos.FechaViatico;

                command.Parameters.Add(
                    "@CategoriaTransporte",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            CategoriaGasto
                                .Transporte);

                command.Parameters.Add(
                    "@CategoriaOtros",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            CategoriaGasto
                                .Otros);

                command.Parameters.Add(
                    "@CategoriaAlojamiento",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            CategoriaGasto
                                .Alojamiento);

                command.Parameters.Add(
                    "@MetodoPersonal",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            MetodoPago
                                .PagoPersonal);

                command.Parameters.Add(
                    "@MetodoEmpresa",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            MetodoPago
                                .EfectivoEmpresa);

                command.Parameters.Add(
                    "@MetodoCorporativo",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            MetodoPago
                                .TarjetaCorporativa);

                command.Parameters.Add(
                    "@EstadoVigente",
                    SqlDbType.TinyInt).Value =
                        Convert.ToByte(
                            EstadoViatico
                                .Vigente);

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    datos.IdPersona =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdPersona"));

                    datos.IdViaje =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdViaje"));
                }
            }

            return datos;
        }

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.Comprobante
WHERE IdViatico IN
(
    SELECT IdViatico
    FROM dbo.Viatico
    WHERE IdViaje = @IdViaje
);

DELETE FROM dbo.Viatico
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.VisitaCliente
WHERE IdVisita IN
(
    SELECT IdVisita
    FROM dbo.Visita
    WHERE IdViaje = @IdViaje
);

DELETE FROM dbo.Visita
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.Viaje
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.Persona
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
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        datos.IdViaje;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        datos.IdPersona;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string
            ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings["SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                throw new AssertInconclusiveException(
                    "No se encontró la cadena de conexión SIGEVIP para las pruebas de integración.");
            }

            return settings.ConnectionString;
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

        private sealed class DatosPrueba
        {
            public int IdPersona
            {
                get;
                set;
            }

            public int IdViaje
            {
                get;
                set;
            }

            public string EmailPersona
            {
                get;
                set;
            }

            public string DescripcionViaje
            {
                get;
                set;
            }

            public DateTime FechaViatico
            {
                get;
                set;
            }
        }
    }
}
