using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Viaticos
{
    public sealed class PersonaConsultaViaticoRepository
        : IPersonaConsultaViaticoRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public PersonaConsultaViaticoRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<PagadorSeleccionDto>
            ListarActivas()
        {
            const string sql = @"
SELECT
    persona.IdPersona,
    persona.Nombre,
    persona.Apellido,
    persona.Activo
FROM dbo.Persona AS persona
WHERE persona.Activo = 1
ORDER BY
    persona.Apellido,
    persona.Nombre,
    persona.IdPersona;";

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
                    connection.Open();

                    var resultados =
                        new List<PagadorSeleccionDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nombre =
                                LeerTextoObligatorio(
                                    reader,
                                    "Nombre");

                            string apellido =
                                LeerTextoObligatorio(
                                    reader,
                                    "Apellido");

                            resultados.Add(
                                new PagadorSeleccionDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdPersona")),
                                    nombre +
                                    " " +
                                    apellido,
                                    reader.GetBoolean(
                                        reader.GetOrdinal(
                                            "Activo"))));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar las personas pagadoras disponibles.",
                    exception);
            }
        }

        public Persona ObtenerPorId(
            int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona),
                    "El identificador de la Persona debe ser mayor que cero.");
            }

            const string sql = @"
SELECT
    persona.IdPersona,
    persona.Nombre,
    persona.Apellido,
    persona.Email,
    persona.Activo
FROM dbo.Persona AS persona
WHERE persona.IdPersona = @IdPersona;";

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

                        return ReconstruirPersona(
                            reader);
                    }
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible recuperar la persona pagadora.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de la Persona pagadora son inválidos.",
                    exception);
            }
        }

        private static Persona ReconstruirPersona(
            SqlDataReader reader)
        {
            var persona =
                new Persona(
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

            if (!reader.GetBoolean(
                reader.GetOrdinal(
                    "Activo")))
            {
                persona.Desactivar();
            }

            return persona;
        }

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            if (reader.IsDBNull(
                ordinal))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede ser nula.");
            }

            string valor =
                reader.GetString(
                    ordinal);

            if (string.IsNullOrWhiteSpace(
                valor))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede estar vacía.");
            }

            return valor;
        }

        private static PersistenciaException
            CrearErrorPersistencia(
                string mensaje,
                SqlException exception)
        {
            return new PersistenciaException(
                mensaje,
                exception);
        }
    }
}
