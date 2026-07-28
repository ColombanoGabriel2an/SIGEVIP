using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Permisos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Permisos
{
    public sealed class PermisoGestionRepository
        : IPermisoGestionRepository
    {
        private const int ErrorIndiceDuplicado = 2601;
        private const int ErrorRestriccionUnica = 2627;

        private readonly SqlConnectionFactory
            _connectionFactory;

        public PermisoGestionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<PermisoListadoDto>
            Listar(
                PermisoFiltro filtro)
        {
            PermisoFiltro filtroEfectivo =
                filtro
                ?? PermisoFiltro.CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    (
        SELECT COUNT(*)
        FROM dbo.GrupoPermiso AS gp
        WHERE gp.IdPermiso = p.IdPermiso
    ) AS CantidadGrupos,
    p.Activo
FROM dbo.Permiso AS p
WHERE 1 = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            AgregarFiltroTexto(
                sql,
                parametros,
                filtroEfectivo.TextoGeneral);

            if (filtroEfectivo.Activo.HasValue)
            {
                sql.Append(@"
    AND p.Activo = @Activo");

                parametros.Add(
                    new SqlParameter(
                        "@Activo",
                        SqlDbType.Bit)
                    {
                        Value =
                            filtroEfectivo
                                .Activo
                                .Value
                    });
            }

            sql.Append(@"
ORDER BY
    p.Codigo,
    p.IdPermiso;");

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

                    List<PermisoListadoDto> resultados =
                        new List<PermisoListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                CrearPermisoListadoDto(
                                    reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los permisos.",
                    exception);
            }
        }

        public PermisoDetalleDto ObtenerDetallePorId(
            int idPermiso)
        {
            ValidarIdPermiso(
                idPermiso);

            const string sql = @"
SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    (
        SELECT COUNT(*)
        FROM dbo.GrupoPermiso AS gp
        WHERE gp.IdPermiso = p.IdPermiso
    ) AS CantidadGrupos,
    p.Activo
FROM dbo.Permiso AS p
WHERE p.IdPermiso = @IdPermiso;";

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
                        "@IdPermiso",
                        SqlDbType.Int).Value =
                            idPermiso;

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

                        return CrearPermisoDetalleDto(
                            reader);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el detalle del permiso.",
                    exception);
            }
        }

        public Permiso ObtenerPorId(
            int idPermiso)
        {
            ValidarIdPermiso(
                idPermiso);

            const string sql = @"
SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    p.Activo
FROM dbo.Permiso AS p
WHERE p.IdPermiso = @IdPermiso;";

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
                        "@IdPermiso",
                        SqlDbType.Int).Value =
                            idPermiso;

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

                        return ReconstruirPermiso(
                            reader);
                    }
                }
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos del permiso son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el permiso.",
                    exception);
            }
        }

        public bool ExisteCodigo(
            string codigo,
            int? idPermisoExcluido)
        {
            string codigoNormalizado =
                NormalizarTexto(
                    codigo);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Permiso AS p
            WHERE
                p.Codigo = @Codigo
                AND
                (
                    @IdPermisoExcluido IS NULL
                    OR p.IdPermiso <> @IdPermisoExcluido
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
                        "@Codigo",
                        SqlDbType.NVarChar,
                        100).Value =
                            codigoNormalizado;

                    SqlParameter idExcluido =
                        command.Parameters.Add(
                            "@IdPermisoExcluido",
                            SqlDbType.Int);

                    idExcluido.Value =
                        idPermisoExcluido.HasValue
                            ? (object)
                                idPermisoExcluido.Value
                            : DBNull.Value;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible comprobar el código del permiso.",
                    exception);
            }
        }

        public int Insertar(
            Permiso permiso)
        {
            return InsertarInterno(
                permiso,
                null);
        }

        public int Insertar(
            Permiso permiso,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            return InsertarInterno(
                permiso,
                auditoria);
        }

        private int InsertarInterno(
            Permiso permiso,
            AuditoriaRegistro auditoria)
        {
            if (permiso == null)
            {
                throw new ArgumentNullException(
                    nameof(permiso));
            }

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                {
                    connection.Open();

                    using (
                        SqlTransaction transaction =
                            connection.BeginTransaction(
                                IsolationLevel.Serializable))
                    {
                        try
                        {
                            ValidarCodigoUnicoInterno(
                                connection,
                                transaction,
                                permiso.Codigo,
                                null);

                            int idPermiso =
                                InsertarPermisoInterno(
                                    connection,
                                    transaction,
                                    permiso);

                            if (auditoria != null)
                            {
                                AuditoriaSqlWriter.Insertar(
                                    connection,
                                    transaction,
                                    auditoria.ConIdEntidad(
                                        idPermiso));
                            }

                            transaction.Commit();

                            return idPermiso;
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
                when (EsErrorDuplicado(
                    exception))
            {
                throw new ReglaNegocioException(
                    "Ya existe un permiso con el código indicado.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar el permiso.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de registro del permiso no pudo completarse.",
                    exception);
            }
        }

        public void Actualizar(
            Permiso permiso)
        {
            ActualizarInterno(
                permiso,
                null);
        }

        public void Actualizar(
            Permiso permiso,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarInterno(
                permiso,
                auditoria);
        }

        private void ActualizarInterno(
            Permiso permiso,
            AuditoriaRegistro auditoria)
        {
            if (permiso == null)
            {
                throw new ArgumentNullException(
                    nameof(permiso));
            }

            ValidarIdPermiso(
                permiso.IdPermiso);

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                {
                    connection.Open();

                    using (
                        SqlTransaction transaction =
                            connection.BeginTransaction(
                                IsolationLevel.Serializable))
                    {
                        try
                        {
                            ActualizarPermisoInterno(
                                connection,
                                transaction,
                                permiso);

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
                    "No fue posible actualizar el permiso.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de actualización del permiso no pudo completarse.",
                    exception);
            }
        }

        public void Activar(
            int idPermiso)
        {
            ActualizarEstadoInterno(
                idPermiso,
                true,
                null);
        }

        public void Activar(
            int idPermiso,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idPermiso,
                true,
                auditoria);
        }

        public void Desactivar(
            int idPermiso)
        {
            ActualizarEstadoInterno(
                idPermiso,
                false,
                null);
        }

        public void Desactivar(
            int idPermiso,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idPermiso,
                false,
                auditoria);
        }

        private void ActualizarEstadoInterno(
            int idPermiso,
            bool activo,
            AuditoriaRegistro auditoria)
        {
            ValidarIdPermiso(
                idPermiso);

            const string sql = @"
UPDATE dbo.Permiso
SET Activo = @Activo
WHERE IdPermiso = @IdPermiso;";

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
                                    "@IdPermiso",
                                    SqlDbType.Int).Value =
                                        idPermiso;

                                ExigirUnaFilaPermiso(
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
                    "No fue posible actualizar el estado del permiso.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de estado del permiso no pudo completarse.",
                    exception);
            }
        }

        private static void ValidarCodigoUnicoInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            int? idPermisoExcluido)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Permiso AS p
            WHERE
                p.Codigo = @Codigo
                AND
                (
                    @IdPermisoExcluido IS NULL
                    OR p.IdPermiso <> @IdPermisoExcluido
                )
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        NormalizarTexto(
                            codigo);

                SqlParameter idExcluido =
                    command.Parameters.Add(
                        "@IdPermisoExcluido",
                        SqlDbType.Int);

                idExcluido.Value =
                    idPermisoExcluido.HasValue
                        ? (object)
                            idPermisoExcluido.Value
                        : DBNull.Value;

                bool duplicado =
                    Convert.ToBoolean(
                        command.ExecuteScalar());

                if (duplicado)
                {
                    throw new ReglaNegocioException(
                        "Ya existe un permiso con el código indicado.");
                }
            }
        }

        private static int InsertarPermisoInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Permiso permiso)
        {
            const string sql = @"
INSERT INTO dbo.Permiso
(
    Codigo,
    Nombre,
    Descripcion,
    Activo
)
VALUES
(
    @Codigo,
    @Nombre,
    @Descripcion,
    @Activo
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                AgregarParametrosPermiso(
                    command,
                    permiso);

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void ActualizarPermisoInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Permiso permiso)
        {
            const string sql = @"
UPDATE dbo.Permiso
SET
    Nombre = @Nombre,
    Descripcion = @Descripcion
WHERE IdPermiso = @IdPermiso;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        permiso.Nombre;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        ObtenerValorDescripcion(
                            permiso.Descripcion);

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        permiso.IdPermiso;

                int filas =
                    command.ExecuteNonQuery();

                ExigirUnaFilaPermiso(
                    filas,
                    "actualizar");
            }
        }

        private static void AgregarParametrosPermiso(
            SqlCommand command,
            Permiso permiso)
        {
            command.Parameters.Add(
                "@Codigo",
                SqlDbType.NVarChar,
                100).Value =
                    permiso.Codigo;

            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                150).Value =
                    permiso.Nombre;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                500).Value =
                    ObtenerValorDescripcion(
                        permiso.Descripcion);

            command.Parameters.Add(
                "@Activo",
                SqlDbType.Bit).Value =
                    permiso.Activo;
        }

        private static Permiso ReconstruirPermiso(
            SqlDataReader reader)
        {
            Permiso permiso =
                new Permiso(
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdPermiso")),
                    LeerTextoObligatorio(
                        reader,
                        "Codigo"),
                    LeerTextoObligatorio(
                        reader,
                        "Nombre"),
                    LeerTextoOpcional(
                        reader,
                        "Descripcion"));

            bool activo =
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo"));

            if (!activo)
            {
                permiso.Desactivar();
            }

            return permiso;
        }

        private static PermisoListadoDto
            CrearPermisoListadoDto(
                SqlDataReader reader)
        {
            return new PermisoListadoDto(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdPermiso")),
                LeerTextoObligatorio(
                    reader,
                    "Codigo"),
                LeerTextoObligatorio(
                    reader,
                    "Nombre"),
                LeerTextoOpcional(
                    reader,
                    "Descripcion"),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "CantidadGrupos")),
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo")));
        }

        private static PermisoDetalleDto
            CrearPermisoDetalleDto(
                SqlDataReader reader)
        {
            return new PermisoDetalleDto(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdPermiso")),
                LeerTextoObligatorio(
                    reader,
                    "Codigo"),
                LeerTextoObligatorio(
                    reader,
                    "Nombre"),
                LeerTextoOpcional(
                    reader,
                    "Descripcion"),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "CantidadGrupos")),
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo")));
        }

        private static void AgregarFiltroTexto(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return;
            }

            sql.Append(@"
    AND
    (
        p.Codigo LIKE @TextoGeneral
        OR p.Nombre LIKE @TextoGeneral
        OR p.Descripcion LIKE @TextoGeneral
    )");

            parametros.Add(
                new SqlParameter(
                    "@TextoGeneral",
                    SqlDbType.NVarChar,
                    500)
                {
                    Value =
                        "%" +
                        texto.Trim() +
                        "%"
                });
        }

        private static object ObtenerValorDescripcion(
            string descripcion)
        {
            return string.IsNullOrWhiteSpace(
                descripcion)
                ? (object)DBNull.Value
                : descripcion.Trim();
        }

        private static string NormalizarTexto(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }

        private static void ValidarIdPermiso(
            int idPermiso)
        {
            if (idPermiso <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPermiso));
            }
        }

        private static void ExigirUnaFilaPermiso(
            int filas,
            string operacion)
        {
            if (filas != 1)
            {
                throw new PersistenciaException(
                    "No fue posible " +
                    operacion +
                    " el permiso porque el registro no existe.");
            }
        }

        private static bool EsErrorDuplicado(
            SqlException exception)
        {
            return exception.Number ==
                    ErrorIndiceDuplicado
                || exception.Number ==
                    ErrorRestriccionUnica;
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

        private static void RevertirSiCorresponde(
            SqlTransaction transaction)
        {
            if (transaction == null ||
                transaction.Connection == null)
            {
                return;
            }

            transaction.Rollback();
        }

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string nombreColumna)
        {
            int ordinal =
                reader.GetOrdinal(
                    nombreColumna);

            if (reader.IsDBNull(
                ordinal))
            {
                throw new PersistenciaException(
                    "La columna " +
                    nombreColumna +
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
                    nombreColumna +
                    " no puede estar vacía.");
            }

            return valor.Trim();
        }

        private static string LeerTextoOpcional(
            SqlDataReader reader,
            string nombreColumna)
        {
            int ordinal =
                reader.GetOrdinal(
                    nombreColumna);

            if (reader.IsDBNull(
                ordinal))
            {
                return string.Empty;
            }

            string valor =
                reader.GetString(
                    ordinal);

            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}