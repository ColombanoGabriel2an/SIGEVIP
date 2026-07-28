using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
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
    public class ViaticoAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteViaticoComprobanteYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    true);

                Viatico viatico =
                    CrearViatico(
                        datos,
                        datos.DescripcionAlta);

                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            viatico,
                            CrearAuditoriaAltaValida(
                                datos));

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
                    1500m,
                    recuperado.Monto);

                Assert.AreEqual(
                    datos.DescripcionAlta,
                    recuperado.Descripcion);

                Assert.IsNotNull(
                    recuperado.Comprobante);

                Assert.AreEqual(
                    datos.NumeroOriginal,
                    recuperado
                        .Comprobante
                        .Numero);

                Assert.AreEqual(
                    1500m,
                    recuperado
                        .Comprobante
                        .Total);

                Assert.AreEqual(
                    1,
                    ContarAuditorias(
                        datos));

                Assert.AreEqual(
                    datos.IdViatico,
                    ObtenerIdEntidadAuditoria(
                        datos,
                        "Alta"));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_RevierteViaticoYComprobante()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    false);

                Viatico viatico =
                    CrearViatico(
                        datos,
                        datos.DescripcionAltaRollback);

                Assert.ThrowsException<
                    PersistenciaException>(
                        () => CrearRepository()
                            .Insertar(
                                viatico,
                                CrearAuditoriaAltaInvalida(
                                    datos)));

                Assert.AreEqual(
                    0,
                    ContarViaticosPorDescripcion(
                        datos.DescripcionAltaRollback));

                Assert.AreEqual(
                    0,
                    ContarComprobantesPorDescripcionViatico(
                        datos.DescripcionAltaRollback));

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConAuditoriaValida_PersisteCambiosComprobanteYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    true);

                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            CrearViatico(
                                datos,
                                datos.DescripcionOriginal));

                Viatico viatico =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                ModificarViatico(
                    datos,
                    viatico);

                CrearRepository()
                    .Actualizar(
                        viatico,
                        CrearAuditoriaModificacionValida(
                            datos));

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    2750m,
                    recuperado.Monto);

                Assert.AreEqual(
                    CategoriaGasto.Alojamiento,
                    recuperado.Categoria);

                Assert.AreEqual(
                    datos.DescripcionModificada,
                    recuperado.Descripcion);

                Assert.IsNotNull(
                    recuperado.Comprobante);

                Assert.AreEqual(
                    datos.NumeroModificado,
                    recuperado
                        .Comprobante
                        .Numero);

                Assert.AreEqual(
                    2750m,
                    recuperado
                        .Comprobante
                        .Total);

                Assert.AreEqual(
                    1,
                    ContarAuditorias(
                        datos));

                Assert.AreEqual(
                    datos.IdViatico,
                    ObtenerIdEntidadAuditoria(
                        datos,
                        "Modificacion"));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConActorInexistente_RevierteViaticoYComprobante()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    false);

                datos.IdViatico =
                    CrearRepository()
                        .Insertar(
                            CrearViatico(
                                datos,
                                datos.DescripcionOriginal));

                Viatico viatico =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                ModificarViatico(
                    datos,
                    viatico);

                Assert.ThrowsException<
                    PersistenciaException>(
                        () => CrearRepository()
                            .Actualizar(
                                viatico,
                                CrearAuditoriaModificacionInvalida(
                                    datos)));

                Viatico recuperado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdViatico);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    1500m,
                    recuperado.Monto);

                Assert.AreEqual(
                    CategoriaGasto.Alimentacion,
                    recuperado.Categoria);

                Assert.AreEqual(
                    datos.DescripcionOriginal,
                    recuperado.Descripcion);

                Assert.IsNotNull(
                    recuperado.Comprobante);

                Assert.AreEqual(
                    datos.NumeroOriginal,
                    recuperado
                        .Comprobante
                        .Numero);

                Assert.AreEqual(
                    1500m,
                    recuperado
                        .Comprobante
                        .Total);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12);

            int baseNumerica =
                Convert.ToInt32(
                    sufijo.Substring(
                        0,
                        7),
                    16);

            int numeroComprobante =
                baseNumerica
                % 99999998
                + 1;

            return new DatosPrueba
            {
                Marca =
                    "VIATICO_AUDIT_TEST_" +
                    sufijo,

                NombreActor =
                    "viatico_actor_" +
                    sufijo,

                EmailActor =
                    "viatico_actor_" +
                    sufijo +
                    "@sigevip.test",

                EmailParticipante =
                    "viatico_participante_" +
                    sufijo +
                    "@sigevip.test",

                CuitProveedor =
                    "30" +
                    (
                        baseNumerica
                        % 1000000000
                    ).ToString(
                        "D9"),

                NumeroOriginal =
                    numeroComprobante
                        .ToString(
                            "D8"),

                NumeroModificado =
                    (
                        numeroComprobante
                        + 1
                    ).ToString(
                        "D8"),

                DescripcionAlta =
                    "VIATICO_AUDIT_TEST_" +
                    sufijo +
                    " alta",

                DescripcionAltaRollback =
                    "VIATICO_AUDIT_TEST_" +
                    sufijo +
                    " rollback alta",

                DescripcionOriginal =
                    "VIATICO_AUDIT_TEST_" +
                    sufijo +
                    " original",

                DescripcionModificada =
                    "VIATICO_AUDIT_TEST_" +
                    sufijo +
                    " modificada",

                FechaInicio =
                    new DateTime(
                        2037,
                        8,
                        10),

                FechaFin =
                    new DateTime(
                        2037,
                        8,
                        15),

                FechaViatico =
                    new DateTime(
                        2037,
                        8,
                        12)
            };
        }

        private static void PrepararContexto(
            DatosPrueba datos,
            bool insertarActor)
        {
            if (insertarActor)
            {
                InsertarActor(
                    datos);
            }

            datos.IdPersonaParticipante =
                InsertarPersona(
                    "Participante",
                    "Viatico",
                    datos.EmailParticipante);

            Persona participante =
                new Persona(
                    datos.IdPersonaParticipante,
                    "Participante",
                    "Viatico",
                    datos.EmailParticipante);

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje para " +
                        datos.Marca,
                    TipoViaje.Desplazamiento,
                    0m);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    participante
                });

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);
        }

        private static Viatico CrearViatico(
            DatosPrueba datos,
            string descripcion)
        {
            var viatico =
                new Viatico(
                    0,
                    datos.FechaViatico,
                    CategoriaGasto.Alimentacion,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    1500m,
                    descripcion,
                    CrearComprobanteOriginal(
                        datos));

            CrearViajeDominio(
                    datos,
                    null)
                .AgregarViatico(
                    viatico);

            return viatico;
        }

        private static void ModificarViatico(
            DatosPrueba datos,
            Viatico viatico)
        {
            Assert.IsNotNull(
                viatico);

            Assert.IsNotNull(
                viatico.Comprobante);

            Viaje viaje =
                CrearViajeDominio(
                    datos,
                    viatico);

            viaje.ModificarViatico(
                viatico,
                datos.FechaViatico.AddDays(
                    1),
                CategoriaGasto.Alojamiento,
                MetodoPago.EfectivoEmpresa,
                null,
                2750m,
                datos.DescripcionModificada,
                CrearComprobanteModificado(
                    datos,
                    viatico
                        .Comprobante
                        .IdComprobante));
        }

        private static Viaje CrearViajeDominio(
            DatosPrueba datos,
            Viatico viatico)
        {
            Persona participante =
                new Persona(
                    datos.IdPersonaParticipante,
                    "Participante",
                    "Viatico",
                    datos.EmailParticipante);

            var viaje =
                new Viaje(
                    datos.IdViaje,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje reconstruido para " +
                        datos.Marca,
                    TipoViaje.Desplazamiento,
                    0m);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    participante
                });

            if (viatico != null)
            {
                viaje.AgregarViatico(
                    viatico);
            }

            return viaje;
        }

        private static Comprobante
            CrearComprobanteOriginal(
                DatosPrueba datos)
        {
            return new Comprobante(
                0,
                TipoComprobante.FacturaB,
                datos.CuitProveedor,
                datos.Marca +
                    " proveedor original",
                SituacionFiscal.ResponsableInscripto,
                "0001",
                datos.NumeroOriginal,
                1200m,
                300m);
        }

        private static Comprobante
            CrearComprobanteModificado(
                DatosPrueba datos,
                int idComprobante)
        {
            return new Comprobante(
                idComprobante,
                TipoComprobante.FacturaB,
                datos.CuitProveedor,
                datos.Marca +
                    " proveedor modificado",
                SituacionFiscal.ResponsableInscripto,
                "0002",
                datos.NumeroModificado,
                2200m,
                550m);
        }

        private static AuditoriaRegistro
            CrearAuditoriaAltaValida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                datos.IdUsuarioActor,
                datos.NombreActor,
                "Viaticos",
                "Alta",
                "Viatico",
                null,
                datos.Marca +
                    " alta valida");
        }

        private static AuditoriaRegistro
            CrearAuditoriaAltaInvalida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "viatico_actor_inexistente",
                "Viaticos",
                "Alta",
                "Viatico",
                null,
                datos.Marca +
                    " alta invalida");
        }

        private static AuditoriaRegistro
            CrearAuditoriaModificacionValida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                datos.IdUsuarioActor,
                datos.NombreActor,
                "Viaticos",
                "Modificacion",
                "Viatico",
                datos.IdViatico,
                datos.Marca +
                    " modificacion valida");
        }

        private static AuditoriaRegistro
            CrearAuditoriaModificacionInvalida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "viatico_actor_inexistente",
                "Viaticos",
                "Modificacion",
                "Viatico",
                datos.IdViatico,
                datos.Marca +
                    " modificacion invalida");
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

        private static void InsertarActor(
            DatosPrueba datos)
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
    N'Auditor',
    N'Viaticos',
    @Email,
    1
);

DECLARE @IdPersona INT =
    CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.Usuario
(
    IdPersona,
    NombreUsuario,
    PasswordHash,
    PasswordSalt,
    IteracionesPassword,
    Activo
)
VALUES
(
    @IdPersona,
    @NombreUsuario,
    @Hash,
    @Salt,
    100000,
    1
);

SELECT
    @IdPersona,
    CAST(SCOPE_IDENTITY() AS INT);";

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
                        datos.EmailActor;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreActor;

                command.Parameters.Add(
                    "@Hash",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            1);

                command.Parameters.Add(
                    "@Salt",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            2);

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    datos.IdPersonaActor =
                        reader.GetInt32(
                            0);

                    datos.IdUsuarioActor =
                        reader.GetInt32(
                            1);
                }
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

        private static int ContarViaticosPorDescripcion(
            string descripcion)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Viatico
WHERE Descripcion = @Descripcion;";

            return EjecutarConteo(
                sql,
                parametros =>
                {
                    parametros.Add(
                        "@Descripcion",
                        SqlDbType.NVarChar,
                        1000).Value =
                            descripcion;
                });
        }

        private static int
            ContarComprobantesPorDescripcionViatico(
                string descripcion)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Comprobante AS comprobante
INNER JOIN dbo.Viatico AS viatico
    ON viatico.IdViatico =
        comprobante.IdViatico
WHERE viatico.Descripcion = @Descripcion;";

            return EjecutarConteo(
                sql,
                parametros =>
                {
                    parametros.Add(
                        "@Descripcion",
                        SqlDbType.NVarChar,
                        1000).Value =
                            descripcion;
                });
        }

        private static int ContarAuditorias(
            DatosPrueba datos)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE
    Descripcion LIKE @Marca
    AND Modulo = N'Viaticos'
    AND Entidad = N'Viatico';";

            return EjecutarConteo(
                sql,
                parametros =>
                {
                    parametros.Add(
                        "@Marca",
                        SqlDbType.NVarChar,
                        1000).Value =
                            "%" +
                            datos.Marca +
                            "%";
                });
        }

        private static int ObtenerIdEntidadAuditoria(
            DatosPrueba datos,
            string accion)
        {
            const string sql = @"
SELECT TOP (1)
    IdEntidad
FROM dbo.Auditoria
WHERE
    Descripcion LIKE @Marca
    AND Modulo = N'Viaticos'
    AND Accion = @Accion
    AND Entidad = N'Viatico'
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
                    "@Marca",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "%" +
                        datos.Marca +
                        "%";

                command.Parameters.Add(
                    "@Accion",
                    SqlDbType.NVarChar,
                    100).Value =
                        accion;

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

        private static int EjecutarConteo(
            string sql,
            Action<SqlParameterCollection>
                configurarParametros)
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
                configurarParametros(
                    command.Parameters);

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

            const string sql = @"
DELETE FROM dbo.Auditoria
WHERE
    IdUsuario = @IdUsuarioActor
    OR NombreUsuario = @NombreActor
    OR Descripcion LIKE @Marca;

DELETE FROM dbo.Comprobante
WHERE
    IdViatico IN
    (
        SELECT IdViatico
        FROM dbo.Viatico
        WHERE
            IdViaje = @IdViaje
            OR Descripcion LIKE @Marca
    )
    OR RazonSocialProveedor LIKE @Marca;

DELETE FROM dbo.Viatico
WHERE
    IdViaje = @IdViaje
    OR Descripcion LIKE @Marca;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.Viaje
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Persona
WHERE
    IdPersona IN
    (
        @IdPersonaActor,
        @IdPersonaParticipante
    )
    OR Email = @EmailActor
    OR Email = @EmailParticipante;";

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
                        using (
                            SqlCommand command =
                                new SqlCommand(
                                    sql,
                                    connection,
                                    transaction))
                        {
                            command.Parameters.Add(
                                "@IdUsuarioActor",
                                SqlDbType.Int).Value =
                                    datos.IdUsuarioActor;

                            command.Parameters.Add(
                                "@IdPersonaActor",
                                SqlDbType.Int).Value =
                                    datos.IdPersonaActor;

                            command.Parameters.Add(
                                "@IdPersonaParticipante",
                                SqlDbType.Int).Value =
                                    datos.IdPersonaParticipante;

                            command.Parameters.Add(
                                "@IdViaje",
                                SqlDbType.Int).Value =
                                    datos.IdViaje;

                            command.Parameters.Add(
                                "@NombreActor",
                                SqlDbType.NVarChar,
                                100).Value =
                                    datos.NombreActor
                                    ?? string.Empty;

                            command.Parameters.Add(
                                "@Marca",
                                SqlDbType.NVarChar,
                                1000).Value =
                                    "%" +
                                    (
                                        datos.Marca
                                        ?? string.Empty
                                    ) +
                                    "%";

                            command.Parameters.Add(
                                "@EmailActor",
                                SqlDbType.NVarChar,
                                254).Value =
                                    datos.EmailActor
                                    ?? string.Empty;

                            command.Parameters.Add(
                                "@EmailParticipante",
                                SqlDbType.NVarChar,
                                254).Value =
                                    datos.EmailParticipante
                                    ?? string.Empty;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        if (transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }

                        throw;
                    }
                }
            }
        }

        private static byte[] CrearBytes(
            byte valor)
        {
            byte[] resultado =
                new byte[32];

            for (
                int indice = 0;
                indice < resultado.Length;
                indice++)
            {
                resultado[indice] =
                    valor;
            }

            return resultado;
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings[
                        "SIGEVIP"];

            Assert.IsNotNull(
                settings);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString));

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public string Marca
            {
                get;
                set;
            }

            public int IdPersonaActor
            {
                get;
                set;
            }

            public int IdUsuarioActor
            {
                get;
                set;
            }

            public string NombreActor
            {
                get;
                set;
            }

            public string EmailActor
            {
                get;
                set;
            }

            public int IdPersonaParticipante
            {
                get;
                set;
            }

            public string EmailParticipante
            {
                get;
                set;
            }

            public int IdViaje
            {
                get;
                set;
            }

            public int IdViatico
            {
                get;
                set;
            }

            public string CuitProveedor
            {
                get;
                set;
            }

            public string NumeroOriginal
            {
                get;
                set;
            }

            public string NumeroModificado
            {
                get;
                set;
            }

            public string DescripcionAlta
            {
                get;
                set;
            }

            public string DescripcionAltaRollback
            {
                get;
                set;
            }

            public string DescripcionOriginal
            {
                get;
                set;
            }

            public string DescripcionModificada
            {
                get;
                set;
            }

            public DateTime FechaInicio
            {
                get;
                set;
            }

            public DateTime FechaFin
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
