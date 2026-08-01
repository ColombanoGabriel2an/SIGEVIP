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
    public class AuditoriaDetalleRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerCambios_EventoConDetalle_DevuelveCambiosOrdenados()
        {
            DatosPrueba datos = CrearDatosPrueba();

            try
            {
                InsertarUsuario(datos);
                InsertarEventoConCambios(datos);

                AuditoriaCambioDto[] cambios =
                    CrearRepository()
                        .ObtenerCambios(datos.IdAuditoria)
                        .ToArray();

                Assert.AreEqual(2, cambios.Length);
                Assert.AreEqual("RazonSocial", cambios[0].Campo);
                Assert.AreEqual("Original", cambios[0].ValorAnterior);
                Assert.AreEqual("Modificada", cambios[0].ValorNuevo);
                Assert.AreEqual("Email", cambios[1].Campo);
                Assert.IsNull(cambios[1].ValorAnterior);
                Assert.AreEqual(
                    datos.EmailNuevo,
                    cambios[1].ValorNuevo);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void ObtenerCambios_EventoSinDetalle_DevuelveColeccionVacia()
        {
            DatosPrueba datos = CrearDatosPrueba();

            try
            {
                InsertarUsuario(datos);
                InsertarEventoSinCambios(datos);

                AuditoriaCambioDto[] cambios =
                    CrearRepository()
                        .ObtenerCambios(datos.IdAuditoria)
                        .ToArray();

                Assert.AreEqual(0, cambios.Length);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        private static AuditoriaRepository CrearRepository()
        {
            return new AuditoriaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string marca =
                "AUDIT_DETAIL_TEST_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 10);

            return new DatosPrueba
            {
                Marca = marca,
                NombreUsuario = marca.ToLowerInvariant(),
                EmailUsuario =
                    marca.ToLowerInvariant() +
                    "@sigevip.test",
                EmailNuevo =
                    "nuevo." +
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
    N'Detalle',
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
    @PasswordHash,
    @PasswordSalt,
    1000,
    1
);

SELECT
    @IdPersona AS IdPersona,
    CAST(SCOPE_IDENTITY() AS INT) AS IdUsuario;";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value = datos.EmailUsuario;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value = datos.NombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    32).Value = CrearBytes(1);

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    32).Value = CrearBytes(2);

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(reader.Read());
                    datos.IdPersona =
                        reader.GetInt32(
                            reader.GetOrdinal("IdPersona"));
                    datos.IdUsuario =
                        reader.GetInt32(
                            reader.GetOrdinal("IdUsuario"));
                }
            }
        }

        private static void InsertarEventoConCambios(
            DatosPrueba datos)
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
    SYSUTCDATETIME(),
    @IdUsuario,
    @NombreUsuario,
    N'Clientes',
    N'Modificacion',
    N'Cliente',
    1,
    @Descripcion
);

DECLARE @IdAuditoria BIGINT =
    CAST(SCOPE_IDENTITY() AS BIGINT);

INSERT INTO dbo.AuditoriaCambio
(
    IdAuditoria,
    Campo,
    ValorAnterior,
    ValorNuevo
)
VALUES
(
    @IdAuditoria,
    N'RazonSocial',
    N'Original',
    N'Modificada'
);

INSERT INTO dbo.AuditoriaCambio
(
    IdAuditoria,
    Campo,
    ValorAnterior,
    ValorNuevo
)
VALUES
(
    @IdAuditoria,
    N'Email',
    NULL,
    @EmailNuevo
);

SELECT @IdAuditoria;";

            datos.IdAuditoria =
                EjecutarInsercionEvento(
                    datos,
                    sql,
                    true);
        }

        private static void InsertarEventoSinCambios(
            DatosPrueba datos)
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
    SYSUTCDATETIME(),
    @IdUsuario,
    @NombreUsuario,
    N'Seguridad',
    N'InicioSesion',
    N'Sesion',
    NULL,
    @Descripcion
);

SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            datos.IdAuditoria =
                EjecutarInsercionEvento(
                    datos,
                    sql,
                    false);
        }

        private static long EjecutarInsercionEvento(
            DatosPrueba datos,
            string sql,
            bool incluirEmailNuevo)
        {
            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value = datos.IdUsuario;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value = datos.NombreUsuario;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    1000).Value = datos.Marca;

                if (incluirEmailNuevo)
                {
                    command.Parameters.Add(
                        "@EmailNuevo",
                        SqlDbType.NVarChar,
                        -1).Value = datos.EmailNuevo;
                }

                connection.Open();
                object result = command.ExecuteScalar();
                Assert.IsNotNull(result);
                return Convert.ToInt64(result);
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
DELETE FROM dbo.AuditoriaCambio
WHERE IdAuditoria = @IdAuditoria;

DELETE FROM dbo.Auditoria
WHERE
    IdAuditoria = @IdAuditoria
    OR Descripcion = @Descripcion;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona;";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@IdAuditoria",
                    SqlDbType.BigInt).Value =
                        datos.IdAuditoria;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    1000).Value =
                        datos.Marca ?? string.Empty;

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        datos.IdUsuario;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        datos.IdPersona;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static byte[] CrearBytes(byte valor)
        {
            byte[] bytes = new byte[32];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = valor;
            }
            return bytes;
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings["SIGEVIP"];

            Assert.IsNotNull(settings);
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString));
            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public string Marca { get; set; }
            public string NombreUsuario { get; set; }
            public string EmailUsuario { get; set; }
            public string EmailNuevo { get; set; }
            public int IdPersona { get; set; }
            public int IdUsuario { get; set; }
            public long IdAuditoria { get; set; }
        }
    }
}
