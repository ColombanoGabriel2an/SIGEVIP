using System;
using System.Data.SqlClient;

namespace SIGEVIP.Infrastructure.Data
{
    public sealed class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "La cadena de conexión no puede estar vacía.",
                    nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
