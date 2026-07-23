using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class InicializacionSeguridadRepositoryIntegrationTests
    {
        private const string PasswordPrueba =
            "ClaveInicial123";

        [TestMethod]
        public void CrearAdministradorInicial_DatosValidos_CreaRegistrosYGrupo()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InicializacionSeguridadService service =
                    CrearService();

                ResultadoInicializacionSeguridad resultado =
                    service.CrearAdministradorInicial(
                        datos.Nombre,
                        datos.Apellido,
                        datos.Email,
                        datos.NombreUsuario,
                        PasswordPrueba,
                        PasswordPrueba);

                Assert.IsTrue(resultado.Exitoso);

                RegistroPersistido registro =
                    LeerRegistro(
                        datos.NombreUsuario);

                Assert.IsNotNull(registro);
                Assert.AreEqual(
                    datos.Nombre,
                    registro.Nombre);
                Assert.AreEqual(
                    datos.Apellido,
                    registro.Apellido);
                Assert.AreEqual(
                    datos.Email,
                    registro.Email);
                Assert.AreEqual(
                    "ADMINISTRADOR_GENERAL",
                    registro.CodigoGrupo);
                Assert.IsTrue(registro.PersonaActiva);
                Assert.IsTrue(registro.UsuarioActivo);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void CrearAdministradorInicial_DatosValidos_PersistePbkdf2Verificable()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                ResultadoInicializacionSeguridad resultado =
                    CrearService()
                        .CrearAdministradorInicial(
                            datos.Nombre,
                            datos.Apellido,
                            datos.Email,
                            datos.NombreUsuario,
                            PasswordPrueba,
                            PasswordPrueba);

                Assert.IsTrue(resultado.Exitoso);

                RegistroPersistido registro =
                    LeerRegistro(
                        datos.NombreUsuario);

                Assert.AreEqual(
                    Pbkdf2PasswordHasher.LongitudHashBytes,
                    registro.PasswordHash.Length);

                Assert.AreEqual(
                    Pbkdf2PasswordHasher.LongitudSaltBytes,
                    registro.PasswordSalt.Length);

                Assert.AreEqual(
                    Pbkdf2PasswordHasher.IteracionesPredeterminadas,
                    registro.IteracionesPassword);

                Assert.IsTrue(
                    new Pbkdf2PasswordHasher()
                        .Verificar(
                            PasswordPrueba,
                            registro.PasswordHash,
                            registro.PasswordSalt,
                            registro.IteracionesPassword));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void CrearAdministradorInicial_SegundaEjecucion_NoDuplica()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InicializacionSeguridadService service =
                    CrearService();

                ResultadoInicializacionSeguridad primero =
                    service.CrearAdministradorInicial(
                        datos.Nombre,
                        datos.Apellido,
                        datos.Email,
                        datos.NombreUsuario,
                        PasswordPrueba,
                        PasswordPrueba);

                ResultadoInicializacionSeguridad segundo =
                    service.CrearAdministradorInicial(
                        datos.Nombre,
                        datos.Apellido,
                        datos.Email,
                        datos.NombreUsuario,
                        PasswordPrueba,
                        PasswordPrueba);

                Assert.IsTrue(primero.Exitoso);
                Assert.IsFalse(segundo.Exitoso);
                Assert.IsTrue(segundo.UsuarioExistente);

                Assert.AreEqual(
                    1,
                    ContarUsuarios(
                        datos.NombreUsuario));

                Assert.AreEqual(
                    1,
                    ContarPersonas(
                        datos.Email));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void CrearAdministradorInicial_HashInvalido_ReviertePersona()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            InicializacionSeguridadRepository repository =
                CrearRepository();

            PasswordHashResult hashInvalido =
                new PasswordHashResult(
                    new byte[] { 1 },
                    new byte[] { 2 },
                    1);

            try
            {
                Assert.ThrowsException<PersistenciaException>(
                    () => repository.CrearAdministradorInicial(
                        datos.Nombre,
                        datos.Apellido,
                        datos.Email,
                        datos.NombreUsuario,
                        hashInvalido));

                Assert.AreEqual(
                    0,
                    ContarUsuarios(
                        datos.NombreUsuario));

                Assert.AreEqual(
                    0,
                    ContarPersonas(
                        datos.Email));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        private static InicializacionSeguridadService CrearService()
        {
            return new InicializacionSeguridadService(
                CrearRepository(),
                new Pbkdf2PasswordHasher());
        }

        private static InicializacionSeguridadRepository CrearRepository()
        {
            return new InicializacionSeguridadRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            return new DatosPrueba(
                "Administrador",
                "Integración",
                "inicializacion_" +
                    sufijo +
                    "@sigevip.test",
                "inicializacion_" +
                    sufijo);
        }

        private static RegistroPersistido LeerRegistro(
            string nombreUsuario)
        {
            const string sql = @"
SELECT
    p.Nombre,
    p.Apellido,
    p.Email,
    p.Activo AS PersonaActiva,
    u.PasswordHash,
    u.PasswordSalt,
    u.IteracionesPassword,
    u.Activo AS UsuarioActivo,
    g.Codigo AS CodigoGrupo
FROM dbo.Usuario AS u
INNER JOIN dbo.Persona AS p
    ON p.IdPersona = u.IdPersona
INNER JOIN dbo.UsuarioGrupo AS ug
    ON ug.IdUsuario = u.IdUsuario
INNER JOIN dbo.Grupo AS g
    ON g.IdGrupo = ug.IdGrupo
WHERE u.NombreUsuario = @NombreUsuario;";

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
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombreUsuario;

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new RegistroPersistido(
                        reader.GetString(
                            reader.GetOrdinal("Nombre")),
                        reader.GetString(
                            reader.GetOrdinal("Apellido")),
                        reader.GetString(
                            reader.GetOrdinal("Email")),
                        reader.GetBoolean(
                            reader.GetOrdinal("PersonaActiva")),
                        (byte[])reader[
                            "PasswordHash"],
                        (byte[])reader[
                            "PasswordSalt"],
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IteracionesPassword")),
                        reader.GetBoolean(
                            reader.GetOrdinal("UsuarioActivo")),
                        reader.GetString(
                            reader.GetOrdinal("CodigoGrupo")));
                }
            }
        }

        private static int ContarUsuarios(
            string nombreUsuario)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Usuario AS u
WHERE u.NombreUsuario = @NombreUsuario;";

            return EjecutarConteo(
                sql,
                "@NombreUsuario",
                nombreUsuario,
                100);
        }

        private static int ContarPersonas(
            string email)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Persona AS p
WHERE p.Email = @Email;";

            return EjecutarConteo(
                sql,
                "@Email",
                email,
                254);
        }

        private static int EjecutarConteo(
            string sql,
            string nombreParametro,
            string valor,
            int longitud)
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
                    nombreParametro,
                    SqlDbType.NVarChar,
                    longitud).Value =
                        valor;

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
DECLARE @IdUsuario INT;
DECLARE @IdPersona INT;

SELECT
    @IdUsuario = u.IdUsuario,
    @IdPersona = u.IdPersona
FROM dbo.Usuario AS u
WHERE u.NombreUsuario = @NombreUsuario;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Persona
WHERE
    IdPersona = @IdPersona
    OR Email = @Email;";

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
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreUsuario;

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        datos.Email;

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
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public DatosPrueba(
                string nombre,
                string apellido,
                string email,
                string nombreUsuario)
            {
                Nombre = nombre;
                Apellido = apellido;
                Email = email;
                NombreUsuario = nombreUsuario;
            }

            public string Nombre { get; private set; }

            public string Apellido { get; private set; }

            public string Email { get; private set; }

            public string NombreUsuario { get; private set; }
        }

        private sealed class RegistroPersistido
        {
            public RegistroPersistido(
                string nombre,
                string apellido,
                string email,
                bool personaActiva,
                byte[] passwordHash,
                byte[] passwordSalt,
                int iteracionesPassword,
                bool usuarioActivo,
                string codigoGrupo)
            {
                Nombre = nombre;
                Apellido = apellido;
                Email = email;
                PersonaActiva = personaActiva;
                PasswordHash = passwordHash;
                PasswordSalt = passwordSalt;
                IteracionesPassword = iteracionesPassword;
                UsuarioActivo = usuarioActivo;
                CodigoGrupo = codigoGrupo;
            }

            public string Nombre { get; private set; }

            public string Apellido { get; private set; }

            public string Email { get; private set; }

            public bool PersonaActiva { get; private set; }

            public byte[] PasswordHash { get; private set; }

            public byte[] PasswordSalt { get; private set; }

            public int IteracionesPassword { get; private set; }

            public bool UsuarioActivo { get; private set; }

            public string CodigoGrupo { get; private set; }
        }
    }
}
