using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Clientes;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ClienteAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteClienteYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                ClienteRepository repository =
                    CrearClienteRepository();

                datos.IdCliente =
                    repository.Insertar(
                        datos.CrearCliente(),
                        CrearAuditoria(
                            datos,
                            "Alta",
                            null,
                            datos.Marca +
                            " alta de cliente"));

                AuditoriaListadoDto evento =
                    CrearAuditoriaRepository()
                        .Listar(
                            new AuditoriaFiltro(
                                null,
                                null,
                                datos.NombreUsuario,
                                "Clientes",
                                "Alta",
                                datos.Marca))
                        .SingleOrDefault();

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdCliente,
                    evento.IdEntidad);

                Assert.AreEqual(
                    "Cliente",
                    evento.Entidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConUsuarioAuditoriaInexistente_RevierteCliente()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                ClienteRepository repository =
                    CrearClienteRepository();

                AuditoriaRegistro auditoria =
                    new AuditoriaRegistro(
                        int.MaxValue,
                        "usuario.inexistente",
                        "Clientes",
                        "Alta",
                        "Cliente",
                        null,
                        datos.Marca +
                        " auditoria inválida");

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Insertar(
                            datos.CrearCliente(),
                            auditoria));

                Assert.IsFalse(
                    repository.ExisteCuit(
                        datos.Cuit,
                        null));

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConUsuarioAuditoriaInexistente_RevierteCambios()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                ClienteRepository repository =
                    CrearClienteRepository();

                datos.IdCliente =
                    repository.Insertar(
                        datos.CrearCliente());

                Cliente cliente =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                string razonSocialOriginal =
                    cliente.RazonSocial;

                cliente.ActualizarDatos(
                    razonSocialOriginal +
                    " Modificado",
                    cliente.Cuit,
                    cliente.Email,
                    cliente.Telefono,
                    cliente.Localidad,
                    cliente.Provincia);

                AuditoriaRegistro auditoria =
                    new AuditoriaRegistro(
                        int.MaxValue,
                        "usuario.inexistente",
                        "Clientes",
                        "Modificacion",
                        "Cliente",
                        datos.IdCliente,
                        datos.Marca +
                        " modificación inválida");

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Actualizar(
                            cliente,
                            auditoria));

                Cliente recuperado =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                Assert.AreEqual(
                    razonSocialOriginal,
                    recuperado.RazonSocial);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void CambiarEstado_ConAuditoriaValida_PersisteDosEventos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                ClienteRepository repository =
                    CrearClienteRepository();

                datos.IdCliente =
                    repository.Insertar(
                        datos.CrearCliente());

                repository.Desactivar(
                    datos.IdCliente,
                    CrearAuditoria(
                        datos,
                        "Desactivacion",
                        datos.IdCliente,
                        datos.Marca +
                        " desactivación"));

                repository.Activar(
                    datos.IdCliente,
                    CrearAuditoria(
                        datos,
                        "Activacion",
                        datos.IdCliente,
                        datos.Marca +
                        " activación"));

                AuditoriaListadoDto[] eventos =
                    CrearAuditoriaRepository()
                        .Listar(
                            new AuditoriaFiltro(
                                null,
                                null,
                                datos.NombreUsuario,
                                "Clientes",
                                string.Empty,
                                datos.Marca))
                        .ToArray();

                Assert.AreEqual(
                    2,
                    eventos.Length);

                Assert.IsTrue(
                    eventos.Any(
                        actual =>
                            actual.Accion ==
                            "Desactivacion"));

                Assert.IsTrue(
                    eventos.Any(
                        actual =>
                            actual.Accion ==
                            "Activacion"));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdCliente)
                        .Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static ClienteRepository
            CrearClienteRepository()
        {
            return new ClienteRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static AuditoriaRepository
            CrearAuditoriaRepository()
        {
            return new AuditoriaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static AuditoriaRegistro CrearAuditoria(
            DatosPrueba datos,
            string accion,
            int? idEntidad,
            string descripcion)
        {
            return new AuditoriaRegistro(
                datos.IdUsuario,
                datos.NombreUsuario,
                "Clientes",
                accion,
                "Cliente",
                idEntidad,
                descripcion);
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string marca =
                "CLIENTE_AUDIT_TEST_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12);

            uint numero =
                unchecked(
                    (uint)Guid.NewGuid()
                        .GetHashCode());

            string parteNumerica =
                numero
                    .ToString("D10")
                    .Substring(
                        0,
                        9);

            return new DatosPrueba
            {
                Marca =
                    marca,

                NombreUsuario =
                    marca
                        .ToLowerInvariant(),

                EmailActor =
                    marca
                        .ToLowerInvariant() +
                    "@sigevip.test",

                Cuit =
                    "30" +
                    parteNumerica,

                RazonSocial =
                    "Cliente Auditoría " +
                    marca
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
    N'Auditoria',
    N'Clientes',
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

        private static int ContarAuditorias(
            string marca)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE Descripcion LIKE @Marca;";

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

            public string EmailActor
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

            public int IdCliente
            {
                get;
                set;
            }

            public string Cuit
            {
                get;
                set;
            }

            public string RazonSocial
            {
                get;
                set;
            }

            public Cliente CrearCliente()
            {
                return new Cliente(
                    0,
                    RazonSocial,
                    Cuit,
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe");
            }
        }
    }
}