using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class SesionAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void RegistrarInicioYCierreSesion_PersisteDosEventos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarUsuario(
                    datos);

                SesionActual sesion =
                    new SesionActual();

                sesion.Iniciar(
                    new Usuario(
                        datos.IdUsuario,
                        datos.IdPersona,
                        datos.NombreUsuario,
                        CrearBytes(1),
                        CrearBytes(2),
                        1000));

                SesionAuditoriaService servicio =
                    new SesionAuditoriaService(
                        new SesionAuditoriaRepository(
                            new SqlConnectionFactory(
                                ObtenerConnectionString())),
                        sesion);

                servicio.RegistrarInicioSesion();
                servicio.RegistrarCierreSesion();

                AuditoriaListadoDto[] eventos =
                    new AuditoriaRepository(
                        new SqlConnectionFactory(
                            ObtenerConnectionString()))
                    .Listar(
                        new AuditoriaFiltro(
                            null,
                            null,
                            datos.NombreUsuario,
                            "Seguridad",
                            string.Empty,
                            datos.NombreUsuario))
                    .ToArray();

                Assert.AreEqual(
                    2,
                    eventos.Length);

                Assert.IsTrue(
                    eventos.Any(
                        actual =>
                            actual.Accion ==
                            "InicioSesion"));

                Assert.IsTrue(
                    eventos.Any(
                        actual =>
                            actual.Accion ==
                            "CierreSesion"));

                Assert.IsTrue(
                    eventos.All(
                        actual =>
                            actual.Entidad ==
                            "Sesion"));

                Assert.IsTrue(
                    eventos.All(
                        actual =>
                            actual.IdEntidad ==
                            datos.IdUsuario));

                Assert.IsTrue(
                    eventos.All(
                        actual =>
                            !actual.Descripcion
                                .ToLowerInvariant()
                                .Contains(
                                    "password")));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string marca =
                "SESION_AUDIT_TEST_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12);

            return new DatosPrueba
            {
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
    N'Sesion',
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
                        CrearBytes(1);

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(2);

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
    OR NombreUsuario = @NombreUsuario;

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
                    .ConnectionStrings["SIGEVIP"];

            Assert.IsNotNull(
                settings);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString));

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
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