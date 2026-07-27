using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Viajes
{
    public sealed class ViajeRepository
        : IViajeRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public ViajeRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public Viaje ObtenerPorId(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            const string sqlViaje = @"
SELECT
    v.IdViaje,
    v.FechaInicio,
    v.FechaFin,
    v.Descripcion,
    v.TipoViaje,
    v.MontoAnticipado,
    v.EstadoViaje,
    v.IdUsuarioEnvioRendicion,
    v.FechaEnvioRendicion,
    v.IdUsuarioAprobador,
    v.FechaAprobacion,
    v.MotivoCancelacion,
    v.IdUsuarioCancelacion,
    v.FechaCancelacion
FROM dbo.Viaje AS v
WHERE v.IdViaje = @IdViaje;";

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                {
                    connection.Open();

                    DatosViaje datos;

                    using (
                        SqlCommand command =
                            new SqlCommand(
                                sqlViaje,
                                connection))
                    {
                        command.Parameters.Add(
                            "@IdViaje",
                            SqlDbType.Int).Value =
                                idViaje;

                        using (
                            SqlDataReader reader =
                                command.ExecuteReader(
                                    CommandBehavior.SingleRow))
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            datos =
                                LeerDatosViaje(
                                    reader);
                        }
                    }

                    IReadOnlyCollection<Persona>
                        participantes =
                            ObtenerParticipantes(
                                connection,
                                idViaje);

                    IReadOnlyCollection<Visita>
                        visitas =
                            ObtenerVisitas(
                                connection,
                                idViaje);

                    IReadOnlyCollection<Viatico>
                        viaticos =
                            ObtenerViaticos(
                                connection,
                                idViaje);

                    return Viaje.Reconstruir(
                        datos.IdViaje,
                        datos.FechaInicio,
                        datos.FechaFin,
                        datos.Descripcion,
                        datos.TipoViaje,
                        datos.MontoAnticipado,
                        datos.EstadoViaje,
                        participantes,
                        visitas,
                        viaticos,
                        datos.IdUsuarioEnvioRendicion,
                        datos.FechaEnvioRendicion,
                        datos.IdUsuarioAprobador,
                        datos.FechaAprobacion,
                        datos.MotivoCancelacion,
                        datos.IdUsuarioCancelacion,
                        datos.FechaCancelacion);
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el viaje.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos del viaje son inválidos.",
                    exception);
            }
        }

        public IReadOnlyCollection<ViajeListadoDto> Listar(
            ViajeFiltro filtro)
        {
            ViajeFiltro filtroEfectivo =
                filtro
                ?? ViajeFiltro.CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    v.IdViaje,
    v.FechaInicio,
    v.FechaFin,
    v.Descripcion,
    v.TipoViaje,
    v.MontoAnticipado,
    v.EstadoViaje,
    STUFF
    (
        (
            SELECT
                N', ' +
                p.Apellido +
                N', ' +
                p.Nombre
            FROM dbo.ViajeParticipante AS vpResumen
            INNER JOIN dbo.Persona AS p
                ON p.IdPersona =
                    vpResumen.IdPersona
            WHERE vpResumen.IdViaje =
                v.IdViaje
            ORDER BY
                p.Apellido,
                p.Nombre,
                p.IdPersona
            FOR XML PATH(N''),
                TYPE
        ).value(N'.', N'nvarchar(max)'),
        1,
        2,
        N''
    ) AS ParticipantesResumen
FROM dbo.Viaje AS v
WHERE 1 = 1");

            var parametros =
                new List<SqlParameter>();

            if (filtroEfectivo.FechaDesde.HasValue)
            {
                sql.Append(@"
    AND v.FechaInicio >= @FechaDesde");

                parametros.Add(
                    new SqlParameter(
                        "@FechaDesde",
                        SqlDbType.Date)
                    {
                        Value =
                            filtroEfectivo
                                .FechaDesde
                                .Value
                                .Date
                    });
            }

            if (filtroEfectivo.FechaHasta.HasValue)
            {
                sql.Append(@"
    AND v.FechaInicio <= @FechaHasta");

                parametros.Add(
                    new SqlParameter(
                        "@FechaHasta",
                        SqlDbType.Date)
                    {
                        Value =
                            filtroEfectivo
                                .FechaHasta
                                .Value
                                .Date
                    });
            }

            if (filtroEfectivo.Estado.HasValue)
            {
                sql.Append(@"
    AND v.EstadoViaje = @EstadoViaje");

                parametros.Add(
                    new SqlParameter(
                        "@EstadoViaje",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                filtroEfectivo
                                    .Estado
                                    .Value)
                    });
            }

            if (filtroEfectivo.IdParticipante.HasValue)
            {
                sql.Append(@"
    AND EXISTS
    (
        SELECT 1
        FROM dbo.ViajeParticipante AS vpFiltro
        WHERE
            vpFiltro.IdViaje = v.IdViaje
            AND vpFiltro.IdPersona =
                @IdParticipante
    )");

                parametros.Add(
                    new SqlParameter(
                        "@IdParticipante",
                        SqlDbType.Int)
                    {
                        Value =
                            filtroEfectivo
                                .IdParticipante
                                .Value
                    });
            }

            sql.Append(@"
ORDER BY
    v.FechaInicio DESC,
    v.IdViaje DESC;");

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
                        new List<ViajeListadoDto>();

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
                    "No fue posible listar los viajes.",
                    exception);
            }
        }

        public int Insertar(
            Viaje viaje)
        {
            ValidarViajePersistible(
                viaje,
                false);

            const string sqlViaje = @"
INSERT INTO dbo.Viaje
(
    FechaInicio,
    FechaFin,
    Descripcion,
    TipoViaje,
    MontoAnticipado,
    EstadoViaje
)
VALUES
(
    @FechaInicio,
    @FechaFin,
    @Descripcion,
    @TipoViaje,
    @MontoAnticipado,
    @EstadoViaje
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
                            int idViaje;

                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sqlViaje,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosViaje(
                                    command,
                                    viaje);

                                idViaje =
                                    Convert.ToInt32(
                                        command
                                            .ExecuteScalar());
                            }

                            InsertarParticipantes(
                                connection,
                                transaction,
                                idViaje,
                                viaje.Participantes);

                            transaction.Commit();

                            return idViaje;
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
                    "No fue posible registrar el viaje.",
                    exception);
            }
        }

        public void Actualizar(
            Viaje viaje)
        {
            ValidarViajePersistible(
                viaje,
                true);

            const string sqlViaje = @"
UPDATE dbo.Viaje
SET
    FechaInicio = @FechaInicio,
    FechaFin = @FechaFin,
    Descripcion = @Descripcion,
    TipoViaje = @TipoViaje,
    MontoAnticipado = @MontoAnticipado
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje = 1;";

            const string sqlEliminarParticipantes = @"
DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @IdViaje;";

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
                                        sqlViaje,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosViaje(
                                    command,
                                    viaje);

                                command.Parameters.Add(
                                    "@IdViaje",
                                    SqlDbType.Int).Value =
                                        viaje.IdViaje;

                                int filas =
                                    command.ExecuteNonQuery();

                                ExigirUnaFila(
                                    filas,
                                    "actualizar");
                            }

                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sqlEliminarParticipantes,
                                        connection,
                                        transaction))
                            {
                                command.Parameters.Add(
                                    "@IdViaje",
                                    SqlDbType.Int).Value =
                                        viaje.IdViaje;

                                command.ExecuteNonQuery();
                            }

                            InsertarParticipantes(
                                connection,
                                transaction,
                                viaje.IdViaje,
                                viaje.Participantes);

                            transaction.Commit();
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
                    "No fue posible actualizar el viaje.",
                    exception);
            }
        }

        public void Cancelar(
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

            if (viaje.EstadoActual !=
                EstadoViaje.Cancelado)
            {
                throw new ReglaNegocioException(
                    "El viaje debe encontrarse Cancelado antes de persistir su estado.");
            }

            const string sql = @"
UPDATE dbo.Viaje
SET EstadoViaje = @EstadoViaje
WHERE
    IdViaje = @IdViaje
    AND EstadoViaje IN (1, 2)
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
                        "@EstadoViaje",
                        SqlDbType.TinyInt).Value =
                            Convert.ToByte(
                                viaje.EstadoActual);

                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            viaje.IdViaje;

                    connection.Open();

                    int filas =
                        command.ExecuteNonQuery();

                    ExigirUnaFila(
                        filas,
                        "cancelar");
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible cancelar el viaje.",
                    exception);
            }
        }

        private static IReadOnlyCollection<Persona>
            ObtenerParticipantes(
                SqlConnection connection,
                int idViaje)
        {
            const string sql = @"
SELECT
    p.IdPersona,
    p.Nombre,
    p.Apellido,
    p.Email,
    p.Activo
FROM dbo.ViajeParticipante AS vp
INNER JOIN dbo.Persona AS p
    ON p.IdPersona = vp.IdPersona
WHERE vp.IdViaje = @IdViaje
ORDER BY
    p.Apellido,
    p.Nombre,
    p.IdPersona;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        idViaje;

                var participantes =
                    new List<Persona>();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    while (reader.Read())
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

                        participantes.Add(
                            persona);
                    }
                }

                return participantes.AsReadOnly();
            }
        }

        private static IReadOnlyCollection<Visita>
            ObtenerVisitas(
                SqlConnection connection,
                int idViaje)
        {
            const string sqlVisitas = @"
SELECT
    visita.IdVisita,
    visita.IdViaje,
    visita.Fecha,
    visita.Observacion,
    visita.LocalidadEncuentro
FROM dbo.Visita AS visita
WHERE visita.IdViaje = @IdViaje
ORDER BY
    visita.Fecha,
    visita.IdVisita;";

            var datosVisitas =
                new List<DatosVisitaPersistida>();

            using (
                SqlCommand command =
                    new SqlCommand(
                        sqlVisitas,
                        connection))
            {
                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        idViaje;

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datosVisitas.Add(
                            new DatosVisitaPersistida(
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
                                    "LocalidadEncuentro")));
                    }
                }
            }

            var visitas =
                new List<Visita>();

            foreach (
                DatosVisitaPersistida datos
                in datosVisitas)
            {
                IReadOnlyCollection<Cliente>
                    clientes =
                        ObtenerClientesVisita(
                            connection,
                            datos.IdVisita);

                visitas.Add(
                    Visita.Reconstruir(
                        datos.IdVisita,
                        datos.IdViaje,
                        datos.Fecha,
                        datos.Observacion,
                        datos.LocalidadEncuentro,
                        clientes));
            }

            return visitas.AsReadOnly();
        }

        private static IReadOnlyCollection<Cliente>
            ObtenerClientesVisita(
                SqlConnection connection,
                int idVisita)
        {
            const string sql = @"
SELECT
    cliente.IdCliente,
    cliente.RazonSocial,
    cliente.Cuit,
    cliente.Email,
    cliente.Telefono,
    cliente.Localidad,
    cliente.Provincia,
    cliente.Activo
FROM dbo.VisitaCliente AS relacion
INNER JOIN dbo.Cliente AS cliente
    ON cliente.IdCliente =
        relacion.IdCliente
WHERE relacion.IdVisita = @IdVisita
ORDER BY
    cliente.RazonSocial,
    cliente.IdCliente;";

            var clientes =
                new List<Cliente>();

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdVisita",
                    SqlDbType.Int).Value =
                        idVisita;

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(
                            Cliente.Reconstruir(
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
                                        "Activo"))));
                    }
                }
            }

            return clientes.AsReadOnly();
        }


        private static IReadOnlyCollection<Viatico>
            ObtenerViaticos(
                SqlConnection connection,
                int idViaje)
        {
            const string sql = @"
SELECT
    viatico.IdViatico,
    viatico.IdViaje,
    viatico.Fecha,
    viatico.CategoriaGasto,
    viatico.MetodoPago,
    viatico.IdPersonaPagadora,
    viatico.Monto,
    viatico.Descripcion,
    viatico.EstadoViatico,
    viatico.MotivoExclusion,
    viatico.IdUsuarioExclusion,
    viatico.FechaExclusion,
    viatico.IdUsuarioReactivacion,
    viatico.FechaReactivacion,

    persona.IdPersona
        AS PagadorIdPersona,
    persona.Nombre
        AS PagadorNombre,
    persona.Apellido
        AS PagadorApellido,
    persona.Email
        AS PagadorEmail,
    persona.Activo
        AS PagadorActivo,

    comprobante.IdComprobante,
    comprobante.TipoComprobante,
    comprobante.CuitProveedor,
    comprobante.RazonSocialProveedor,
    comprobante.SituacionFiscal,
    comprobante.Sucursal,
    comprobante.Numero,
    comprobante.MontoGravado,
    comprobante.MontoImpuestos
FROM dbo.Viatico AS viatico
LEFT JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora
LEFT JOIN dbo.Comprobante AS comprobante
    ON comprobante.IdViatico =
        viatico.IdViatico
WHERE viatico.IdViaje = @IdViaje
ORDER BY
    viatico.Fecha,
    viatico.IdViatico;";

            var viaticos =
                new List<Viatico>();

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        idViaje;

                using (
                    SqlDataReader reader =
                        command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viaticos.Add(
                            ReconstruirViatico(
                                reader));
                    }
                }
            }

            return viaticos.AsReadOnly();
        }

        private static Viatico ReconstruirViatico(
            SqlDataReader reader)
        {
            Persona pagador =
                ReconstruirPagadorViatico(
                    reader);

            Comprobante comprobante =
                ReconstruirComprobanteViatico(
                    reader);

            return Viatico.Reconstruir(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdViatico")),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdViaje")),
                reader.GetDateTime(
                    reader.GetOrdinal(
                        "Fecha")),
                (CategoriaGasto)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "CategoriaGasto")),
                (MetodoPago)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "MetodoPago")),
                pagador,
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "Monto")),
                LeerTextoPermitidoVacio(
                    reader,
                    "Descripcion"),
                comprobante,
                (EstadoViatico)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "EstadoViatico")),
                LeerTextoOpcionalNulo(
                    reader,
                    "MotivoExclusion"),
                LeerEnteroOpcional(
                    reader,
                    "IdUsuarioExclusion"),
                LeerFechaOpcional(
                    reader,
                    "FechaExclusion"),
                LeerEnteroOpcional(
                    reader,
                    "IdUsuarioReactivacion"),
                LeerFechaOpcional(
                    reader,
                    "FechaReactivacion"));
        }

        private static Persona
            ReconstruirPagadorViatico(
                SqlDataReader reader)
        {
            int ordinalId =
                reader.GetOrdinal(
                    "PagadorIdPersona");

            if (reader.IsDBNull(
                ordinalId))
            {
                return null;
            }

            var persona =
                new Persona(
                    reader.GetInt32(
                        ordinalId),
                    LeerTextoObligatorio(
                        reader,
                        "PagadorNombre"),
                    LeerTextoObligatorio(
                        reader,
                        "PagadorApellido"),
                    LeerTextoObligatorio(
                        reader,
                        "PagadorEmail"));

            if (!reader.GetBoolean(
                reader.GetOrdinal(
                    "PagadorActivo")))
            {
                persona.Desactivar();
            }

            return persona;
        }

        private static Comprobante
            ReconstruirComprobanteViatico(
                SqlDataReader reader)
        {
            int ordinalId =
                reader.GetOrdinal(
                    "IdComprobante");

            if (reader.IsDBNull(
                ordinalId))
            {
                return null;
            }

            return new Comprobante(
                reader.GetInt32(
                    ordinalId),
                (TipoComprobante)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "TipoComprobante")),
                LeerTextoObligatorio(
                    reader,
                    "CuitProveedor"),
                LeerTextoObligatorio(
                    reader,
                    "RazonSocialProveedor"),
                (SituacionFiscal)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "SituacionFiscal")),
                LeerTextoObligatorio(
                    reader,
                    "Sucursal"),
                LeerTextoObligatorio(
                    reader,
                    "Numero"),
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "MontoGravado")),
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "MontoImpuestos")));
        }

        private static void InsertarParticipantes(
            SqlConnection connection,
            SqlTransaction transaction,
            int idViaje,
            IReadOnlyCollection<Persona> participantes)
        {
            const string sql = @"
INSERT INTO dbo.ViajeParticipante
(
    IdViaje,
    IdPersona
)
VALUES
(
    @IdViaje,
    @IdPersona
);";

            foreach (
                Persona participante
                in participantes)
            {
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql,
                            connection,
                            transaction))
                {
                    command.Parameters.Add(
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            idViaje;

                    command.Parameters.Add(
                        "@IdPersona",
                        SqlDbType.Int).Value =
                            participante.IdPersona;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AgregarParametrosViaje(
            SqlCommand command,
            Viaje viaje)
        {
            command.Parameters.Add(
                "@FechaInicio",
                SqlDbType.Date).Value =
                    viaje.FechaInicio.Date;

            command.Parameters.Add(
                "@FechaFin",
                SqlDbType.Date).Value =
                    viaje.FechaFin.Date;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                500).Value =
                    viaje.Descripcion;

            command.Parameters.Add(
                "@TipoViaje",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        viaje.TipoViaje);

            SqlParameter monto =
                command.Parameters.Add(
                    "@MontoAnticipado",
                    SqlDbType.Decimal);

            monto.Precision = 18;
            monto.Scale = 2;
            monto.Value =
                viaje.MontoAnticipado;

            command.Parameters.Add(
                "@EstadoViaje",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        viaje.EstadoActual);
        }

        private static void ValidarViajePersistible(
            Viaje viaje,
            bool exigirId)
        {
            if (viaje == null)
            {
                throw new ArgumentNullException(
                    nameof(viaje));
            }

            if (exigirId &&
                viaje.IdViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viaje),
                    "El viaje debe tener un identificador persistido.");
            }

            if (!exigirId &&
                viaje.IdViaje < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viaje),
                    "El identificador del viaje no puede ser negativo.");
            }

            if (viaje.Participantes == null ||
                viaje.Participantes.Count == 0)
            {
                throw new ReglaNegocioException(
                    "El viaje debe tener al menos un participante.");
            }

            foreach (
                Persona participante
                in viaje.Participantes)
            {
                if (participante == null ||
                    participante.IdPersona <= 0)
                {
                    throw new ReglaNegocioException(
                        "Todos los participantes deben estar persistidos.");
                }
            }
        }

        private static DatosViaje LeerDatosViaje(
            SqlDataReader reader)
        {
            return new DatosViaje(
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
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "MontoAnticipado")),
                (EstadoViaje)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "EstadoViaje")),
                LeerEnteroOpcional(
                    reader,
                    "IdUsuarioEnvioRendicion"),
                LeerFechaOpcional(
                    reader,
                    "FechaEnvioRendicion"),
                LeerEnteroOpcional(
                    reader,
                    "IdUsuarioAprobador"),
                LeerFechaOpcional(
                    reader,
                    "FechaAprobacion"),
                LeerTextoOpcionalNulo(
                    reader,
                    "MotivoCancelacion"),
                LeerEnteroOpcional(
                    reader,
                    "IdUsuarioCancelacion"),
                LeerFechaOpcional(
                    reader,
                    "FechaCancelacion"));
        }

        private static ViajeListadoDto CrearDto(
            SqlDataReader reader)
        {
            return new ViajeListadoDto(
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
                (EstadoViaje)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "EstadoViaje")),
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "MontoAnticipado")),
                LeerTextoOpcional(
                    reader,
                    "ParticipantesResumen"));
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
                    " el viaje porque no existe o su estado cambió.");
            }
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


        private static int? LeerEnteroOpcional(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            return reader.IsDBNull(
                ordinal)
                    ? (int?)null
                    : reader.GetInt32(
                        ordinal);
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

        private static string LeerTextoOpcionalNulo(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            return reader.IsDBNull(
                ordinal)
                    ? null
                    : reader.GetString(
                        ordinal);
        }

        private static string LeerTextoPermitidoVacio(
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

            return reader.GetString(
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

        private sealed class DatosVisitaPersistida
        {
            public DatosVisitaPersistida(
                int idVisita,
                int idViaje,
                DateTime fecha,
                string observacion,
                string localidadEncuentro)
            {
                IdVisita = idVisita;
                IdViaje = idViaje;
                Fecha = fecha;
                Observacion = observacion;
                LocalidadEncuentro =
                    localidadEncuentro;
            }

            public int IdVisita { get; private set; }

            public int IdViaje { get; private set; }

            public DateTime Fecha { get; private set; }

            public string Observacion { get; private set; }

            public string LocalidadEncuentro
            {
                get;
                private set;
            }
        }

        private sealed class DatosViaje
        {
            public DatosViaje(
                int idViaje,
                DateTime fechaInicio,
                DateTime fechaFin,
                string descripcion,
                TipoViaje tipoViaje,
                decimal montoAnticipado,
                EstadoViaje estadoViaje,
                int? idUsuarioEnvioRendicion,
                DateTime? fechaEnvioRendicion,
                int? idUsuarioAprobador,
                DateTime? fechaAprobacion,
                string motivoCancelacion,
                int? idUsuarioCancelacion,
                DateTime? fechaCancelacion)
            {
                IdViaje = idViaje;
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                Descripcion = descripcion;
                TipoViaje = tipoViaje;
                MontoAnticipado =
                    montoAnticipado;
                EstadoViaje =
                    estadoViaje;
                IdUsuarioEnvioRendicion =
                    idUsuarioEnvioRendicion;
                FechaEnvioRendicion =
                    fechaEnvioRendicion;
                IdUsuarioAprobador =
                    idUsuarioAprobador;
                FechaAprobacion =
                    fechaAprobacion;
                MotivoCancelacion =
                    motivoCancelacion;
                IdUsuarioCancelacion =
                    idUsuarioCancelacion;
                FechaCancelacion =
                    fechaCancelacion;
            }

            public int IdViaje { get; private set; }

            public DateTime FechaInicio { get; private set; }

            public DateTime FechaFin { get; private set; }

            public string Descripcion { get; private set; }

            public TipoViaje TipoViaje { get; private set; }

            public decimal MontoAnticipado
            {
                get;
                private set;
            }

            public EstadoViaje EstadoViaje
            {
                get;
                private set;
            }

            public int? IdUsuarioEnvioRendicion
            {
                get;
                private set;
            }

            public DateTime? FechaEnvioRendicion
            {
                get;
                private set;
            }

            public int? IdUsuarioAprobador
            {
                get;
                private set;
            }

            public DateTime? FechaAprobacion
            {
                get;
                private set;
            }

            public string MotivoCancelacion
            {
                get;
                private set;
            }

            public int? IdUsuarioCancelacion
            {
                get;
                private set;
            }

            public DateTime? FechaCancelacion
            {
                get;
                private set;
            }
        }
    }
}
