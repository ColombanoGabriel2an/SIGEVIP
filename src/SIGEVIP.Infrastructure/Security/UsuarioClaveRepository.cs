using System;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class UsuarioClaveRepository
        : IUsuarioClaveRepository
    {
        private const string ActualizacionCredenciales = @"
UPDATE dbo.Usuario
SET
    PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt,
    IteracionesPassword = @IteracionesPassword
WHERE IdUsuario = @IdUsuario
  AND Activo = 1;";

        private readonly SqlConnectionFactory
            _connectionFactory;

        public UsuarioClaveRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public bool ActualizarCredenciales(
            int idUsuario,
            PasswordHashResult passwordHash)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario),
                    "El identificador del usuario debe ser mayor que cero.");
            }

            if (passwordHash == null)
            {
                throw new ArgumentNullException(
                    nameof(passwordHash));
            }

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                using (
                    SqlCommand command =
                        connection.CreateCommand())
                {
                    command.CommandType =
                        CommandType.Text;

                    command.CommandText =
                        ActualizacionCredenciales;

                    command.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int).Value =
                            idUsuario;

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

                    connection.Open();

                    int filasAfectadas =
                        command.ExecuteNonQuery();

                    return filasAfectadas == 1;
                }
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible actualizar la clave del usuario en SQL Server.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "No fue posible ejecutar la actualización de la clave.",
                    exception);
            }
        }
    }
}
