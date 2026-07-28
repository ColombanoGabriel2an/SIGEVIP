using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Clientes
{
    public sealed class ClienteRepository
        : IClienteRepository
    {
        private const int ErrorIndiceDuplicado = 2601;
        private const int ErrorRestriccionUnica = 2627;

        private readonly SqlConnectionFactory
            _connectionFactory;

        public ClienteRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public Cliente ObtenerPorId(
            int idCliente)
        {
            if (idCliente <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idCliente));
            }

            const string sql = @"
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
WHERE c.IdCliente = @IdCliente;";

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
                        "@IdCliente",
                        SqlDbType.Int).Value =
                            idCliente;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader(
                                CommandBehavior.SingleRow))
                    {
                        return reader.Read()
                            ? ReconstruirCliente(reader)
                            : null;
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
                    "No fue posible consultar el cliente.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos del cliente son inválidos.",
                    exception);
            }
        }

        public bool ExisteCuit(
            string cuit,
            int? idClienteExcluido)
        {
            string cuitNormalizado =
                NormalizarCuit(cuit);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Cliente AS c
            WHERE
                c.Cuit = @Cuit
                AND
                (
                    @IdClienteExcluido IS NULL
                    OR c.IdCliente <> @IdClienteExcluido
                )
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
                        "@Cuit",
                        SqlDbType.NVarChar,
                        20).Value =
                            cuitNormalizado;

                    SqlParameter idExcluido =
                        command.Parameters.Add(
                            "@IdClienteExcluido",
                            SqlDbType.Int);

                    idExcluido.Value =
                        idClienteExcluido.HasValue
                            ? (object)idClienteExcluido.Value
                            : DBNull.Value;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible verificar la existencia del CUIT.",
                    exception);
            }
        }

        public IReadOnlyCollection<ClienteListadoDto> Listar(
            ClienteFiltro filtro)
        {
            ClienteFiltro filtroEfectivo =
                filtro
                ?? ClienteFiltro.CrearSinFiltros();

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
WHERE 1 = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            AgregarFiltroTextoGeneral(
                sql,
                parametros,
                filtroEfectivo.TextoGeneral);

            AgregarFiltroTexto(
                sql,
                parametros,
                "c.Cuit",
                "@Cuit",
                NormalizarCuit(
                    filtroEfectivo.Cuit),
                20);

            AgregarFiltroTexto(
                sql,
                parametros,
                "c.Localidad",
                "@Localidad",
                filtroEfectivo.Localidad,
                100);

            AgregarFiltroTexto(
                sql,
                parametros,
                "c.Provincia",
                "@Provincia",
                filtroEfectivo.Provincia,
                100);

            if (filtroEfectivo.Activo.HasValue)
            {
                sql.Append(@"
    AND c.Activo = @Activo");

                parametros.Add(
                    new SqlParameter(
                        "@Activo",
                        SqlDbType.Bit)
                    {
                        Value =
                            filtroEfectivo.Activo.Value
                    });
            }

            sql.Append(@"
ORDER BY
    c.RazonSocial,
    c.IdCliente;");

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

                    List<ClienteListadoDto> resultados =
                        new List<ClienteListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                CrearDto(reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los clientes.",
                    exception);
            }
        }

        public int Insertar(
            Cliente cliente)
        {
            return InsertarInterno(
                cliente,
                null);
        }

        public int Insertar(
            Cliente cliente,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            return InsertarInterno(
                cliente,
                auditoria);
        }

        private int InsertarInterno(
            Cliente cliente,
            AuditoriaRegistro auditoria)
        {
            if (cliente == null)
            {
                throw new ArgumentNullException(
                    nameof(cliente));
            }

            const string sql = @"
INSERT INTO dbo.Cliente
(
    RazonSocial,
    Cuit,
    Email,
    Telefono,
    Localidad,
    Provincia,
    Activo
)
VALUES
(
    @RazonSocial,
    @Cuit,
    @Email,
    @Telefono,
    @Localidad,
    @Provincia,
    @Activo
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
                            int idCliente;

                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sql,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosCliente(
                                    command,
                                    cliente);

                                idCliente =
                                    Convert.ToInt32(
                                        command.ExecuteScalar());
                            }

                            if (auditoria != null)
                            {
                                AuditoriaSqlWriter.Insertar(
                                    connection,
                                    transaction,
                                    auditoria.ConIdEntidad(
                                        idCliente));
                            }

                            transaction.Commit();

                            return idCliente;
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
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
                when (EsErrorCuitDuplicado(
                    exception))
            {
                throw new ReglaNegocioException(
                    "Ya existe un cliente con el CUIT indicado.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar el cliente.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de registro del cliente no pudo completarse.",
                    exception);
            }
        }

        public void Actualizar(
            Cliente cliente)
        {
            ActualizarInterno(
                cliente,
                null);
        }

        public void Actualizar(
            Cliente cliente,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarInterno(
                cliente,
                auditoria);
        }

        private void ActualizarInterno(
            Cliente cliente,
            AuditoriaRegistro auditoria)
        {
            if (cliente == null)
            {
                throw new ArgumentNullException(
                    nameof(cliente));
            }

            const string sql = @"
UPDATE dbo.Cliente
SET
    RazonSocial = @RazonSocial,
    Cuit = @Cuit,
    Email = @Email,
    Telefono = @Telefono,
    Localidad = @Localidad,
    Provincia = @Provincia
WHERE IdCliente = @IdCliente;";

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
                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sql,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosCliente(
                                    command,
                                    cliente);

                                command.Parameters.Add(
                                    "@IdCliente",
                                    SqlDbType.Int).Value =
                                        cliente.IdCliente;

                                ExigirUnaFila(
                                    command.ExecuteNonQuery(),
                                    "actualizar");
                            }

                            if (auditoria != null)
                            {
                                AuditoriaSqlWriter.Insertar(
                                    connection,
                                    transaction,
                                    auditoria);
                            }

                            transaction.Commit();
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
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
                when (EsErrorCuitDuplicado(
                    exception))
            {
                throw new ReglaNegocioException(
                    "Ya existe otro cliente con el CUIT indicado.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible actualizar el cliente.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de actualización del cliente no pudo completarse.",
                    exception);
            }
        }

        public void Activar(
            int idCliente)
        {
            ActualizarEstadoInterno(
                idCliente,
                true,
                null);
        }

        public void Activar(
            int idCliente,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idCliente,
                true,
                auditoria);
        }

        public void Desactivar(
            int idCliente)
        {
            ActualizarEstadoInterno(
                idCliente,
                false,
                null);
        }

        public void Desactivar(
            int idCliente,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idCliente,
                false,
                auditoria);
        }

        private void ActualizarEstadoInterno(
            int idCliente,
            bool activo,
            AuditoriaRegistro auditoria)
        {
            if (idCliente <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idCliente));
            }

            const string sql = @"
UPDATE dbo.Cliente
SET Activo = @Activo
WHERE IdCliente = @IdCliente;";

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
                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sql,
                                        connection,
                                        transaction))
                            {
                                command.Parameters.Add(
                                    "@Activo",
                                    SqlDbType.Bit).Value =
                                        activo;

                                command.Parameters.Add(
                                    "@IdCliente",
                                    SqlDbType.Int).Value =
                                        idCliente;

                                ExigirUnaFila(
                                    command.ExecuteNonQuery(),
                                    activo
                                        ? "activar"
                                        : "desactivar");
                            }

                            if (auditoria != null)
                            {
                                AuditoriaSqlWriter.Insertar(
                                    connection,
                                    transaction,
                                    auditoria);
                            }

                            transaction.Commit();
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
            {
                throw CrearErrorPersistencia(
                    "No fue posible actualizar el estado del cliente.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de estado del cliente no pudo completarse.",
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

        private static ClienteListadoDto CrearDto(
            SqlDataReader reader)
        {
            return new ClienteListadoDto(
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

        private static void AgregarParametrosCliente(
            SqlCommand command,
            Cliente cliente)
        {
            command.Parameters.Add(
                "@RazonSocial",
                SqlDbType.NVarChar,
                150).Value =
                    cliente.RazonSocial;

            command.Parameters.Add(
                "@Cuit",
                SqlDbType.NVarChar,
                20).Value =
                    cliente.Cuit;

            AgregarParametroOpcional(
                command,
                "@Email",
                cliente.Email,
                150);

            AgregarParametroOpcional(
                command,
                "@Telefono",
                cliente.Telefono,
                50);

            AgregarParametroOpcional(
                command,
                "@Localidad",
                cliente.Localidad,
                100);

            AgregarParametroOpcional(
                command,
                "@Provincia",
                cliente.Provincia,
                100);

            command.Parameters.Add(
                "@Activo",
                SqlDbType.Bit).Value =
                    cliente.Activo;
        }

        private static void AgregarParametroOpcional(
            SqlCommand command,
            string nombre,
            string valor,
            int longitud)
        {
            command.Parameters.Add(
                nombre,
                SqlDbType.NVarChar,
                longitud).Value =
                    string.IsNullOrWhiteSpace(valor)
                        ? (object)DBNull.Value
                        : valor;
        }

        private static void AgregarFiltroTextoGeneral(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return;
            }

            sql.Append(@"
    AND
    (
        c.RazonSocial LIKE @TextoGeneral
        OR c.Cuit LIKE @TextoGeneral
        OR c.Email LIKE @TextoGeneral
        OR c.Telefono LIKE @TextoGeneral
        OR c.Localidad LIKE @TextoGeneral
        OR c.Provincia LIKE @TextoGeneral
    )");

            parametros.Add(
                new SqlParameter(
                    "@TextoGeneral",
                    SqlDbType.NVarChar,
                    150)
                {
                    Value =
                        "%" +
                        texto.Trim() +
                        "%"
                });
        }

        private static void AgregarFiltroTexto(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string columna,
            string nombreParametro,
            string valor,
            int longitud)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            sql.Append(
                "\n    AND " +
                columna +
                " LIKE " +
                nombreParametro);

            parametros.Add(
                new SqlParameter(
                    nombreParametro,
                    SqlDbType.NVarChar,
                    longitud)
                {
                    Value =
                        "%" +
                        valor.Trim() +
                        "%"
                });
        }

        private static void RevertirSiCorresponde(
            SqlTransaction transaction)
        {
            if (transaction == null ||
                transaction.Connection == null)
            {
                return;
            }

            try
            {
                transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
            }
            catch (SqlException)
            {
            }
        }

        private static void ExigirUnaFila(
            int filas,
            string operacion)
        {
            if (filas != 1)
            {
                throw new PersistenciaException(
                    "No fue posible " +
                    operacion +
                    " el cliente porque el registro no existe.");
            }
        }

        private static bool EsErrorCuitDuplicado(
            SqlException exception)
        {
            return exception.Number ==
                ErrorIndiceDuplicado
                || exception.Number ==
                ErrorRestriccionUnica;
        }

        private static PersistenciaException CrearErrorPersistencia(
            string mensaje,
            SqlException exception)
        {
            return new PersistenciaException(
                mensaje,
                exception);
        }

        private static string NormalizarCuit(
            string cuit)
        {
            return string.IsNullOrWhiteSpace(cuit)
                ? string.Empty
                : cuit
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .Trim();
        }

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            if (reader.IsDBNull(ordinal))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede ser nula.");
            }

            string valor =
                reader.GetString(
                    ordinal);

            if (string.IsNullOrWhiteSpace(valor))
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

            return reader.IsDBNull(ordinal)
                ? string.Empty
                : reader.GetString(ordinal);
        }
    }
}
