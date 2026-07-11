using System;
using System.Data.SqlClient;

namespace SIGEVIP.Infrastructure.Data
{
    public sealed class DatabaseHealthCheck
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public DatabaseHealthCheck(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory
                ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public string Check()
        {
            using (SqlConnection connection = _connectionFactory.Create())
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT CAST(SERVERPROPERTY('MachineName') AS NVARCHAR(128)) " +
                        "+ CASE " +
                        "WHEN SERVERPROPERTY('InstanceName') IS NULL THEN '' " +
                        "ELSE '\\' + CAST(SERVERPROPERTY('InstanceName') AS NVARCHAR(128)) " +
                        "END";

                    object result = command.ExecuteScalar();

                    return result == null
                        ? "Conexión correcta."
                        : "Conexión correcta con " + result;
                }
            }
        }
    }
}
