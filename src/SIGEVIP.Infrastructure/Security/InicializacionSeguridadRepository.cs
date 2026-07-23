using System;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class InicializacionSeguridadRepository
        : IInicializacionSeguridadRepository
    {
        private const string CodigoGrupoAdministrador =
            "ADMINISTRADOR_GENERAL";

        private readonly SqlConnectionFactory _connectionFactory;

        public InicializacionSeguridadRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public bool ExisteUsuario(
            string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE u.NombreUsuario = @NombreUsuario
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END;";

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
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
                            NormalizarNombreUsuario(
                                nombreUsuario);

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible comprobar la existencia del usuario.",
                    exception);
            }
        }

        public bool CrearAdministradorInicial(
            string nombre,
            string apellido,
            string email,
            string nombreUsuario,
            PasswordHashResult passwordHash)
        {
            if (passwordHash == null)
            {
                throw new ArgumentNullException(
                    nameof(passwordHash));
            }

            string nombreNormalizado =
                NormalizarNombreUsuario(
                    nombreUsuario);

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                {
                    connection.Open();

                    using (
                        SqlTransaction transaction =
                            connection.BeginTransaction())
                    {
                        try
                        {
                            if (ExisteUsuarioInterno(
                                connection,
                                transaction,
                                nombreNormalizado))
                            {
                                transaction.Rollback();
                                return false;
                            }

                            int idGrupo =
                                ObtenerGrupoAdministrador(
                                    connection,
                                    transaction);

                            int idPersona =
                                InsertarPersona(
                                    connection,
                                    transaction,
                                    nombre,
                                    apellido,
                                    email);

                            int idUsuario =
                                InsertarUsuario(
                                    connection,
                                    transaction,
                                    idPersona,
                                    nombreNormalizado,
                                    passwordHash);

                            InsertarUsuarioGrupo(
                                connection,
                                transaction,
                                idUsuario,
                                idGrupo);

                            transaction.Commit();

                            return true;
                        }
                        catch
                        {
                            RevertirSiCorresponde(
                                transaction);

                            throw;
                        }
                    }
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
                when (
                    exception.Number == 2601 ||
                    exception.Number == 2627)
            {
                return false;
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible crear el administrador inicial.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de inicialización no pudo completarse.",
                    exception);
            }
        }

        private static bool ExisteUsuarioInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            string nombreUsuario)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE u.NombreUsuario = @NombreUsuario
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombreUsuario;

                return Convert.ToBoolean(
                    command.ExecuteScalar());
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
                        CodigoGrupoAdministrador;

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    throw new PersistenciaException(
                        "No existe un grupo activo ADMINISTRADOR_GENERAL.");
                }

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static int InsertarPersona(
            SqlConnection connection,
            SqlTransaction transaction,
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
                        nombre.Trim();

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        apellido.Trim();

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email.Trim();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarUsuario(
            SqlConnection connection,
            SqlTransaction transaction,
            int idPersona,
            string nombreUsuario,
            PasswordHashResult passwordHash)
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

        private static void RevertirSiCorresponde(
            SqlTransaction transaction)
        {
            if (transaction == null ||
                transaction.Connection == null)
            {
                return;
            }

            transaction.Rollback();
        }

        private static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return string.IsNullOrWhiteSpace(nombreUsuario)
                ? string.Empty
                : nombreUsuario
                    .Trim()
                    .ToLowerInvariant();
        }
    }
}
