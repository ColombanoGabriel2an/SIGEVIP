using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Clientes;
using SIGEVIP.Infrastructure.Data;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ClienteControlCambiosIntegrationTests
    {
        [TestMethod]
        public void Actualizar_ConCambiosDetallados_PersisteValoresAnterioresYNuevos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(datos);

                ClienteRepository repository =
                    new ClienteRepository(
                        new SqlConnectionFactory(
                            ObtenerConnectionString()));

                Cliente cliente =
                    new Cliente(
                        0,
                        datos.RazonSocialOriginal,
                        datos.Cuit,
                        datos.EmailOriginal,
                        "3415550000",
                        "Rosario",
                        "Santa Fe");

                datos.IdCliente =
                    repository.Insertar(cliente);

                Cliente persistido =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                persistido.ActualizarDatos(
                    datos.RazonSocialNueva,
                    persistido.Cuit,
                    datos.EmailNuevo,
                    persistido.Telefono,
                    persistido.Localidad,
                    persistido.Provincia);

                AuditoriaRegistro auditoria =
                    new AuditoriaRegistro(
                        datos.IdUsuario,
                        datos.NombreUsuario,
                        "Clientes",
                        "Modificacion",
                        "Cliente",
                        datos.IdCliente,
                        datos.Marca +
                        " modificación detallada")
                    .ConCambios(
                        new[]
                        {
                            new AuditoriaCambioRegistro(
                                "RazonSocial",
                                datos.RazonSocialOriginal,
                                datos.RazonSocialNueva),

                            new AuditoriaCambioRegistro(
                                "Email",
                                datos.EmailOriginal,
                                datos.EmailNuevo)
                        });

                repository.Actualizar(
                    persistido,
                    auditoria);

                Dictionary<string, CambioPersistido>
                    cambios =
                        ObtenerCambios(
                            datos.Marca);

                Assert.AreEqual(
                    2,
                    cambios.Count);

                Assert.AreEqual(
                    datos.RazonSocialOriginal,
                    cambios["RazonSocial"]
                        .ValorAnterior);

                Assert.AreEqual(
                    datos.RazonSocialNueva,
                    cambios["RazonSocial"]
                        .ValorNuevo);

                Assert.AreEqual(
                    datos.EmailOriginal,
                    cambios["Email"]
                        .ValorAnterior);

                Assert.AreEqual(
                    datos.EmailNuevo,
                    cambios["Email"]
                        .ValorNuevo);

                Cliente actualizado =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                Assert.AreEqual(
                    datos.RazonSocialNueva,
                    actualizado.RazonSocial);

                Assert.AreEqual(
                    datos.EmailNuevo,
                    actualizado.Email);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string marca =
                "CLIENTE_CAMBIO_TEST_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 10);

            uint numero =
                unchecked(
                    (uint)Guid.NewGuid()
                        .GetHashCode());

            string parteNumerica =
                numero
                    .ToString("D10")
                    .Substring(0, 9);

            return new DatosPrueba
            {
                Marca = marca,
                NombreUsuario = marca.ToLowerInvariant(),
                EmailActor =
                    marca.ToLowerInvariant() +
                    "@sigevip.test",
                Cuit = "30" + parteNumerica,
                RazonSocialOriginal =
                    "Cliente original " +
                    marca,
                RazonSocialNueva =
                    "Cliente modificado " +
                    marca,
                EmailOriginal =
                    "original." +
                    marca.ToLowerInvariant() +
                    "@sigevip.test",
                EmailNuevo =
                    "nuevo." +
                    marca.ToLowerInvariant() +
                    "@sigevip.test"
            };
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
    N'Control',
    N'Cambios',
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
                        datos.EmailActor;

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
                    Assert.IsTrue(reader.Read());

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

        private static Dictionary<string, CambioPersistido>
            ObtenerCambios(
                string marca)
        {
            const string sql = @"
SELECT
    cambio.Campo,
    cambio.ValorAnterior,
    cambio.ValorNuevo
FROM dbo.Auditoria AS auditoria
INNER JOIN dbo.AuditoriaCambio AS cambio
    ON cambio.IdAuditoria =
        auditoria.IdAuditoria
WHERE
    auditoria.Modulo = N'Clientes'
    AND auditoria.Accion = N'Modificacion'
    AND auditoria.Entidad = N'Cliente'
    AND auditoria.Descripcion LIKE @Marca;";

            Dictionary<string, CambioPersistido>
                resultado =
                    new Dictionary
                        <string, CambioPersistido>(
                            StringComparer.OrdinalIgnoreCase);

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
                        marca +
                        "%";

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string campo =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Campo"));

                        resultado.Add(
                            campo,
                            new CambioPersistido
                            {
                                ValorAnterior =
                                    LeerTextoOpcional(
                                        reader,
                                        "ValorAnterior"),

                                ValorNuevo =
                                    LeerTextoOpcional(
                                        reader,
                                        "ValorNuevo")
                            });
                    }
                }
            }

            return resultado;
        }

        private static string LeerTextoOpcional(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(columna);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE cambio
FROM dbo.AuditoriaCambio AS cambio
INNER JOIN dbo.Auditoria AS auditoria
    ON auditoria.IdAuditoria =
        cambio.IdAuditoria
WHERE
    auditoria.IdUsuario = @IdUsuario
    OR auditoria.NombreUsuario = @NombreUsuario
    OR auditoria.Descripcion LIKE @Marca;

DELETE FROM dbo.Auditoria
WHERE
    IdUsuario = @IdUsuario
    OR NombreUsuario = @NombreUsuario
    OR Descripcion LIKE @Marca;

DELETE FROM dbo.Cliente
WHERE
    IdCliente = @IdCliente
    OR Cuit = @Cuit;

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
                    "@IdCliente",
                    SqlDbType.Int).Value =
                        datos.IdCliente;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreUsuario
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
                    "@Cuit",
                    SqlDbType.NVarChar,
                    20).Value =
                        datos.Cuit
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
                resultado[indice] = valor;
            }

            return resultado;
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
            public string EmailActor { get; set; }
            public int IdPersona { get; set; }
            public int IdUsuario { get; set; }
            public int IdCliente { get; set; }
            public string Cuit { get; set; }
            public string RazonSocialOriginal { get; set; }
            public string RazonSocialNueva { get; set; }
            public string EmailOriginal { get; set; }
            public string EmailNuevo { get; set; }
        }

        private sealed class CambioPersistido
        {
            public string ValorAnterior { get; set; }
            public string ValorNuevo { get; set; }
        }
    }
}
