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
            throw new NotSupportedException(
                "La exclusión de viáticos se implementará en el incremento 3A.3.2.2.");
        }

        public void ReactivarViatico(
            Viaje viaje,
            Viatico viatico)
        {
            throw new NotSupportedException(
                "La reactivación de viáticos se implementará en el incremento 3A.3.2.2.");
        }

        public void AjustarMontoAnticipado(
            Viaje viaje)
        {
            throw new NotSupportedException(
                "El ajuste del anticipo se implementará en el incremento 3A.3.2.2.");
        }

        public void Aprobar(
            Viaje viaje)
        {
            throw new NotSupportedException(
                "La aprobación se implementará en el incremento 3A.3.2.2.");
        }

        public void Cancelar(
            Viaje viaje)
        {
            throw new NotSupportedException(
                "La cancelación se implementará en el incremento 3A.3.2.2.");
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
