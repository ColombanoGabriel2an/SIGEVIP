using System;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class PerfilSesionRepository
        : IPerfilSesionRepository
    {
        private const string ConsultaPerfil = @"
SELECT
    p.IdPersona,
    p.Nombre,
    p.Apellido,
    p.Email
FROM dbo.Persona AS p
WHERE p.IdPersona = @IdPersona;";

        private readonly SqlConnectionFactory
            _connectionFactory;

        public PerfilSesionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public PerfilSesion BuscarPorIdPersona(
            int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona),
                    "El identificador de persona debe ser mayor que cero.");
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
                        ConsultaPerfil;

                    command.Parameters.Add(
                        "@IdPersona",
                        SqlDbType.Int).Value =
                            idPersona;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader(
                                CommandBehavior.SingleRow))
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new PerfilSesion(
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "IdPersona")),
                            LeerTextoObligatorio(
                                reader,
                                "Nombre"),
                            LeerTextoObligatorio(
                                reader,
                                "Apellido"),
                            LeerTextoObligatorio(
                                reader,
                                "Email"));
                    }
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible consultar el perfil de la sesion.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "No fue posible reconstruir el perfil de la sesion.",
                    exception);
            }
            catch (ArgumentException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de la persona son invalidos.",
                    exception);
            }
        }

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string nombreColumna)
        {
            int ordinal =
                reader.GetOrdinal(nombreColumna);

            if (reader.IsDBNull(ordinal))
            {
                throw new PersistenciaException(
                    "La columna obligatoria " +
                    nombreColumna +
                    " no contiene un valor.");
            }

            string valor =
                reader.GetString(ordinal);

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new PersistenciaException(
                    "La columna obligatoria " +
                    nombreColumna +
                    " contiene un valor invalido.");
            }

            return valor;
        }
    }
}
