using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Viajes
{
    public sealed class PersonaConsultaRepository
        : IPersonaConsultaRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public PersonaConsultaRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<PersonaSeleccionDto>
            ListarActivas()
        {
            const string sql = @"
SELECT
    p.IdPersona,
    p.Nombre,
    p.Apellido,
    p.Email,
    p.Activo
FROM dbo.Persona AS p
WHERE p.Activo = 1
ORDER BY
    p.Apellido,
    p.Nombre,
    p.IdPersona;";

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
                        new List<PersonaSeleccionDto>();

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
                                new PersonaSeleccionDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdPersona")),
                                    nombre +
                                    " " +
                                    apellido,
                                    LeerTextoObligatorio(
                                        reader,
                                        "Email"),
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
                    "No fue posible listar las personas disponibles.",
                    exception);
            }
        }

        public IReadOnlyCollection<Persona>
            ObtenerPorIds(
                IReadOnlyCollection<int> idsPersona)
        {
            if (idsPersona == null)
            {
                throw new ArgumentNullException(
                    nameof(idsPersona));
            }

            if (idsPersona.Count == 0)
            {
                return new List<Persona>()
                    .AsReadOnly();
            }

            List<int> ids =
                idsPersona
                    .Distinct()
                    .ToList();

            if (ids.Any(
                idPersona =>
                    idPersona <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idsPersona),
                    "Los identificadores de Persona deben ser mayores que cero.");
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    p.IdPersona,
    p.Nombre,
    p.Apellido,
    p.Email,
    p.Activo
FROM dbo.Persona AS p
WHERE p.IdPersona IN
(");

            var parametros =
                new List<SqlParameter>();

            for (int indice = 0;
                indice < ids.Count;
                indice++)
            {
                if (indice > 0)
                {
                    sql.Append(", ");
                }

                string nombreParametro =
                    "@IdPersona" +
                    indice;

                sql.Append(
                    nombreParametro);

                parametros.Add(
                    new SqlParameter(
                        nombreParametro,
                        SqlDbType.Int)
                    {
                        Value =
                            ids[indice]
                    });
            }

            sql.Append(@"
)
ORDER BY p.IdPersona;");

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql.ToString(),
                            connection))
                {
                    foreach (
                        SqlParameter parametro
                        in parametros)
                    {
                        command.Parameters.Add(
                            parametro);
                    }

                    connection.Open();

                    var resultados =
                        new List<Persona>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                ReconstruirPersona(
                                    reader));
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
                    "No fue posible recuperar las personas seleccionadas.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de una Persona son inválidos.",
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
