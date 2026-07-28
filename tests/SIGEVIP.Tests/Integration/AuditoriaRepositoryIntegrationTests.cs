using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class AuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Listar_SinFiltros_DevuelveEventoPersistido()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarUsuario(
                    datos);

                InsertarAuditoria(
                    datos,
                    new DateTime(
                        2026,
                        7,
                        20,
                        10,
                        0,
                        0),
                    "Clientes",
                    "Alta",
                    "Cliente",
                    10,
                    datos.Marca +
                        " alta de cliente");

                AuditoriaListadoDto resultado =
                    CrearRepository()
                        .Listar(
                            AuditoriaFiltro
                                .CrearSinFiltros())
                        .SingleOrDefault(
                            actual =>
                                actual.Descripcion
                                    .Contains(
                                        datos.Marca));

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.IdUsuario,
                    resultado.IdUsuario);

                Assert.AreEqual(
                    "Clientes",
                    resultado.Modulo);

                Assert.AreEqual(
                    "Alta",
                    resultado.Accion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_ConFiltros_DevuelveSoloCoincidencia()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarUsuario(
                    datos);

                InsertarAuditoria(
                    datos,
                    new DateTime(
                        2026,
                        7,
                        20,
                        11,
                        0,
                        0),
                    "Clientes",
                    "Alta",
                    "Cliente",
                    20,
                    datos.Marca +
                        " cliente esperado");

                InsertarAuditoria(
                    datos,
                    new DateTime(
                        2026,
                        7,
                        20,
                        12,
                        0,
                        0),
                    "Viajes",
                    "Modificacion",
                    "Viaje",
                    30,
                    datos.Marca +
                        " viaje no esperado");

                AuditoriaFiltro filtro =
                    new AuditoriaFiltro(
                        new DateTime(
                            2026,
                            7,
                            20),
                        new DateTime(
                            2026,
                            7,
                            20),
                        datos.NombreUsuario,
                        "Clientes",
                        "Alta",
                        "cliente esperado");

                AuditoriaListadoDto[] resultados =
                    CrearRepository()
                        .Listar(
                            filtro)
                        .Where(
                            actual =>
                                actual.Descripcion
                                    .Contains(
                                        datos.Marca))
                        .ToArray();

                Assert.AreEqual(
                    1,
                    resultados.Length);

                Assert.AreEqual(
                    "Clientes",
                    resultados[0].Modulo);

                Assert.AreEqual(
                    20,
                    resultados[0].IdEntidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_MultiplesEventos_OrdenaMasRecientePrimero()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarUsuario(
                    datos);

                InsertarAuditoria(
                    datos,
                    new DateTime(
                        2026,
                        7,
                        20,
                        9,
                        0,
                        0),
                    "Clientes",
                    "Alta",
                    "Cliente",
                    40,
                    datos.Marca +
                        " evento anterior");

                InsertarAuditoria(
                    datos,
                    new DateTime(
                        2026,
                        7,
                        20,
                        15,
                        0,
                        0),
                    "Clientes",
                    "Modificacion",
                    "Cliente",
                    40,
                    datos.Marca +
                        " evento reciente");

                AuditoriaListadoDto[] resultados =
                    CrearRepository()
                        .Listar(
                            new AuditoriaFiltro(
                                null,
                                null,
                                datos.NombreUsuario,
                                string.Empty,
                                string.Empty,
                                datos.Marca))
                        .ToArray();

                Assert.AreEqual(
                    2,
                    resultados.Length);

                Assert.IsTrue(
                    resultados[0].FechaHora >
                    resultados[1].FechaHora);

                Assert.IsTrue(
                    resultados[0]
                        .Descripcion
                        .Contains(
                            "evento reciente"));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static AuditoriaRepository
            CrearRepository()
        {
            return new AuditoriaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string marca =
                "AUDIT_TEST_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12);

            return new DatosPrueba
            {
                Marca =
                    marca,

                NombreUsuario =
                    marca.ToLowerInvariant(),

                Email =
                    marca.ToLowerInvariant() +
                    "@sigevip.test"
            };
        }

        private static void InsertarUsuario(
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
    N'Auditoria',
    N'Prueba',
    @Email,
    1
);

DECLARE @IdPersona INT =
    CAST(
        SCOPE_IDENTITY()
        AS INT
    );

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
    @PasswordHash,
    @PasswordSalt,
    1000,
    1
);

SELECT
    @IdPersona AS IdPersona,
    CAST(
        SCOPE_IDENTITY()
        AS INT
    ) AS IdUsuario;";

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
                        datos.Email;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            1);

                command.Parameters.Add(
                    "@PasswordSalt",
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

                    datos.IdPersona =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdPersona"));

                    datos.IdUsuario =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdUsuario"));
                }
            }
        }

        private static void InsertarAuditoria(
            DatosPrueba datos,
            DateTime fechaHora,
            string modulo,
            string accion,
            string entidad,
            int? idEntidad,
            string descripcion)
        {
            const string sql = @"
INSERT INTO dbo.Auditoria
(
    FechaHora,
    IdUsuario,
    NombreUsuario,
    Modulo,
    Accion,
    Entidad,
    IdEntidad,
    Descripcion
)
VALUES
(
    @FechaHora,
    @IdUsuario,
    @NombreUsuario,
    @Modulo,
    @Accion,
    @Entidad,
    @IdEntidad,
    @Descripcion
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
                    "@FechaHora",
                    SqlDbType.DateTime2).Value =
                        fechaHora;

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        datos.IdUsuario;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreUsuario;

                command.Parameters.Add(
                    "@Modulo",
                    SqlDbType.NVarChar,
                    50).Value =
                        modulo;

                command.Parameters.Add(
                    "@Accion",
                    SqlDbType.NVarChar,
                    50).Value =
                        accion;

                command.Parameters.Add(
                    "@Entidad",
                    SqlDbType.NVarChar,
                    100).Value =
                        entidad;

                SqlParameter idEntidadParametro =
                    command.Parameters.Add(
                        "@IdEntidad",
                        SqlDbType.Int);

                idEntidadParametro.Value =
                    idEntidad.HasValue
                        ? (object)idEntidad.Value
                        : DBNull.Value;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    1000).Value =
                        descripcion;

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

            const string sql = @"
DELETE FROM dbo.Auditoria
WHERE
    IdUsuario = @IdUsuario
    OR NombreUsuario =
        @NombreUsuario;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario =
    @IdUsuario;

DELETE FROM dbo.Usuario
WHERE IdUsuario =
    @IdUsuario;

DELETE FROM dbo.Persona
WHERE IdPersona =
    @IdPersona;";

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
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        datos.IdUsuario;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        datos.IdPersona;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreUsuario
                        ?? string.Empty;

                connection.Open();

                command.ExecuteNonQuery();
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

            public string NombreUsuario
            {
                get;
                set;
            }

            public string Email
            {
                get;
                set;
            }

            public int IdPersona
            {
                get;
                set;
            }

            public int IdUsuario
            {
                get;
                set;
            }
        }
    }
}