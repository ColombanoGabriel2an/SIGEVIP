using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class JerarquiaGruposRepositoryIntegrationTests
    {
        private const string PasswordPrueba =
            "ClaveJerarquia123";

        [TestMethod]
        public void BuscarPorNombreUsuario_GrupoHijoPersistido_HeredaPermiso()
        {
            DatosJerarquiaPrueba datos =
                CrearJerarquiaConPermiso();

            try
            {
                Usuario usuario =
                    CrearRepository()
                        .BuscarPorNombreUsuario(
                            datos.NombreUsuario);

                Assert.IsNotNull(usuario);

                Grupo grupoPadre =
                    usuario.Grupos.SingleOrDefault(
                        grupo =>
                            string.Equals(
                                grupo.Codigo,
                                datos.CodigoGrupoPadre,
                                StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(grupoPadre);

                Assert.IsTrue(
                    grupoPadre.ObtenerPermisosEfectivos()
                        .Any(
                            permiso =>
                                string.Equals(
                                    permiso.Codigo,
                                    datos.CodigoPermiso,
                                    StringComparison.OrdinalIgnoreCase)));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void BuscarPorNombreUsuario_CicloPersistido_LanzaPersistenciaException()
        {
            DatosJerarquiaPrueba datos =
                CrearJerarquiaCiclica();

            try
            {
                PersistenciaException exception =
                    Assert.ThrowsException<PersistenciaException>(
                        () =>
                            CrearRepository()
                                .BuscarPorNombreUsuario(
                                    datos.NombreUsuario));

                Assert.IsTrue(
                    exception.Message.Contains(
                        "jerarquía de grupos inválida o cíclica"));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        private static UsuarioAutenticacionRepository CrearRepository()
        {
            return new UsuarioAutenticacionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosJerarquiaPrueba CrearJerarquiaConPermiso()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            DatosJerarquiaPrueba datos =
                new DatosJerarquiaPrueba(
                    "jerarquia_" + sufijo,
                    "jerarquia_" + sufijo + "@sigevip.test",
                    "PADRE_" + sufijo,
                    "HIJO_" + sufijo,
                    "PERMISO_" + sufijo);

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
                        datos.IdPersona =
                            InsertarPersona(
                                connection,
                                transaction,
                                datos.Email);

                        PasswordHashResult hash =
                            new Pbkdf2PasswordHasher()
                                .CrearHash(
                                    PasswordPrueba);

                        datos.IdUsuario =
                            InsertarUsuario(
                                connection,
                                transaction,
                                datos.IdPersona,
                                datos.NombreUsuario,
                                hash);

                        datos.IdGrupoPadre =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoPadre,
                                "Grupo padre de integración");

                        datos.IdGrupoHijo =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoHijo,
                                "Grupo hijo de integración");

                        datos.IdPermiso =
                            InsertarPermiso(
                                connection,
                                transaction,
                                datos.CodigoPermiso);

                        InsertarGrupoPermiso(
                            connection,
                            transaction,
                            datos.IdGrupoHijo,
                            datos.IdPermiso);

                        InsertarGrupoGrupo(
                            connection,
                            transaction,
                            datos.IdGrupoPadre,
                            datos.IdGrupoHijo);

                        InsertarUsuarioGrupo(
                            connection,
                            transaction,
                            datos.IdUsuario,
                            datos.IdGrupoPadre);

                        transaction.Commit();

                        return datos;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static DatosJerarquiaPrueba CrearJerarquiaCiclica()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            DatosJerarquiaPrueba datos =
                new DatosJerarquiaPrueba(
                    "ciclo_" + sufijo,
                    "ciclo_" + sufijo + "@sigevip.test",
                    "CICLO_A_" + sufijo,
                    "CICLO_B_" + sufijo,
                    null);

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
                        datos.IdPersona =
                            InsertarPersona(
                                connection,
                                transaction,
                                datos.Email);

                        PasswordHashResult hash =
                            new Pbkdf2PasswordHasher()
                                .CrearHash(
                                    PasswordPrueba);

                        datos.IdUsuario =
                            InsertarUsuario(
                                connection,
                                transaction,
                                datos.IdPersona,
                                datos.NombreUsuario,
                                hash);

                        datos.IdGrupoPadre =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoPadre,
                                "Grupo A del ciclo de integración");

                        datos.IdGrupoHijo =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoHijo,
                                "Grupo B del ciclo de integración");

                        InsertarGrupoGrupo(
                            connection,
                            transaction,
                            datos.IdGrupoPadre,
                            datos.IdGrupoHijo);

                        InsertarGrupoGrupo(
                            connection,
                            transaction,
                            datos.IdGrupoHijo,
                            datos.IdGrupoPadre);

                        InsertarUsuarioGrupo(
                            connection,
                            transaction,
                            datos.IdUsuario,
                            datos.IdGrupoPadre);

                        transaction.Commit();

                        return datos;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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
                        "Jerarquía";

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
            PasswordHashResult hash)
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

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string descripcion)
        {
            const string sql = @"
INSERT INTO dbo.Grupo
(
    Codigo,
    Nombre,
    Descripcion,
    Activo
)
VALUES
(
    @Codigo,
    @Nombre,
    @Descripcion,
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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        codigo;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        codigo;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        descripcion;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarPermiso(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo)
        {
            const string sql = @"
INSERT INTO dbo.Permiso
(
    Codigo,
    Nombre,
    Descripcion,
    Activo
)
VALUES
(
    @Codigo,
    @Nombre,
    @Descripcion,
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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        codigo;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        codigo;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        "Permiso temporal para prueba de integración.";

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void InsertarGrupoPermiso(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupo,
            int idPermiso)
        {
            const string sql = @"
INSERT INTO dbo.GrupoPermiso
(
    IdGrupo,
    IdPermiso
)
VALUES
(
    @IdGrupo,
    @IdPermiso
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        idPermiso;

                command.ExecuteNonQuery();
            }
        }

        private static void InsertarGrupoGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre,
            int idGrupoHijo)
        {
            const string sql = @"
INSERT INTO dbo.GrupoGrupo
(
    IdGrupoPadre,
    IdGrupoHijo
)
VALUES
(
    @IdGrupoPadre,
    @IdGrupoHijo
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        idGrupoHijo;

                command.ExecuteNonQuery();
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

        private static void EliminarDatosPrueba(
            DatosJerarquiaPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre IN (@IdGrupoPadre, @IdGrupoHijo)
    OR IdGrupoHijo IN (@IdGrupoPadre, @IdGrupoHijo);

DELETE FROM dbo.GrupoPermiso
WHERE
    IdGrupo IN (@IdGrupoPadre, @IdGrupoHijo)
    OR IdPermiso = @IdPermiso;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona;

DELETE FROM dbo.Grupo
WHERE IdGrupo IN (@IdGrupoPadre, @IdGrupoHijo);

DELETE FROM dbo.Permiso
WHERE IdPermiso = @IdPermiso;";

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
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        datos.IdGrupoPadre;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        datos.IdGrupoHijo;

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        datos.IdPermiso;

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

        private sealed class DatosJerarquiaPrueba
        {
            public DatosJerarquiaPrueba(
                string nombreUsuario,
                string email,
                string codigoGrupoPadre,
                string codigoGrupoHijo,
                string codigoPermiso)
            {
                NombreUsuario = nombreUsuario;
                Email = email;
                CodigoGrupoPadre = codigoGrupoPadre;
                CodigoGrupoHijo = codigoGrupoHijo;
                CodigoPermiso = codigoPermiso;
            }

            public int IdPersona { get; set; }

            public int IdUsuario { get; set; }

            public int IdGrupoPadre { get; set; }

            public int IdGrupoHijo { get; set; }

            public int IdPermiso { get; set; }

            public string NombreUsuario { get; private set; }

            public string Email { get; private set; }

            public string CodigoGrupoPadre { get; private set; }

            public string CodigoGrupoHijo { get; private set; }

            public string CodigoPermiso { get; private set; }
        }
    }
}
