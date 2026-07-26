using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Visitas
{
    public sealed class ClienteConsultaVisitaRepository
        : IClienteConsultaVisitaRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public ClienteConsultaVisitaRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<ClienteSeleccionVisitaDto>
            ListarActivos()
        {
            const string sql = @"
SELECT
    c.IdCliente,
    c.RazonSocial,
    c.Cuit,
    c.Activo
FROM dbo.Cliente AS c
WHERE c.Activo = 1
ORDER BY
    c.RazonSocial,
    c.IdCliente;";

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
                        new List<ClienteSeleccionVisitaDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new ClienteSeleccionVisitaDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdCliente")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "RazonSocial"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Cuit"),
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
                    "No fue posible listar los clientes disponibles para la visita.",
                    exception);
            }
        }

        public IReadOnlyCollection<Cliente>
            ObtenerPorIds(
                IReadOnlyCollection<int> idsClientes)
        {
            if (idsClientes == null)
            {
                throw new ArgumentNullException(
                    nameof(idsClientes));
            }

            if (idsClientes.Count == 0)
            {
                return new List<Cliente>()
                    .AsReadOnly();
            }

            List<int> ids =
                idsClientes
                    .Distinct()
                    .ToList();

            if (ids.Any(
                idCliente =>
                    idCliente <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idsClientes),
                    "Los identificadores de Cliente deben ser mayores que cero.");
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    c.IdCliente,
    c.RazonSocial,
    c.Cuit,
    c.Email,
    c.Telefono,
    c.Localidad,
    c.Provincia,
    c.Activo
FROM dbo.Cliente AS c
WHERE c.IdCliente IN
(");

            var parametros =
                new List<SqlParameter>();

            for (
                int indice = 0;
                indice < ids.Count;
                indice++)
            {
                if (indice > 0)
                {
                    sql.Append(
                        ", ");
                }

                string nombreParametro =
                    "@IdCliente" +
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
ORDER BY c.IdCliente;");

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
                        new List<Cliente>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                ReconstruirCliente(
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
                    "No fue posible recuperar los clientes seleccionados.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de un Cliente son inválidos.",
                    exception);
            }
        }

        private static Cliente ReconstruirCliente(
            SqlDataReader reader)
        {
            return Cliente.Reconstruir(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdCliente")),
                LeerTextoObligatorio(
                    reader,
                    "RazonSocial"),
                LeerTextoObligatorio(
                    reader,
                    "Cuit"),
                LeerTextoOpcional(
                    reader,
                    "Email"),
                LeerTextoOpcional(
                    reader,
                    "Telefono"),
                LeerTextoOpcional(
                    reader,
                    "Localidad"),
                LeerTextoOpcional(
                    reader,
                    "Provincia"),
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo")));
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

        private static string LeerTextoOpcional(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            return reader.IsDBNull(
                ordinal)
                ? string.Empty
                : reader.GetString(
                    ordinal);
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