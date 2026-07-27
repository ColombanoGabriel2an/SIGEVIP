using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Viajes;

namespace SIGEVIP.Infrastructure.Rendiciones
{
    public sealed class RendicionRepository
        : IRendicionRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        private readonly ViajeRepository
            _viajeRepository;

        public RendicionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));

            _viajeRepository =
                new ViajeRepository(
                    _connectionFactory);
        }

        public Viaje ObtenerPorId(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }

            return _viajeRepository
                .ObtenerPorId(
                    idViaje);
        }

        public IReadOnlyCollection<RendicionListadoDto>
            ListarPendientes()
        {
            const string sql = @"
SELECT
    viaje.IdViaje,
    viaje.FechaInicio,
    viaje.FechaFin,
    viaje.Descripcion,
    viaje.TipoViaje,
    viaje.MontoAnticipado,
    viaje.FechaEnvioRendicion,

    STUFF
    (
        (
            SELECT
                N', ' +
                persona.Apellido +
                N', ' +
                persona.Nombre
            FROM dbo.ViajeParticipante
                AS relacionParticipante
            INNER JOIN dbo.Persona
                AS persona
                ON persona.IdPersona =
                    relacionParticipante.IdPersona
            WHERE relacionParticipante.IdViaje =
                viaje.IdViaje
            ORDER BY
                persona.Apellido,
                persona.Nombre,
                persona.IdPersona
            FOR XML PATH(N''),
                TYPE
        ).value(
            N'.',
            N'nvarchar(max)'
        ),
        1,
        2,
        N''
    ) AS ParticipantesResumen,

    ISNULL
    (
        (
            SELECT
                SUM(viatico.Monto)
            FROM dbo.Viatico AS viatico
            WHERE
                viatico.IdViaje =
                    viaje.IdViaje
                AND viatico.EstadoViatico = 1
        ),
        0
    ) AS TotalGastado
FROM dbo.Viaje AS viaje
WHERE viaje.EstadoViaje = @EstadoEnRendicion
ORDER BY
    viaje.FechaEnvioRendicion,
    viaje.IdViaje;";

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
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    var resultados =
                        new List<RendicionListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal montoAnticipado =
                                reader.GetDecimal(
                                    reader.GetOrdinal(
                                        "MontoAnticipado"));

                            decimal totalGastado =
                                reader.GetDecimal(
                                    reader.GetOrdinal(
                                        "TotalGastado"));

                            resultados.Add(
                                new RendicionListadoDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdViaje")),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "FechaInicio")),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "FechaFin")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Descripcion"),
                                    (TipoViaje)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "TipoViaje")),
                                    LeerTextoOpcional(
                                        reader,
                                        "ParticipantesResumen"),
                                    montoAnticipado,
                                    totalGastado,
                                    totalGastado -
                                        montoAnticipado,
                                    LeerFechaOpcional(
                                        reader,
                                        "FechaEnvioRendicion")));
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
                    "No fue posible listar las rendiciones pendientes.",
                    exception);
            }
        }

        public void Enviar(
            Viaje viaje)
        {
            ValidarViajePersistido(
                viaje);

            if (viaje.EstadoActual !=
                EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse EnRendicion antes de persistir el envío.");
            }

            if (!viaje.IdUsuarioEnvioRendicion.HasValue ||
                viaje.IdUsuarioEnvioRendicion.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "El envío debe conservar un usuario de auditoría válido.");
            }

            if (!viaje.FechaEnvioRendicion.HasValue ||
                viaje.FechaEnvioRendicion.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "El envío debe conservar una fecha de auditoría válida.");
            }

            const string sql = @"
UPDATE dbo.Viaje
SET
    EstadoViaje =
        @EstadoEnRendicion,
    IdUsuarioEnvioRendicion =
        @IdUsuarioEnvioRendicion,
    FechaEnvioRendicion =
        @FechaEnvioRendicion
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje =
        @EstadoAbierto
    AND IdUsuarioEnvioRendicion IS NULL
    AND FechaEnvioRendicion IS NULL;";

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
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    command.Parameters.Add(
                        "@IdUsuarioEnvioRendicion",
                        SqlDbType.Int).Value =
                            viaje
                                .IdUsuarioEnvioRendicion
                                .Value;

                    command.Parameters.Add(
                        "@FechaEnvioRendicion",
                        SqlDbType.DateTime2).Value =
                            viaje
                                .FechaEnvioRendicion
                                .Value;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoAbierto",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.Abierto);

                    connection.Open();

                    int filas =
                        command.ExecuteNonQuery();

                    ExigirUnaFila(
                        filas,
                        "enviar el viaje a rendición");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible enviar el viaje a rendición.",
                    exception);
            }
        }

        public void ExcluirViatico(
            Viaje viaje,
            Viatico viatico)
        {
            ValidarViaticoPersistido(
                viaje,
                viatico);

            if (viaje.EstadoActual !=
                EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse EnRendicion antes de persistir la exclusión.");
            }

            if (viatico.Estado !=
                EstadoViatico.Excluido)
            {
                throw new ReglaNegocioException(
                    "El viático debe encontrarse Excluido antes de persistir la operación.");
            }

            if (string.IsNullOrWhiteSpace(
                viatico.MotivoExclusion))
            {
                throw new ReglaNegocioException(
                    "La exclusión debe conservar un motivo.");
            }

            if (!viatico.IdUsuarioExclusion.HasValue ||
                viatico.IdUsuarioExclusion.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "La exclusión debe conservar un usuario de auditoría válido.");
            }

            if (!viatico.FechaExclusion.HasValue ||
                viatico.FechaExclusion.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La exclusión debe conservar una fecha de auditoría válida.");
            }

            const string sql = @"
UPDATE viatico
SET
    viatico.EstadoViatico =
        @EstadoExcluido,
    viatico.MotivoExclusion =
        @MotivoExclusion,
    viatico.IdUsuarioExclusion =
        @IdUsuarioExclusion,
    viatico.FechaExclusion =
        @FechaExclusion
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
WHERE
    viatico.IdViatico =
        @IdViatico
    AND viatico.IdViaje =
        @IdViaje
    AND viatico.EstadoViatico =
        @EstadoVigente
    AND viaje.EstadoViaje =
        @EstadoEnRendicion;";

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
                        "@EstadoExcluido",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViatico.Excluido);

                    command.Parameters.Add(
                        "@MotivoExclusion",
                        SqlDbType.NVarChar,
                        500).Value =
                            viatico.MotivoExclusion;

                    command.Parameters.Add(
                        "@IdUsuarioExclusion",
                        SqlDbType.Int).Value =
                            viatico
                                .IdUsuarioExclusion
                                .Value;

                    command.Parameters.Add(
                        "@FechaExclusion",
                        SqlDbType.DateTime2).Value =
                            viatico
                                .FechaExclusion
                                .Value;

                    command.Parameters.Add(
                        "@IdViatico",
                        SqlDbType.Int).Value =
                            viatico.IdViatico;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoVigente",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViatico.Vigente);

                    command.Parameters.Add(
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    ExigirUnaFila(
                        command.ExecuteNonQuery(),
                        "excluir el viático");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible excluir el viático.",
                    exception);
            }
        }

        public void ReactivarViatico(
            Viaje viaje,
            Viatico viatico)
        {
            ValidarViaticoPersistido(
                viaje,
                viatico);

            if (viaje.EstadoActual !=
                EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse EnRendicion antes de persistir la reactivación.");
            }

            if (viatico.Estado !=
                EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "El viático debe encontrarse Vigente después de reactivarse.");
            }

            if (!viatico.IdUsuarioReactivacion.HasValue ||
                viatico.IdUsuarioReactivacion.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "La reactivación debe conservar un usuario de auditoría válido.");
            }

            if (!viatico.FechaReactivacion.HasValue ||
                viatico.FechaReactivacion.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La reactivación debe conservar una fecha de auditoría válida.");
            }

            const string sql = @"
UPDATE viatico
SET
    viatico.EstadoViatico =
        @EstadoVigente,
    viatico.IdUsuarioReactivacion =
        @IdUsuarioReactivacion,
    viatico.FechaReactivacion =
        @FechaReactivacion
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
WHERE
    viatico.IdViatico =
        @IdViatico
    AND viatico.IdViaje =
        @IdViaje
    AND viatico.EstadoViatico =
        @EstadoExcluido
    AND viaje.EstadoViaje =
        @EstadoEnRendicion;";

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
                        "@EstadoVigente",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViatico.Vigente);

                    command.Parameters.Add(
                        "@IdUsuarioReactivacion",
                        SqlDbType.Int).Value =
                            viatico
                                .IdUsuarioReactivacion
                                .Value;

                    command.Parameters.Add(
                        "@FechaReactivacion",
                        SqlDbType.DateTime2).Value =
                            viatico
                                .FechaReactivacion
                                .Value;

                    command.Parameters.Add(
                        "@IdViatico",
                        SqlDbType.Int).Value =
                            viatico.IdViatico;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoExcluido",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViatico.Excluido);

                    command.Parameters.Add(
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    ExigirUnaFila(
                        command.ExecuteNonQuery(),
                        "reactivar el viático");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible reactivar el viático.",
                    exception);
            }
        }

        public void AjustarMontoAnticipado(
            Viaje viaje)
        {
            ValidarViajePersistido(
                viaje);

            if (viaje.EstadoActual !=
                EstadoViaje.EnRendicion)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse EnRendicion antes de persistir el ajuste.");
            }

            if (viaje.MontoAnticipado < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto anticipado no puede ser negativo.");
            }

            const string sql = @"
UPDATE dbo.Viaje
SET MontoAnticipado =
    @MontoAnticipado
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje =
        @EstadoEnRendicion;";

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
                    SqlParameter monto =
                        command.Parameters.Add(
                            "@MontoAnticipado",
                            SqlDbType.Decimal);

                    monto.Precision = 18;
                    monto.Scale = 2;
                    monto.Value =
                        viaje.MontoAnticipado;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    ExigirUnaFila(
                        command.ExecuteNonQuery(),
                        "ajustar el monto anticipado");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible ajustar el monto anticipado.",
                    exception);
            }
        }

        public void Aprobar(
            Viaje viaje)
        {
            ValidarViajePersistido(
                viaje);

            if (viaje.EstadoActual !=
                EstadoViaje.Aprobado)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse Aprobado antes de persistir la operación.");
            }

            if (!viaje.IdUsuarioAprobador.HasValue ||
                viaje.IdUsuarioAprobador.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "La aprobación debe conservar un usuario de auditoría válido.");
            }

            if (!viaje.FechaAprobacion.HasValue ||
                viaje.FechaAprobacion.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La aprobación debe conservar una fecha de auditoría válida.");
            }

            const string sql = @"
UPDATE dbo.Viaje
SET
    EstadoViaje =
        @EstadoAprobado,
    IdUsuarioAprobador =
        @IdUsuarioAprobador,
    FechaAprobacion =
        @FechaAprobacion
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje =
        @EstadoEnRendicion
    AND IdUsuarioAprobador IS NULL
    AND FechaAprobacion IS NULL;";

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
                        "@EstadoAprobado",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.Aprobado);

                    command.Parameters.Add(
                        "@IdUsuarioAprobador",
                        SqlDbType.Int).Value =
                            viaje
                                .IdUsuarioAprobador
                                .Value;

                    command.Parameters.Add(
                        "@FechaAprobacion",
                        SqlDbType.DateTime2).Value =
                            viaje
                                .FechaAprobacion
                                .Value;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    ExigirUnaFila(
                        command.ExecuteNonQuery(),
                        "aprobar la rendición");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible aprobar la rendición.",
                    exception);
            }
        }

        public void Cancelar(
            Viaje viaje)
        {
            ValidarViajePersistido(
                viaje);

            if (viaje.EstadoActual !=
                EstadoViaje.Cancelado)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse Cancelado antes de persistir la operación.");
            }

            if (string.IsNullOrWhiteSpace(
                viaje.MotivoCancelacion))
            {
                throw new ReglaNegocioException(
                    "La cancelación debe conservar un motivo.");
            }

            if (!viaje.IdUsuarioCancelacion.HasValue ||
                viaje.IdUsuarioCancelacion.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "La cancelación debe conservar un usuario de auditoría válido.");
            }

            if (!viaje.FechaCancelacion.HasValue ||
                viaje.FechaCancelacion.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La cancelación debe conservar una fecha de auditoría válida.");
            }

            const string sql = @"
UPDATE dbo.Viaje
SET
    EstadoViaje =
        @EstadoCancelado,
    MotivoCancelacion =
        @MotivoCancelacion,
    IdUsuarioCancelacion =
        @IdUsuarioCancelacion,
    FechaCancelacion =
        @FechaCancelacion
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje =
        @EstadoEnRendicion
    AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.Visita AS visita
        WHERE visita.IdViaje =
            dbo.Viaje.IdViaje
    );";

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
                        "@EstadoCancelado",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.Cancelado);

                    command.Parameters.Add(
                        "@MotivoCancelacion",
                        SqlDbType.NVarChar,
                        500).Value =
                            viaje.MotivoCancelacion;

                    command.Parameters.Add(
                        "@IdUsuarioCancelacion",
                        SqlDbType.Int).Value =
                            viaje
                                .IdUsuarioCancelacion
                                .Value;

                    command.Parameters.Add(
                        "@FechaCancelacion",
                        SqlDbType.DateTime2).Value =
                            viaje
                                .FechaCancelacion
                                .Value;

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    command.Parameters.Add(
                        "@EstadoEnRendicion",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                EstadoViaje.EnRendicion);

                    connection.Open();

                    ExigirUnaFila(
                        command.ExecuteNonQuery(),
                        "cancelar la rendición");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible cancelar la rendición.",
                    exception);
            }
        }

        private static void ValidarViaticoPersistido(
            Viaje viaje,
            Viatico viatico)
        {
            ValidarViajePersistido(
                viaje);

            if (viatico == null)
            {
                throw new ArgumentNullException(
                    nameof(viatico));
            }

            if (viatico.IdViatico <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viatico),
                    "El viático debe tener un identificador persistido.");
            }

            if (viatico.IdViaje !=
                viaje.IdViaje)
            {
                throw new ReglaNegocioException(
                    "El viático no pertenece al viaje indicado.");
            }
        }

        private static void ValidarViajePersistido(
            Viaje viaje)
        {
            if (viaje == null)
            {
                throw new ArgumentNullException(
                    nameof(viaje));
            }

            if (viaje.IdViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viaje),
                    "El viaje debe tener un identificador persistido.");
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
                    " porque el viaje no existe o su estado cambió.");
            }
        }

        private static DateTime? LeerFechaOpcional(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            return reader.IsDBNull(
                ordinal)
                    ? (DateTime?)null
                    : reader.GetDateTime(
                        ordinal);
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
