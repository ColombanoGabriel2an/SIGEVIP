using System;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Auditoria
{
    internal static class AuditoriaSqlWriter
    {
        public static void Insertar(
            SqlConnection connection,
            SqlTransaction transaction,
            AuditoriaRegistro registro)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(
                    nameof(connection));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(
                    nameof(transaction));
            }

            if (registro == null)
            {
                throw new ArgumentNullException(
                    nameof(registro));
            }

            if (connection.State !=
                ConnectionState.Open)
            {
                throw new InvalidOperationException(
                    "La conexión debe encontrarse abierta para registrar la auditoría.");
            }

            if (!ReferenceEquals(
                transaction.Connection,
                connection))
            {
                throw new InvalidOperationException(
                    "La transacción de auditoría no pertenece a la conexión indicada.");
            }

            const string sql = @"
INSERT INTO dbo.Auditoria
(
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
    @IdUsuario,
    @NombreUsuario,
    @Modulo,
    @Accion,
    @Entidad,
    @IdEntidad,
    @Descripcion
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
                        registro.IdUsuario;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        registro.NombreUsuario;

                command.Parameters.Add(
                    "@Modulo",
                    SqlDbType.NVarChar,
                    50).Value =
                        registro.Modulo;

                command.Parameters.Add(
                    "@Accion",
                    SqlDbType.NVarChar,
                    50).Value =
                        registro.Accion;

                command.Parameters.Add(
                    "@Entidad",
                    SqlDbType.NVarChar,
                    100).Value =
                        registro.Entidad;

                SqlParameter idEntidad =
                    command.Parameters.Add(
                        "@IdEntidad",
                        SqlDbType.Int);

                idEntidad.Value =
                    registro.IdEntidad.HasValue
                        ? (object)registro.IdEntidad.Value
                        : DBNull.Value;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    1000).Value =
                        registro.Descripcion;

                int filas =
                    command.ExecuteNonQuery();

                if (filas != 1)
                {
                    throw new PersistenciaException(
                        "No fue posible registrar el evento de auditoría.");
                }
            }
        }
    }
}