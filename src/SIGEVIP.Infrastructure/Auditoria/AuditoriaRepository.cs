using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Auditoria
{
    public sealed class AuditoriaRepository
        : IAuditoriaRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public AuditoriaRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<AuditoriaListadoDto>
            Listar(
                AuditoriaFiltro filtro)
        {
            AuditoriaFiltro filtroEfectivo =
                filtro
                ?? AuditoriaFiltro
                    .CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    auditoria.IdAuditoria,
    auditoria.FechaHora,
    auditoria.IdUsuario,
    auditoria.NombreUsuario,
    auditoria.Modulo,
    auditoria.Accion,
    auditoria.Entidad,
    auditoria.IdEntidad,
    auditoria.Descripcion
FROM dbo.Auditoria AS auditoria
WHERE 1 = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            AgregarFiltroFechaDesde(
                sql,
                parametros,
                filtroEfectivo.FechaDesde);

            AgregarFiltroFechaHasta(
                sql,
                parametros,
                filtroEfectivo.FechaHasta);

            AgregarFiltroTextoParcial(
                sql,
                parametros,
                "auditoria.NombreUsuario",
                "@NombreUsuario",
                filtroEfectivo.NombreUsuario);

            AgregarFiltroTextoExacto(
                sql,
                parametros,
                "auditoria.Modulo",
                "@Modulo",
                filtroEfectivo.Modulo,
                50);

            AgregarFiltroTextoExacto(
                sql,
                parametros,
                "auditoria.Accion",
                "@Accion",
                filtroEfectivo.Accion,
                50);

            AgregarFiltroGeneral(
                sql,
                parametros,
                filtroEfectivo.TextoGeneral);

            sql.Append(@"
ORDER BY
    auditoria.FechaHora DESC,
    auditoria.IdAuditoria DESC;");

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

                    List<AuditoriaListadoDto>
                        resultados =
                            new List<AuditoriaListadoDto>();

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

                    return resultados
                        .AsReadOnly();
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible consultar los registros de auditoría.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "No fue posible ejecutar la consulta de auditoría.",
                    exception);
            }
        }

        private static void AgregarFiltroFechaDesde(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            DateTime? fechaDesde)
        {
            if (!fechaDesde.HasValue)
            {
                return;
            }

            sql.Append(@"
    AND auditoria.FechaHora >=
        @FechaDesde");

            parametros.Add(
                new SqlParameter(
                    "@FechaDesde",
                    SqlDbType.DateTime2)
                {
                    Value =
                        fechaDesde.Value.Date
                });
        }

        private static void AgregarFiltroFechaHasta(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            DateTime? fechaHasta)
        {
            if (!fechaHasta.HasValue)
            {
                return;
            }

            sql.Append(@"
    AND auditoria.FechaHora <
        @FechaHastaExclusiva");

            parametros.Add(
                new SqlParameter(
                    "@FechaHastaExclusiva",
                    SqlDbType.DateTime2)
                {
                    Value =
                        fechaHasta
                            .Value
                            .Date
                            .AddDays(1)
                });
        }

        private static void AgregarFiltroTextoParcial(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string columna,
            string nombreParametro,
            string valor)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                return;
            }

            sql.Append(@"
    AND " +
                columna +
                " LIKE " +
                nombreParametro);

            parametros.Add(
                new SqlParameter(
                    nombreParametro,
                    SqlDbType.NVarChar,
                    202)
                {
                    Value =
                        "%" +
                        valor.Trim() +
                        "%"
                });
        }

        private static void AgregarFiltroTextoExacto(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string columna,
            string nombreParametro,
            string valor,
            int longitud)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                return;
            }

            sql.Append(@"
    AND " +
                columna +
                " = " +
                nombreParametro);

            parametros.Add(
                new SqlParameter(
                    nombreParametro,
                    SqlDbType.NVarChar,
                    longitud)
                {
                    Value =
                        valor.Trim()
                });
        }

        private static void AgregarFiltroGeneral(
            StringBuilder sql,
            ICollection<SqlParameter> parametros,
            string textoGeneral)
        {
            if (string.IsNullOrWhiteSpace(
                textoGeneral))
            {
                return;
            }

            sql.Append(@"
    AND
    (
        auditoria.NombreUsuario
            LIKE @TextoGeneral
        OR auditoria.Modulo
            LIKE @TextoGeneral
        OR auditoria.Accion
            LIKE @TextoGeneral
        OR auditoria.Entidad
            LIKE @TextoGeneral
        OR auditoria.Descripcion
            LIKE @TextoGeneral
        OR CONVERT(
            NVARCHAR(20),
            auditoria.IdEntidad
        ) LIKE @TextoGeneral
    )");

            parametros.Add(
                new SqlParameter(
                    "@TextoGeneral",
                    SqlDbType.NVarChar,
                    1002)
                {
                    Value =
                        "%" +
                        textoGeneral.Trim() +
                        "%"
                });
        }

        private static AuditoriaListadoDto CrearDto(
            SqlDataReader reader)
        {
            int ordinalIdEntidad =
                reader.GetOrdinal(
                    "IdEntidad");

            return new AuditoriaListadoDto(
                reader.GetInt64(
                    reader.GetOrdinal(
                        "IdAuditoria")),
                reader.GetDateTime(
                    reader.GetOrdinal(
                        "FechaHora")),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdUsuario")),
                LeerTextoObligatorio(
                    reader,
                    "NombreUsuario"),
                LeerTextoObligatorio(
                    reader,
                    "Modulo"),
                LeerTextoObligatorio(
                    reader,
                    "Accion"),
                LeerTextoObligatorio(
                    reader,
                    "Entidad"),
                reader.IsDBNull(
                    ordinalIdEntidad)
                    ? (int?)null
                    : reader.GetInt32(
                        ordinalIdEntidad),
                LeerTextoObligatorio(
                    reader,
                    "Descripcion"));
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
    }
}