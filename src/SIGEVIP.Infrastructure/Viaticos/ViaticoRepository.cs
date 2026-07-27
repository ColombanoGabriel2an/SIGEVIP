using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Viaticos
{
    public sealed class ViaticoRepository
        : IViaticoRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public ViaticoRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public Viatico ObtenerPorId(
            int idViatico)
        {
            if (idViatico <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViatico),
                    "El identificador del viático debe ser mayor que cero.");
            }

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
WHERE viatico.IdViatico = @IdViatico;";

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
                        "@IdViatico",
                        SqlDbType.Int).Value =
                            idViatico;

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

                        return ReconstruirViatico(
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
                    "No fue posible consultar el viático.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos del viático son inválidos.",
                    exception);
            }
        }

        public IReadOnlyCollection<ViaticoListadoDto>
            ListarPorViaje(
                int idViaje,
                ViaticoFiltro filtro)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }

            ViaticoFiltro filtroEfectivo =
                filtro
                ?? ViaticoFiltro.CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    viatico.IdViatico,
    viatico.IdViaje,
    viatico.Fecha,
    viatico.CategoriaGasto,
    viatico.MetodoPago,
    CASE
        WHEN persona.IdPersona IS NULL
            THEN N''
        ELSE
            persona.Apellido +
            N', ' +
            persona.Nombre
    END AS PagadoPor,
    viatico.Monto,
    viatico.Descripcion,
    viatico.EstadoViatico,
    CASE
        WHEN comprobante.IdComprobante
            IS NULL
            THEN CAST(0 AS BIT)
        ELSE CAST(1 AS BIT)
    END AS TieneComprobante
FROM dbo.Viatico AS viatico
LEFT JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora
LEFT JOIN dbo.Comprobante AS comprobante
    ON comprobante.IdViatico =
        viatico.IdViatico
WHERE viatico.IdViaje = @IdViaje");

            var parametros =
                new List<SqlParameter>
                {
                    new SqlParameter(
                        "@IdViaje",
                        SqlDbType.Int)
                    {
                        Value = idViaje
                    }
                };

            if (filtroEfectivo.FechaDesde.HasValue)
            {
                sql.Append(@"
    AND viatico.Fecha >= @FechaDesde");

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
    AND viatico.Fecha <= @FechaHasta");

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

            if (filtroEfectivo.Categoria.HasValue)
            {
                sql.Append(@"
    AND viatico.CategoriaGasto =
        @CategoriaGasto");

                parametros.Add(
                    new SqlParameter(
                        "@CategoriaGasto",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                filtroEfectivo
                                    .Categoria
                                    .Value)
                    });
            }

            if (filtroEfectivo.Estado.HasValue)
            {
                sql.Append(@"
    AND viatico.EstadoViatico =
        @EstadoViatico");

                parametros.Add(
                    new SqlParameter(
                        "@EstadoViatico",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                filtroEfectivo
                                    .Estado
                                    .Value)
                    });
            }

            sql.Append(@"
ORDER BY
    viatico.Fecha DESC,
    viatico.IdViatico DESC;");

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
                        new List<ViaticoListadoDto>();

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
                    "No fue posible listar los viáticos del viaje.",
                    exception);
            }
        }

        public int Insertar(
            Viatico viatico)
        {
            ValidarViaticoParaInsertar(
                viatico);

            const string sqlViatico = @"
INSERT INTO dbo.Viatico
(
    IdViaje,
    Fecha,
    CategoriaGasto,
    MetodoPago,
    IdPersonaPagadora,
    Monto,
    Descripcion,
    EstadoViatico,
    MotivoExclusion,
    IdUsuarioExclusion,
    FechaExclusion,
    IdUsuarioReactivacion,
    FechaReactivacion
)
SELECT
    viaje.IdViaje,
    @Fecha,
    @CategoriaGasto,
    @MetodoPago,
    @IdPersonaPagadora,
    @Monto,
    @Descripcion,
    @EstadoViatico,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL
FROM dbo.Viaje AS viaje
WHERE
    viaje.IdViaje = @IdViaje
    AND viaje.EstadoViaje = 1
    AND @Fecha BETWEEN
        viaje.FechaInicio
        AND viaje.FechaFin;

SELECT
    CASE
        WHEN @@ROWCOUNT = 1
            THEN CAST(SCOPE_IDENTITY() AS INT)
        ELSE NULL
    END;";

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
                            int idViatico;

                            using (
                                SqlCommand command =
                                    new SqlCommand(
                                        sqlViatico,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosViatico(
                                    command,
                                    viatico);

                                object resultado =
                                    command.ExecuteScalar();

                                if (resultado == null ||
                                    resultado == DBNull.Value)
                                {
                                    throw new PersistenciaException(
                                        "No fue posible registrar el viático porque el viaje no existe, no está Abierto o la fecha está fuera de su período.");
                                }

                                idViatico =
                                    Convert.ToInt32(
                                        resultado);
                            }

                            if (viatico.Comprobante != null)
                            {
                                InsertarComprobante(
                                    connection,
                                    transaction,
                                    idViatico,
                                    viatico.Comprobante);
                            }

                            transaction.Commit();

                            return idViatico;
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
                    "No fue posible registrar el viático.",
                    exception);
            }
        }

        public void Actualizar(
            Viatico viatico)
        {
            ValidarViaticoParaActualizar(
                viatico);

            const string sqlViatico = @"
UPDATE viatico
SET
    viatico.Fecha = @Fecha,
    viatico.CategoriaGasto =
        @CategoriaGasto,
    viatico.MetodoPago =
        @MetodoPago,
    viatico.IdPersonaPagadora =
        @IdPersonaPagadora,
    viatico.Monto = @Monto,
    viatico.Descripcion =
        @Descripcion
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
WHERE
    viatico.IdViatico =
        @IdViatico
    AND viatico.IdViaje =
        @IdViaje
    AND viatico.EstadoViatico = 1
    AND viaje.EstadoViaje = 1
    AND @Fecha BETWEEN
        viaje.FechaInicio
        AND viaje.FechaFin;";

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
                                        sqlViatico,
                                        connection,
                                        transaction))
                            {
                                AgregarParametrosViatico(
                                    command,
                                    viatico);

                                command.Parameters.Add(
                                    "@IdViatico",
                                    SqlDbType.Int).Value =
                                        viatico.IdViatico;

                                int filas =
                                    command.ExecuteNonQuery();

                                if (filas != 1)
                                {
                                    throw new PersistenciaException(
                                        "No fue posible modificar el viático porque no existe, no pertenece al viaje, está Excluido, el viaje no está Abierto o la fecha está fuera del período.");
                                }
                            }

                            PersistirComprobanteActualizado(
                                connection,
                                transaction,
                                viatico);

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
                    "No fue posible modificar el viático.",
                    exception);
            }
        }

        private static void PersistirComprobanteActualizado(
            SqlConnection connection,
            SqlTransaction transaction,
            Viatico viatico)
        {
            if (viatico.Comprobante == null)
            {
                EliminarComprobante(
                    connection,
                    transaction,
                    viatico.IdViatico);

                return;
            }

            int idComprobanteExistente =
                ObtenerIdComprobante(
                    connection,
                    transaction,
                    viatico.IdViatico);

            if (idComprobanteExistente <= 0)
            {
                InsertarComprobante(
                    connection,
                    transaction,
                    viatico.IdViatico,
                    viatico.Comprobante);

                return;
            }

            if (viatico.Comprobante.IdComprobante > 0 &&
                viatico.Comprobante.IdComprobante !=
                    idComprobanteExistente)
            {
                throw new PersistenciaException(
                    "El comprobante indicado no pertenece al viático.");
            }

            ActualizarComprobante(
                connection,
                transaction,
                idComprobanteExistente,
                viatico.IdViatico,
                viatico.Comprobante);
        }

        private static int ObtenerIdComprobante(
            SqlConnection connection,
            SqlTransaction transaction,
            int idViatico)
        {
            const string sql = @"
SELECT IdComprobante
FROM dbo.Comprobante
WHERE IdViatico = @IdViatico;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdViatico",
                    SqlDbType.Int).Value =
                        idViatico;

                object resultado =
                    command.ExecuteScalar();

                return resultado == null ||
                       resultado == DBNull.Value
                    ? 0
                    : Convert.ToInt32(
                        resultado);
            }
        }

        private static void InsertarComprobante(
            SqlConnection connection,
            SqlTransaction transaction,
            int idViatico,
            Comprobante comprobante)
        {
            const string sql = @"
INSERT INTO dbo.Comprobante
(
    IdViatico,
    TipoComprobante,
    CuitProveedor,
    RazonSocialProveedor,
    SituacionFiscal,
    Sucursal,
    Numero,
    MontoGravado,
    MontoImpuestos
)
VALUES
(
    @IdViatico,
    @TipoComprobante,
    @CuitProveedor,
    @RazonSocialProveedor,
    @SituacionFiscal,
    @Sucursal,
    @Numero,
    @MontoGravado,
    @MontoImpuestos
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                AgregarParametrosComprobante(
                    command,
                    idViatico,
                    comprobante);

                command.ExecuteNonQuery();
            }
        }

        private static void ActualizarComprobante(
            SqlConnection connection,
            SqlTransaction transaction,
            int idComprobante,
            int idViatico,
            Comprobante comprobante)
        {
            const string sql = @"
UPDATE dbo.Comprobante
SET
    TipoComprobante =
        @TipoComprobante,
    CuitProveedor =
        @CuitProveedor,
    RazonSocialProveedor =
        @RazonSocialProveedor,
    SituacionFiscal =
        @SituacionFiscal,
    Sucursal =
        @Sucursal,
    Numero =
        @Numero,
    MontoGravado =
        @MontoGravado,
    MontoImpuestos =
        @MontoImpuestos
WHERE
    IdComprobante =
        @IdComprobante
    AND IdViatico =
        @IdViatico;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                AgregarParametrosComprobante(
                    command,
                    idViatico,
                    comprobante);

                command.Parameters.Add(
                    "@IdComprobante",
                    SqlDbType.Int).Value =
                        idComprobante;

                int filas =
                    command.ExecuteNonQuery();

                if (filas != 1)
                {
                    throw new PersistenciaException(
                        "No fue posible actualizar el comprobante asociado al viático.");
                }
            }
        }

        private static void EliminarComprobante(
            SqlConnection connection,
            SqlTransaction transaction,
            int idViatico)
        {
            const string sql = @"
DELETE FROM dbo.Comprobante
WHERE IdViatico = @IdViatico;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdViatico",
                    SqlDbType.Int).Value =
                        idViatico;

                command.ExecuteNonQuery();
            }
        }

        private static Viatico ReconstruirViatico(
            SqlDataReader reader)
        {
            Persona pagador =
                ReconstruirPagador(
                    reader);

            Comprobante comprobante =
                ReconstruirComprobante(
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
                LeerTextoObligatorioOPermitidoVacio(
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

        private static Persona ReconstruirPagador(
            SqlDataReader reader)
        {
            int ordinal =
                reader.GetOrdinal(
                    "PagadorIdPersona");

            if (reader.IsDBNull(
                ordinal))
            {
                return null;
            }

            var persona =
                new Persona(
                    reader.GetInt32(
                        ordinal),
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

        private static Comprobante ReconstruirComprobante(
            SqlDataReader reader)
        {
            int ordinal =
                reader.GetOrdinal(
                    "IdComprobante");

            if (reader.IsDBNull(
                ordinal))
            {
                return null;
            }

            return new Comprobante(
                reader.GetInt32(
                    ordinal),
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

        private static ViaticoListadoDto CrearDto(
            SqlDataReader reader)
        {
            return new ViaticoListadoDto(
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
                LeerTextoObligatorioOPermitidoVacio(
                    reader,
                    "PagadoPor"),
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "Monto")),
                LeerTextoObligatorioOPermitidoVacio(
                    reader,
                    "Descripcion"),
                (EstadoViatico)
                    reader.GetByte(
                        reader.GetOrdinal(
                            "EstadoViatico")),
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "TieneComprobante")));
        }

        private static void AgregarParametrosViatico(
            SqlCommand command,
            Viatico viatico)
        {
            command.Parameters.Add(
                "@IdViaje",
                SqlDbType.Int).Value =
                    viatico.IdViaje;

            command.Parameters.Add(
                "@Fecha",
                SqlDbType.Date).Value =
                    viatico.Fecha.Date;

            command.Parameters.Add(
                "@CategoriaGasto",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        viatico.Categoria);

            command.Parameters.Add(
                "@MetodoPago",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        viatico.MetodoPago);

            command.Parameters.Add(
                "@IdPersonaPagadora",
                SqlDbType.Int).Value =
                    viatico.PagadoPor == null
                        ? (object)DBNull.Value
                        : viatico.PagadoPor.IdPersona;

            SqlParameter monto =
                command.Parameters.Add(
                    "@Monto",
                    SqlDbType.Decimal);

            monto.Precision = 18;
            monto.Scale = 2;
            monto.Value =
                viatico.Monto;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                1000).Value =
                    viatico.Descripcion;

            command.Parameters.Add(
                "@EstadoViatico",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        viatico.Estado);
        }

        private static void AgregarParametrosComprobante(
            SqlCommand command,
            int idViatico,
            Comprobante comprobante)
        {
            command.Parameters.Add(
                "@IdViatico",
                SqlDbType.Int).Value =
                    idViatico;

            command.Parameters.Add(
                "@TipoComprobante",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        comprobante.Tipo);

            command.Parameters.Add(
                "@CuitProveedor",
                SqlDbType.NVarChar,
                20).Value =
                    comprobante.CuitProveedor;

            command.Parameters.Add(
                "@RazonSocialProveedor",
                SqlDbType.NVarChar,
                200).Value =
                    comprobante.RazonSocialProveedor;

            command.Parameters.Add(
                "@SituacionFiscal",
                SqlDbType.TinyInt).Value =
                    Convert.ToByte(
                        comprobante.SituacionFiscal);

            command.Parameters.Add(
                "@Sucursal",
                SqlDbType.Char,
                4).Value =
                    comprobante.Sucursal;

            command.Parameters.Add(
                "@Numero",
                SqlDbType.Char,
                8).Value =
                    comprobante.Numero;

            SqlParameter montoGravado =
                command.Parameters.Add(
                    "@MontoGravado",
                    SqlDbType.Decimal);

            montoGravado.Precision = 18;
            montoGravado.Scale = 2;
            montoGravado.Value =
                comprobante.MontoGravado;

            SqlParameter montoImpuestos =
                command.Parameters.Add(
                    "@MontoImpuestos",
                    SqlDbType.Decimal);

            montoImpuestos.Precision = 18;
            montoImpuestos.Scale = 2;
            montoImpuestos.Value =
                comprobante.MontoImpuestos;
        }

        private static void ValidarViaticoParaInsertar(
            Viatico viatico)
        {
            ValidarViaticoPersistible(
                viatico);

            if (viatico.IdViatico != 0)
            {
                throw new ReglaNegocioException(
                    "Un nuevo viático no puede tener un identificador persistido.");
            }

            if (viatico.Estado !=
                EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "Un nuevo viático debe encontrarse Vigente.");
            }

            if (viatico.Comprobante != null &&
                viatico.Comprobante.IdComprobante != 0)
            {
                throw new ReglaNegocioException(
                    "Un nuevo comprobante no puede tener un identificador persistido.");
            }
        }

        private static void ValidarViaticoParaActualizar(
            Viatico viatico)
        {
            ValidarViaticoPersistible(
                viatico);

            if (viatico.IdViatico <= 0)
            {
                throw new ReglaNegocioException(
                    "El viático debe tener un identificador persistido.");
            }

            if (viatico.Estado !=
                EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "No se puede actualizar un viático Excluido.");
            }
        }

        private static void ValidarViaticoPersistible(
            Viatico viatico)
        {
            if (viatico == null)
            {
                throw new ArgumentNullException(
                    nameof(viatico));
            }

            if (viatico.IdViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "El viático debe estar asociado a un viaje persistido.");
            }

            if (viatico.PagadoPor != null &&
                viatico.PagadoPor.IdPersona <= 0)
            {
                throw new ReglaNegocioException(
                    "La persona pagadora debe estar persistida.");
            }
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

        private static string
            LeerTextoObligatorioOPermitidoVacio(
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

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string columna)
        {
            string valor =
                LeerTextoObligatorioOPermitidoVacio(
                    reader,
                    columna);

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
