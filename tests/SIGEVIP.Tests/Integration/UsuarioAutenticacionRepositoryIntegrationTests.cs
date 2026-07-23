using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class UsuarioAutenticacionRepositoryIntegrationTests
    {
        private const string PasswordPrueba =
            "ClaveIntegracion123";

        [TestMethod]
        public void BuscarPorNombreUsuario_UsuarioInexistente_DevuelveNull()
        {
            UsuarioAutenticacionRepository repository =
                CrearRepository();

            Usuario usuario =
                repository.BuscarPorNombreUsuario(
                    "inexistente_" +
                    Guid.NewGuid().ToString("N"));

            Assert.IsNull(usuario);
        }

        [TestMethod]
        public void BuscarPorNombreUsuario_UsuarioPersistido_RecuperaDatos()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(true);

            try
            {
                UsuarioAutenticacionRepository repository =
                    CrearRepository();

                Usuario usuario =
                    repository.BuscarPorNombreUsuario(
                        datos.NombreUsuario.ToUpperInvariant());

                Assert.IsNotNull(usuario);
                Assert.AreEqual(
                    datos.IdUsuario,
                    usuario.IdUsuario);
                Assert.AreEqual(
                    datos.IdPersona,
                    usuario.IdPersona);
                Assert.AreEqual(
                    datos.NombreUsuario,
                    usuario.NombreUsuario);
                Assert.IsTrue(usuario.Activo);
                Assert.AreEqual(
                    Pbkdf2PasswordHasher.LongitudHashBytes,
                    usuario.PasswordHash.Length);
                Assert.AreEqual(
                    Pbkdf2PasswordHasher.LongitudSaltBytes,
                    usuario.PasswordSalt.Length);
                Assert.AreEqual(
                    Pbkdf2PasswordHasher.IteracionesPredeterminadas,
                    usuario.IteracionesPassword);
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        [TestMethod]
        public void BuscarPorNombreUsuario_UsuarioPersistido_RecuperaGrupoDirecto()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(true);

            try
            {
                Usuario usuario =
                    CrearRepository()
                        .BuscarPorNombreUsuario(
                            datos.NombreUsuario);

                Grupo grupo =
                    usuario.Grupos.SingleOrDefault(
                        actual =>
                            actual.Codigo ==
                            "ADMINISTRADOR_GENERAL");

                Assert.IsNotNull(grupo);
                Assert.IsTrue(grupo.Activo);
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        [TestMethod]
        public void BuscarPorNombreUsuario_UsuarioPersistido_RecuperaPermisosDirectos()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(true);

            try
            {
                Usuario usuario =
                    CrearRepository()
                        .BuscarPorNombreUsuario(
                            datos.NombreUsuario);

                Grupo grupo =
                    usuario.Grupos.Single(
                        actual =>
                            actual.Codigo ==
                            "ADMINISTRADOR_GENERAL");

                Assert.IsTrue(
                    grupo.ObtenerPermisosEfectivos()
                        .Any(
                            permiso =>
                                permiso.Codigo ==
                                "USUARIO_GESTIONAR"));

                Assert.IsTrue(
                    grupo.ObtenerPermisosEfectivos()
                        .Any(
                            permiso =>
                                permiso.Codigo ==
                                "GRUPO_GESTIONAR"));
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        [TestMethod]
        public void BuscarPorNombreUsuario_UsuarioInactivo_ConservaEstado()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(false);

            try
            {
                Usuario usuario =
                    CrearRepository()
                        .BuscarPorNombreUsuario(
                            datos.NombreUsuario);

                Assert.IsNotNull(usuario);
                Assert.IsFalse(usuario.Activo);
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        [TestMethod]
        public void AutenticacionService_UsuarioPersistidoConPasswordCorrecto_Autentica()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(true);

            try
            {
                AutenticacionService service =
                    new AutenticacionService(
                        CrearRepository(),
                        new Pbkdf2PasswordHasher());

                ResultadoAutenticacion resultado =
                    service.Autenticar(
                        datos.NombreUsuario,
                        PasswordPrueba);

                Assert.IsTrue(resultado.Exitoso);
                Assert.IsNotNull(resultado.Usuario);
                Assert.AreEqual(
                    datos.IdUsuario,
                    resultado.Usuario.IdUsuario);
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        [TestMethod]
        public void AutenticacionService_UsuarioPersistidoConPasswordIncorrecto_Rechaza()
        {
            DatosUsuarioPrueba datos =
                CrearUsuarioPrueba(true);

            try
            {
                AutenticacionService service =
                    new AutenticacionService(
                        CrearRepository(),
                        new Pbkdf2PasswordHasher());

                ResultadoAutenticacion resultado =
                    service.Autenticar(
                        datos.NombreUsuario,
                        "PasswordIncorrecto");

                Assert.IsFalse(resultado.Exitoso);
                Assert.IsNull(resultado.Usuario);
                Assert.AreEqual(
                    AutenticacionService.MensajeCredencialesInvalidas,
                    resultado.Mensaje);
            }
            finally
            {
                EliminarUsuarioPrueba(datos);
            }
        }

        private static UsuarioAutenticacionRepository CrearRepository()
        {
            return new UsuarioAutenticacionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosUsuarioPrueba CrearUsuarioPrueba(
            bool activo)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            string nombreUsuario =
                "integracion_" +
                sufijo;

            string email =
                "integracion_" +
                sufijo +
                "@sigevip.test";

            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult hash =
                hasher.CrearHash(
                    PasswordPrueba);

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
                        int idGrupo =
                            ObtenerGrupoAdministrador(
                                connection,
                                transaction);

                        int idPersona =
                            InsertarPersona(
                                connection,
                                transaction,
                                email);

                        int idUsuario =
                            InsertarUsuario(
                                connection,
                                transaction,
                                idPersona,
                                nombreUsuario,
                                hash,
                                activo);

                        InsertarUsuarioGrupo(
                            connection,
                            transaction,
                            idUsuario,
                            idGrupo);

                        transaction.Commit();

                        return new DatosUsuarioPrueba(
                            idPersona,
                            idUsuario,
                            nombreUsuario);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int ObtenerGrupoAdministrador(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string sql = @"
SELECT
    g.IdGrupo
FROM dbo.Grupo AS g
WHERE
    g.Codigo = @Codigo
    AND g.Activo = 1;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        "ADMINISTRADOR_GENERAL";

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "No existe el grupo activo ADMINISTRADOR_GENERAL. Ejecute el seed de seguridad.");
                }

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static int InsertarPersona(
            SqlConnection connection,
            SqlTransaction transaction,
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
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Prueba";

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Integración";

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarUsuario(
            SqlConnection connection,
            SqlTransaction transaction,
            int idPersona,
            string nombreUsuario,
            PasswordHashResult hash,
            bool activo)
        {
            const string sql = @"
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

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    Pbkdf2PasswordHasher.LongitudHashBytes).Value =
                        hash.Hash;

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    Pbkdf2PasswordHasher.LongitudSaltBytes).Value =
                        hash.Salt;

                command.Parameters.Add(
                    "@IteracionesPassword",
                    SqlDbType.Int).Value =
                        hash.Iteraciones;

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void InsertarUsuarioGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            int idUsuario,
            int idGrupo)
        {
            const string sql = @"
INSERT INTO dbo.UsuarioGrupo
(
    IdUsuario,
    IdGrupo
)
VALUES
(
    @IdUsuario,
    @IdGrupo
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.ExecuteNonQuery();
            }
        }

        private static void EliminarUsuarioPrueba(
            DatosUsuarioPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
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

                connection.Open();
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
                    "No se encontró la cadena de conexión SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosUsuarioPrueba
        {
            public DatosUsuarioPrueba(
                int idPersona,
                int idUsuario,
                string nombreUsuario)
            {
                IdPersona = idPersona;
                IdUsuario = idUsuario;
                NombreUsuario = nombreUsuario;
            }

            public int IdPersona { get; private set; }

            public int IdUsuario { get; private set; }

            public string NombreUsuario { get; private set; }
        }
    }
}
