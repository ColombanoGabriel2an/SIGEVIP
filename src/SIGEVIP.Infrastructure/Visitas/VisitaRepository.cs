using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Visitas
{
    public sealed class VisitaRepository
        : IVisitaRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public VisitaRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public int Insertar(
            Visita visita)
        {
            ValidarVisitaPersistible(
                visita);

            const string sqlVisita = @"
INSERT INTO dbo.Visita
(
    IdViaje,
    Fecha,
    Observacion,
    LocalidadEncuentro
)
VALUES
(
    @IdViaje,
    @Fecha,
    @Observacion,
    @LocalidadEncuentro
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

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
                            int idVisita;

                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sqlVisita,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosVisita(
                                    command,
                                    visita);

                                idVisita =
                                    Convert.ToInt32(
                                        command.ExecuteScalar());
                            }

                            InsertarClientes(
                                connection,
                                transaction,
                                idVisita,
                                visita.Clientes);

                            transaction.Commit();

                            return idVisita;
                        }
                        catch
                        {
                            if (transaction.Connection != null)
                            {
                                transaction.Rollback();
                            }

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
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar la visita.",
                    exception);
            }
        }

        public IReadOnlyCollection<VisitaListadoDto>
            ListarPorViaje(
                int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            const string sql = @"
SELECT
    visita.IdVisita,
    visita.IdViaje,
    visita.Fecha,
    visita.Observacion,
    visita.LocalidadEncuentro,

    STUFF
    (
        (
            SELECT
                N', ' +
                cliente.RazonSocial
            FROM dbo.VisitaCliente
                AS relacionResumen
            INNER JOIN dbo.Cliente
                AS cliente
                ON cliente.IdCliente =
                    relacionResumen.IdCliente
            WHERE relacionResumen.IdVisita =
                visita.IdVisita
            ORDER BY
                cliente.RazonSocial,
                cliente.IdCliente
            FOR XML PATH(N''),
                TYPE
        ).value(
            N'.',
            N'nvarchar(max)'
        ),
        1,
        2,
        N''
    ) AS ClientesResumen
FROM dbo.Visita AS visita
WHERE visita.IdViaje = @IdViaje
ORDER BY
    visita.Fecha DESC,
    visita.IdVisita DESC;";

            return EjecutarListado(
                sql,
                "@IdViaje",
                idViaje,
                "No fue posible listar las visitas del viaje.");
        }

        public IReadOnlyCollection<VisitaListadoDto>
            ListarPorCliente(
                int idCliente)
        {
            if (idCliente <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idCliente));
            }

            const string sql = @"
SELECT
    visita.IdVisita,
    visita.IdViaje,
    visita.Fecha,
    visita.Observacion,
    visita.LocalidadEncuentro,

    STUFF
    (
        (
            SELECT
                N', ' +
                clienteResumen.RazonSocial
            FROM dbo.VisitaCliente
                AS relacionResumen
            INNER JOIN dbo.Cliente
                AS clienteResumen
                ON clienteResumen.IdCliente =
                    relacionResumen.IdCliente
            WHERE relacionResumen.IdVisita =
                visita.IdVisita
            ORDER BY
                clienteResumen.RazonSocial,
                clienteResumen.IdCliente
            FOR XML PATH(N''),
                TYPE
        ).value(
            N'.',
            N'nvarchar(max)'
        ),
        1,
        2,
        N''
    ) AS ClientesResumen
FROM dbo.Visita AS visita
INNER JOIN dbo.VisitaCliente AS relacionFiltro
    ON relacionFiltro.IdVisita =
        visita.IdVisita
WHERE relacionFiltro.IdCliente = @IdCliente
ORDER BY
    visita.Fecha DESC,
    visita.IdVisita DESC;";

            return EjecutarListado(
                sql,
                "@IdCliente",
                idCliente,
                "No fue posible listar las visitas del cliente.");
        }

        private IReadOnlyCollection<VisitaListadoDto>
            EjecutarListado(
                string sql,
                string nombreParametro,
                int valorParametro,
                string mensajeError)
        {
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
                        nombreParametro,
                        SqlDbType.Int).Value =
                            valorParametro;

                    connection.Open();

                    var resultados =
                        new List<VisitaListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                CrearDto(
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
                    mensajeError,
                    exception);
            }
        }

        private static void ValidarVisitaPersistible(
            Visita visita)
        {
            if (visita == null)
            {
                throw new ArgumentNullException(
                    nameof(visita));
            }

            if (visita.IdVisita < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(visita),
                    "El identificador de la visita no puede ser negativo.");
            }

            if (visita.IdViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "La visita debe encontrarse asociada a un viaje persistido.");
            }

            if (visita.Clientes == null ||
                visita.Clientes.Count == 0)
            {
                throw new ReglaNegocioException(
                    "La visita debe tener al menos un cliente asociado.");
            }

            foreach (
                Cliente cliente
                in visita.Clientes)
            {
                if (cliente == null ||
                    cliente.IdCliente <= 0)
                {
                    throw new ReglaNegocioException(
                        "Todos los clientes de la visita deben estar persistidos.");
                }

                if (!cliente.Activo)
                {
                    throw new ReglaNegocioException(
                        "No se pueden registrar visitas con clientes inactivos.");
                }
            }
        }

        private static void AgregarParametrosVisita(
            SqlCommand command,
            Visita visita)
        {
            command.Parameters.Add(
                "@IdViaje",
                SqlDbType.Int).Value =
                    visita.IdViaje;

            command.Parameters.Add(
                "@Fecha",
                SqlDbType.Date).Value =
                    visita.Fecha.Date;

            command.Parameters.Add(
                "@Observacion",
                SqlDbType.NVarChar,
                1000).Value =
                    visita.Observacion;

            command.Parameters.Add(
                "@LocalidadEncuentro",
                SqlDbType.NVarChar,
                150).Value =
                    visita.LocalidadEncuentro;
        }

        private static void InsertarClientes(
            SqlConnection connection,
            SqlTransaction transaction,
            int idVisita,
            IReadOnlyCollection<Cliente> clientes)
        {
            const string sql = @"
INSERT INTO dbo.VisitaCliente
(
    IdVisita,
    IdCliente
)
VALUES
(
    @IdVisita,
    @IdCliente
);";

            foreach (
                Cliente cliente
                in clientes)
            {
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql,
                            connection,
                            transaction))
                {
                    command.Parameters.Add(
                        "@IdVisita",
                        SqlDbType.Int).Value =
                            idVisita;

                    command.Parameters.Add(
                        "@IdCliente",
                        SqlDbType.Int).Value =
                            cliente.IdCliente;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static VisitaListadoDto CrearDto(
            SqlDataReader reader)
        {
            return new VisitaListadoDto(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdVisita")),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdViaje")),
                reader.GetDateTime(
                    reader.GetOrdinal(
                        "Fecha")),
                LeerTextoObligatorio(
                    reader,
                    "Observacion"),
                LeerTextoObligatorio(
                    reader,
                    "LocalidadEncuentro"),
                LeerTextoOpcional(
                    reader,
                    "ClientesResumen"));
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