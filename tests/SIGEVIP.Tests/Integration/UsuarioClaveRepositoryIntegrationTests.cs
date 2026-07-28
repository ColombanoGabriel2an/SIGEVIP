using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    public class UsuarioClaveRepositoryIntegrationTests
    {
        private const string PasswordAnterior =
            "ClaveAnterior123";

        private const string PasswordNueva =
            "ClaveNueva456";

        private string _connectionString;
        private SqlConnectionFactory _connectionFactory;
        private Pbkdf2PasswordHasher _passwordHasher;

        [TestInitialize]
        public void Inicializar()
        {
            ConnectionStringSettings configuracion =
                ConfigurationManager
                    .ConnectionStrings["SIGEVIP"];

            Assert.IsNotNull(
                configuracion,
                "No se encontró la cadena de conexión SIGEVIP.");

            _connectionString =
                configuracion.ConnectionString;

            _connectionFactory =
                new SqlConnectionFactory(
                    _connectionString);

            _passwordHasher =
                new Pbkdf2PasswordHasher();
        }

        [TestMethod]
        public void ActualizarCredenciales_UsuarioActivo_ActualizaDatosCriptograficos()
        {
            RegistroPrueba registro =
                CrearUsuarioPrueba(
                    true);

            try
            {
                UsuarioClaveRepository repository =
                    new UsuarioClaveRepository(
                        _connectionFactory);

                PasswordHashResult credencialesNuevas =
                    _passwordHasher.CrearHash(
                        PasswordNueva);

                bool actualizado =
                    repository.ActualizarCredenciales(
                        registro.IdUsuario,
                        credencialesNuevas);

                Assert.IsTrue(
                    actualizado);

                CredencialesPersistidas persistidas =
                    ObtenerCredenciales(
                        registro.IdUsuario);

                CollectionAssert.AreEqual(
                    credencialesNuevas.Hash,
                    persistidas.PasswordHash);

                CollectionAssert.AreEqual(
                    credencialesNuevas.Salt,
                    persistidas.PasswordSalt);

                Assert.AreEqual(
                    credencialesNuevas.Iteraciones,
                    persistidas.IteracionesPassword);

                Assert.IsTrue(
                    _passwordHasher.Verificar(
                        PasswordNueva,
                        persistidas.PasswordHash,
                        persistidas.PasswordSalt,
                        persistidas.IteracionesPassword));

                Assert.IsFalse(
                    _passwordHasher.Verificar(
                        PasswordAnterior,
                        persistidas.PasswordHash,
                        persistidas.PasswordSalt,
                        persistidas.IteracionesPassword));
            }
            finally
            {
                EliminarDatosPrueba(
                    registro);
            }
        }

        [TestMethod]
        public void ActualizarCredenciales_UsuarioInactivo_DevuelveFalseYConservaDatos()
        {
            RegistroPrueba registro =
                CrearUsuarioPrueba(
                    false);

            try
            {
                CredencialesPersistidas anteriores =
                    ObtenerCredenciales(
                        registro.IdUsuario);

                UsuarioClaveRepository repository =
                    new UsuarioClaveRepository(
                        _connectionFactory);

                PasswordHashResult credencialesNuevas =
                    _passwordHasher.CrearHash(
                        PasswordNueva);

                bool actualizado =
                    repository.ActualizarCredenciales(
                        registro.IdUsuario,
                        credencialesNuevas);

                Assert.IsFalse(
                    actualizado);

                CredencialesPersistidas posteriores =
                    ObtenerCredenciales(
                        registro.IdUsuario);

                CollectionAssert.AreEqual(
                    anteriores.PasswordHash,
                    posteriores.PasswordHash);

                CollectionAssert.AreEqual(
                    anteriores.PasswordSalt,
                    posteriores.PasswordSalt);

                Assert.AreEqual(
                    anteriores.IteracionesPassword,
                    posteriores.IteracionesPassword);

                Assert.IsTrue(
                    _passwordHasher.Verificar(
                        PasswordAnterior,
                        posteriores.PasswordHash,
                        posteriores.PasswordSalt,
                        posteriores.IteracionesPassword));
            }
            finally
            {
                EliminarDatosPrueba(
                    registro);
            }
        }

        [TestMethod]
        public void ActualizarCredenciales_UsuarioInexistente_DevuelveFalse()
        {
            UsuarioClaveRepository repository =
                new UsuarioClaveRepository(
                    _connectionFactory);

            PasswordHashResult credencialesNuevas =
                _passwordHasher.CrearHash(
                    PasswordNueva);

            bool actualizado =
                repository.ActualizarCredenciales(
                    int.MaxValue,
                    credencialesNuevas);

            Assert.IsFalse(
                actualizado);
        }

        private RegistroPrueba CrearUsuarioPrueba(
            bool activo)
        {
            string identificador =
                Guid.NewGuid()
                    .ToString("N");

            string nombreUsuario =
                "clave.test." +
                identificador;

            string email =
                "clave.test." +
                identificador +
                "@sigevip.test";

            PasswordHashResult passwordHash =
                _passwordHasher.CrearHash(
                    PasswordAnterior);

            using (
                SqlConnection connection =
                    new SqlConnection(
                        _connectionString))
            using (
                SqlCommand command =
                    connection.CreateCommand())
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandText = @"
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
    @Nombre,
    @Apellido,
    @Email,
    1
);

DECLARE @IdPersona INT =
    CONVERT(INT, SCOPE_IDENTITY());

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
    @IteracionesPassword,
    @Activo
);

DECLARE @IdUsuario INT =
    CONVERT(INT, SCOPE_IDENTITY());

COMMIT TRANSACTION;

SELECT
    @IdPersona AS IdPersona,
    @IdUsuario AS IdUsuario;";

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Prueba";

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Cambio Clave";

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    passwordHash.Hash.Length).Value =
                        passwordHash.Hash;

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    passwordHash.Salt.Length).Value =
                        passwordHash.Salt;

                command.Parameters.Add(
                    "@IteracionesPassword",
                    SqlDbType.Int).Value =
                        passwordHash.Iteraciones;

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    Assert.IsTrue(
                        reader.Read());

                    return new RegistroPrueba(
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdPersona")),
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdUsuario")));
                }
            }
        }

        private CredencialesPersistidas ObtenerCredenciales(
            int idUsuario)
        {
            using (
                SqlConnection connection =
                    new SqlConnection(
                        _connectionString))
            using (
                SqlCommand command =
                    connection.CreateCommand())
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandText = @"
SELECT
    PasswordHash,
    PasswordSalt,
    IteracionesPassword
FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;";

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    Assert.IsTrue(
                        reader.Read(),
                        "No se encontró el Usuario de prueba.");

                    return new CredencialesPersistidas(
                        (byte[])reader["PasswordHash"],
                        (byte[])reader["PasswordSalt"],
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IteracionesPassword")));
                }
            }
        }

        private void EliminarDatosPrueba(
            RegistroPrueba registro)
        {
            if (registro == null)
            {
                return;
            }

            using (
                SqlConnection connection =
                    new SqlConnection(
                        _connectionString))
            using (
                SqlCommand command =
                    connection.CreateCommand())
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandText = @"
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona;

COMMIT TRANSACTION;";

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        registro.IdUsuario;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        registro.IdPersona;

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private sealed class RegistroPrueba
        {
            public RegistroPrueba(
                int idPersona,
                int idUsuario)
            {
                IdPersona =
                    idPersona;

                IdUsuario =
                    idUsuario;
            }

            public int IdPersona
            {
                get;
                private set;
            }

            public int IdUsuario
            {
                get;
                private set;
            }
        }

        private sealed class CredencialesPersistidas
        {
            public CredencialesPersistidas(
                byte[] passwordHash,
                byte[] passwordSalt,
                int iteracionesPassword)
            {
                PasswordHash =
                    passwordHash;

                PasswordSalt =
                    passwordSalt;

                IteracionesPassword =
                    iteracionesPassword;
            }

            public byte[] PasswordHash
            {
                get;
                private set;
            }

            public byte[] PasswordSalt
            {
                get;
                private set;
            }

            public int IteracionesPassword
            {
                get;
                private set;
            }
        }
    }
}
