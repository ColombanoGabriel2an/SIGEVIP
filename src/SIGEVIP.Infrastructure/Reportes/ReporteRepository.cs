using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SIGEVIP.Application.Reportes;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Reportes
{
    public sealed class ReporteRepository
        : IReporteRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public ReporteRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection
            <ReporteViajeSeleccionDto>
            ListarViajes()
        {
            const string sql = @"
SELECT
    viaje.IdViaje,
    viaje.Descripcion,
    viaje.FechaInicio,
    viaje.FechaFin,
    viaje.EstadoViaje
FROM dbo.Viaje AS viaje
ORDER BY
    viaje.FechaInicio DESC,
    viaje.IdViaje DESC;";

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
                        new List
                            <ReporteViajeSeleccionDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new ReporteViajeSeleccionDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdViaje")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Descripcion"),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "FechaInicio")),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "FechaFin")),
                                    (EstadoViaje)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "EstadoViaje"))));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los Viajes disponibles para Reportes.",
                    exception);
            }
        }

        public IReadOnlyCollection
            <ReportePersonaSeleccionDto>
            ListarPersonasPagadoras()
        {
            const string sql = @"
SELECT DISTINCT
    persona.IdPersona,
    LTRIM(RTRIM(
        persona.Apellido +
        N', ' +
        persona.Nombre
    )) AS NombreCompleto
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora
ORDER BY
    NombreCompleto,
    persona.IdPersona;";

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
                        new List
                            <ReportePersonaSeleccionDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new ReportePersonaSeleccionDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdPersona")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "NombreCompleto")));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar las Personas pagadoras.",
                    exception);
            }
        }

        public ReporteViajeDatosDto
            ObtenerDatosViaje(
                int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del Viaje debe ser mayor que cero.");
            }

            const string sql = @"
SELECT
    viaje.IdViaje,
    viaje.Descripcion,
    viaje.TipoViaje,
    viaje.FechaInicio,
    viaje.FechaFin,
    viaje.EstadoViaje,
    viaje.MontoAnticipado,
    viaje.FechaEnvioRendicion,
    viaje.FechaAprobacion,
    viaje.FechaCancelacion,
    viaje.MotivoCancelacion,

    CASE
        WHEN usuarioEnvio.IdUsuario IS NULL
            THEN N''
        ELSE
            LTRIM(RTRIM(
                personaEnvio.Apellido +
                N', ' +
                personaEnvio.Nombre
            )) +
            N' (' +
            usuarioEnvio.NombreUsuario +
            N')'
    END AS ResponsableEnvio,

    CASE
        WHEN usuarioAprobador.IdUsuario IS NULL
            THEN N''
        ELSE
            LTRIM(RTRIM(
                personaAprobador.Apellido +
                N', ' +
                personaAprobador.Nombre
            )) +
            N' (' +
            usuarioAprobador.NombreUsuario +
            N')'
    END AS Aprobador,

    CASE
        WHEN usuarioCancelacion.IdUsuario IS NULL
            THEN N''
        ELSE
            LTRIM(RTRIM(
                personaCancelacion.Apellido +
                N', ' +
                personaCancelacion.Nombre
            )) +
            N' (' +
            usuarioCancelacion.NombreUsuario +
            N')'
    END AS Cancelador
FROM dbo.Viaje AS viaje
LEFT JOIN dbo.Usuario AS usuarioEnvio
    ON usuarioEnvio.IdUsuario =
        viaje.IdUsuarioEnvioRendicion
LEFT JOIN dbo.Persona AS personaEnvio
    ON personaEnvio.IdPersona =
        usuarioEnvio.IdPersona
LEFT JOIN dbo.Usuario AS usuarioAprobador
    ON usuarioAprobador.IdUsuario =
        viaje.IdUsuarioAprobador
LEFT JOIN dbo.Persona AS personaAprobador
    ON personaAprobador.IdPersona =
        usuarioAprobador.IdPersona
LEFT JOIN dbo.Usuario AS usuarioCancelacion
    ON usuarioCancelacion.IdUsuario =
        viaje.IdUsuarioCancelacion
LEFT JOIN dbo.Persona AS personaCancelacion
    ON personaCancelacion.IdPersona =
        usuarioCancelacion.IdPersona
WHERE viaje.IdViaje = @IdViaje;

SELECT
    persona.IdPersona,
    LTRIM(RTRIM(
        persona.Apellido +
        N', ' +
        persona.Nombre
    )) AS NombreCompleto,
    persona.Email,
    persona.Activo
FROM dbo.ViajeParticipante
    AS relacionParticipante
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        relacionParticipante.IdPersona
WHERE relacionParticipante.IdViaje =
    @IdViaje
ORDER BY
    persona.Apellido,
    persona.Nombre,
    persona.IdPersona;

SELECT
    visita.IdVisita,
    visita.Fecha,
    visita.Observacion,
    visita.LocalidadEncuentro
FROM dbo.Visita AS visita
WHERE visita.IdViaje = @IdViaje
ORDER BY
    visita.Fecha,
    visita.IdVisita;

SELECT
    visita.IdVisita,
    cliente.IdCliente,
    cliente.RazonSocial,
    cliente.Cuit,
    cliente.Localidad,
    cliente.Provincia,
    cliente.Activo
FROM dbo.Visita AS visita
INNER JOIN dbo.VisitaCliente
    AS relacionCliente
    ON relacionCliente.IdVisita =
        visita.IdVisita
INNER JOIN dbo.Cliente AS cliente
    ON cliente.IdCliente =
        relacionCliente.IdCliente
WHERE visita.IdViaje = @IdViaje
ORDER BY
    visita.Fecha,
    visita.IdVisita,
    cliente.RazonSocial,
    cliente.IdCliente;

SELECT
    viatico.IdViatico,
    viatico.Fecha,
    viatico.CategoriaGasto,
    viatico.MetodoPago,
    CASE
        WHEN persona.IdPersona IS NULL
            THEN N'Sin persona pagadora'
        ELSE
            LTRIM(RTRIM(
                persona.Apellido +
                N', ' +
                persona.Nombre
            ))
    END AS PagadoPor,
    viatico.Monto,
    viatico.Descripcion,
    viatico.EstadoViatico
FROM dbo.Viatico AS viatico
LEFT JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora
WHERE viatico.IdViaje = @IdViaje
ORDER BY
    viatico.Fecha,
    viatico.IdViatico;";

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
                        "@IdViaje",
                        SqlDbType.Int).Value =
                            idViaje;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        int id =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "IdViaje"));

                        string descripcion =
                            LeerTextoObligatorio(
                                reader,
                                "Descripcion");

                        TipoViaje tipoViaje =
                            (TipoViaje)
                                reader.GetByte(
                                    reader.GetOrdinal(
                                        "TipoViaje"));

                        DateTime fechaInicio =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "FechaInicio"));

                        DateTime fechaFin =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "FechaFin"));

                        EstadoViaje estado =
                            (EstadoViaje)
                                reader.GetByte(
                                    reader.GetOrdinal(
                                        "EstadoViaje"));

                        decimal montoAnticipado =
                            reader.GetDecimal(
                                reader.GetOrdinal(
                                    "MontoAnticipado"));

                        string responsableEnvio =
                            LeerTextoOpcional(
                                reader,
                                "ResponsableEnvio");

                        DateTime? fechaEnvio =
                            LeerFechaOpcional(
                                reader,
                                "FechaEnvioRendicion");

                        string aprobador =
                            LeerTextoOpcional(
                                reader,
                                "Aprobador");

                        DateTime? fechaAprobacion =
                            LeerFechaOpcional(
                                reader,
                                "FechaAprobacion");

                        string cancelador =
                            LeerTextoOpcional(
                                reader,
                                "Cancelador");

                        DateTime? fechaCancelacion =
                            LeerFechaOpcional(
                                reader,
                                "FechaCancelacion");

                        string motivoCancelacion =
                            LeerTextoOpcional(
                                reader,
                                "MotivoCancelacion");

                        ExigirSiguienteResultado(
                            reader,
                            "participantes del Viaje");

                        var participantes =
                            new List
                                <ReporteViajeParticipanteDto>();

                        while (reader.Read())
                        {
                            participantes.Add(
                                new ReporteViajeParticipanteDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdPersona")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "NombreCompleto"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Email"),
                                    reader.GetBoolean(
                                        reader.GetOrdinal(
                                            "Activo"))));
                        }

                        ExigirSiguienteResultado(
                            reader,
                            "Visitas del Viaje");

                        var visitas =
                            new List
                                <ReporteViajeVisitaDto>();

                        while (reader.Read())
                        {
                            visitas.Add(
                                new ReporteViajeVisitaDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdVisita")),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "Fecha")),
                                    LeerTextoOpcional(
                                        reader,
                                        "Observacion"),
                                    LeerTextoOpcional(
                                        reader,
                                        "LocalidadEncuentro")));
                        }

                        ExigirSiguienteResultado(
                            reader,
                            "Clientes visitados");

                        var clientes =
                            new List
                                <ReporteViajeClienteDto>();

                        while (reader.Read())
                        {
                            clientes.Add(
                                new ReporteViajeClienteDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdVisita")),
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
                                        "Localidad"),
                                    LeerTextoOpcional(
                                        reader,
                                        "Provincia"),
                                    reader.GetBoolean(
                                        reader.GetOrdinal(
                                            "Activo"))));
                        }

                        ExigirSiguienteResultado(
                            reader,
                            "Viáticos del Viaje");

                        var viaticos =
                            new List
                                <ReporteViajeViaticoDto>();

                        while (reader.Read())
                        {
                            viaticos.Add(
                                new ReporteViajeViaticoDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdViatico")),
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
                                    LeerTextoObligatorio(
                                        reader,
                                        "PagadoPor"),
                                    reader.GetDecimal(
                                        reader.GetOrdinal(
                                            "Monto")),
                                    LeerTextoOpcional(
                                        reader,
                                        "Descripcion"),
                                    (EstadoViatico)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "EstadoViatico"))));
                        }

                        return new ReporteViajeDatosDto(
                            id,
                            descripcion,
                            tipoViaje,
                            fechaInicio,
                            fechaFin,
                            estado,
                            montoAnticipado,
                            responsableEnvio,
                            fechaEnvio,
                            aprobador,
                            fechaAprobacion,
                            cancelador,
                            fechaCancelacion,
                            motivoCancelacion,
                            participantes,
                            visitas,
                            clientes,
                            viaticos);
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
                    "No fue posible obtener el resumen del Viaje.",
                    exception);
            }
        }

        public IReadOnlyCollection
            <ReporteViaticoFilaDto>
            ListarViaticos(
                ReporteViaticoFiltro filtro)
        {
            ReporteViaticoFiltro
                filtroEfectivo =
                    filtro
                    ?? ReporteViaticoFiltro
                        .CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    viatico.IdViatico,
    viaje.IdViaje,
    viaje.Descripcion AS Viaje,
    viaje.EstadoViaje,
    viatico.Fecha,
    viatico.IdPersonaPagadora,
    CASE
        WHEN persona.IdPersona IS NULL
            THEN N'Sin persona pagadora'
        ELSE
            LTRIM(RTRIM(
                persona.Apellido +
                N', ' +
                persona.Nombre
            ))
    END AS PersonaPagadora,
    viatico.CategoriaGasto,
    viatico.MetodoPago,
    viatico.Monto,
    viatico.EstadoViatico,
    viatico.Descripcion
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
LEFT JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora
WHERE 1 = 1");

            var parametros =
                new List<SqlParameter>();

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
                    });
            }

            if (filtroEfectivo.IdViaje.HasValue)
            {
                sql.Append(@"
    AND viaje.IdViaje = @IdViaje");

                parametros.Add(
                    new SqlParameter(
                        "@IdViaje",
                        SqlDbType.Int)
                    {
                        Value =
                            filtroEfectivo
                                .IdViaje
                                .Value
                    });
            }

            if (filtroEfectivo.IdPersonaPagadora.HasValue)
            {
                sql.Append(@"
    AND viatico.IdPersonaPagadora =
        @IdPersonaPagadora");

                parametros.Add(
                    new SqlParameter(
                        "@IdPersonaPagadora",
                        SqlDbType.Int)
                    {
                        Value =
                            filtroEfectivo
                                .IdPersonaPagadora
                                .Value
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

            if (filtroEfectivo.MetodoPago.HasValue)
            {
                sql.Append(@"
    AND viatico.MetodoPago =
        @MetodoPago");

                parametros.Add(
                    new SqlParameter(
                        "@MetodoPago",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                filtroEfectivo
                                    .MetodoPago
                                    .Value)
                    });
            }

            if (filtroEfectivo.EstadoViatico.HasValue)
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
                                    .EstadoViatico
                                    .Value)
                    });
            }

            if (filtroEfectivo.EstadoViaje.HasValue)
            {
                sql.Append(@"
    AND viaje.EstadoViaje =
        @EstadoViaje");

                parametros.Add(
                    new SqlParameter(
                        "@EstadoViaje",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                filtroEfectivo
                                    .EstadoViaje
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
                        new List
                            <ReporteViaticoFilaDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new ReporteViaticoFilaDto(
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdViatico")),
                                    reader.GetInt32(
                                        reader.GetOrdinal(
                                            "IdViaje")),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Viaje"),
                                    (EstadoViaje)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "EstadoViaje")),
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "Fecha")),
                                    LeerEnteroOpcional(
                                        reader,
                                        "IdPersonaPagadora"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "PersonaPagadora"),
                                    (CategoriaGasto)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "CategoriaGasto")),
                                    (MetodoPago)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "MetodoPago")),
                                    reader.GetDecimal(
                                        reader.GetOrdinal(
                                            "Monto")),
                                    (EstadoViatico)
                                        reader.GetByte(
                                            reader.GetOrdinal(
                                                "EstadoViatico")),
                                    LeerTextoOpcional(
                                        reader,
                                        "Descripcion")));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el reporte de Viáticos.",
                    exception);
            }
        }


        public IReadOnlyCollection
            <ReporteAnalisisItemDto>
            ConsultarAnalisis(
                ReporteAnalisisFiltro filtro)
        {
            if (filtro == null)
            {
                throw new ArgumentNullException(
                    nameof(filtro));
            }

            string expresionEtiqueta;
            string expresionValor;
            string origen;
            string columnaFecha;
            string condicionAdicional =
                string.Empty;

            switch (filtro.Area)
            {
                case ReporteAreaAnalisis.Gastos:
                    expresionValor =
                        "CAST(SUM(viatico.Monto) AS DECIMAL(18, 2))";

                    origen = @"
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
LEFT JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora";

                    columnaFecha =
                        "viatico.Fecha";

                    condicionAdicional = @"
    AND viatico.EstadoViatico =
        @EstadoViaticoVigente";

                    switch (filtro.Agrupacion)
                    {
                        case ReporteAgrupacionAnalisis.Categoria:
                            expresionEtiqueta = @"
CASE viatico.CategoriaGasto
    WHEN 1 THEN N'Combustible'
    WHEN 2 THEN N'Alojamiento'
    WHEN 3 THEN N'Alimentación'
    WHEN 4 THEN N'Peaje'
    WHEN 5 THEN N'Estacionamiento'
    WHEN 6 THEN N'Transporte'
    WHEN 7 THEN N'Otros'
    ELSE N'Sin categoría'
END";
                            break;

                        case ReporteAgrupacionAnalisis.Empleado:
                            expresionEtiqueta = @"
LTRIM(RTRIM(
    persona.Apellido +
    N', ' +
    persona.Nombre
))";
                            condicionAdicional += @"
    AND viatico.IdPersonaPagadora
        IS NOT NULL";
                            break;

                        case ReporteAgrupacionAnalisis.Viaje:
                            expresionEtiqueta = @"
N'#' +
CONVERT(
    NVARCHAR(20),
    viaje.IdViaje
) +
N' - ' +
viaje.Descripcion";
                            break;

                        case ReporteAgrupacionAnalisis.Mes:
                            expresionEtiqueta =
                                "CONVERT(CHAR(7), viatico.Fecha, 120)";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(filtro),
                                "La agrupación no es válida para Gastos.");
                    }
                    break;

                case ReporteAreaAnalisis.Clientes:
                    expresionValor = @"
CAST(
    COUNT(
        DISTINCT visita.IdVisita
    )
    AS DECIMAL(18, 2)
)";

                    origen = @"
FROM dbo.Visita AS visita
INNER JOIN dbo.VisitaCliente
    AS relacionCliente
    ON relacionCliente.IdVisita =
        visita.IdVisita
INNER JOIN dbo.Cliente AS cliente
    ON cliente.IdCliente =
        relacionCliente.IdCliente";

                    columnaFecha =
                        "visita.Fecha";

                    switch (filtro.Agrupacion)
                    {
                        case ReporteAgrupacionAnalisis.Cliente:
                            expresionEtiqueta =
                                "cliente.RazonSocial";
                            break;

                        case ReporteAgrupacionAnalisis.Localidad:
                            expresionEtiqueta = @"
COALESCE(
    NULLIF(
        LTRIM(RTRIM(
            cliente.Localidad
        )),
        N''
    ),
    N'Sin localidad'
)";
                            break;

                        case ReporteAgrupacionAnalisis.Provincia:
                            expresionEtiqueta = @"
COALESCE(
    NULLIF(
        LTRIM(RTRIM(
            cliente.Provincia
        )),
        N''
    ),
    N'Sin provincia'
)";
                            break;

                        case ReporteAgrupacionAnalisis.Mes:
                            expresionEtiqueta =
                                "CONVERT(CHAR(7), visita.Fecha, 120)";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(filtro),
                                "La agrupación no es válida para Clientes.");
                    }
                    break;

                case ReporteAreaAnalisis.Viajes:
                    expresionValor = @"
CAST(
    COUNT(
        DISTINCT viaje.IdViaje
    )
    AS DECIMAL(18, 2)
)";

                    columnaFecha =
                        "viaje.FechaInicio";

                    switch (filtro.Agrupacion)
                    {
                        case ReporteAgrupacionAnalisis.Empleado:
                            expresionEtiqueta = @"
LTRIM(RTRIM(
    persona.Apellido +
    N', ' +
    persona.Nombre
))";

                            origen = @"
FROM dbo.Viaje AS viaje
INNER JOIN dbo.ViajeParticipante
    AS relacionParticipante
    ON relacionParticipante.IdViaje =
        viaje.IdViaje
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        relacionParticipante.IdPersona";
                            break;

                        case ReporteAgrupacionAnalisis.TipoViaje:
                            expresionEtiqueta = @"
CASE viaje.TipoViaje
    WHEN 1 THEN N'Desplazamiento'
    WHEN 2 THEN N'En oficina'
    WHEN 3 THEN N'Evento o feria'
    ELSE N'Sin tipo'
END";

                            origen =
                                "FROM dbo.Viaje AS viaje";
                            break;

                        case ReporteAgrupacionAnalisis.EstadoViaje:
                            expresionEtiqueta = @"
CASE viaje.EstadoViaje
    WHEN 1 THEN N'Abierto'
    WHEN 2 THEN N'En rendición'
    WHEN 3 THEN N'Aprobado'
    WHEN 4 THEN N'Cancelado'
    ELSE N'Sin estado'
END";

                            origen =
                                "FROM dbo.Viaje AS viaje";
                            break;

                        case ReporteAgrupacionAnalisis.Mes:
                            expresionEtiqueta =
                                "CONVERT(CHAR(7), viaje.FechaInicio, 120)";

                            origen =
                                "FROM dbo.Viaje AS viaje";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(filtro),
                                "La agrupación no es válida para Viajes.");
                    }
                    break;

                case ReporteAreaAnalisis.Empleados:
                    expresionEtiqueta = @"
LTRIM(RTRIM(
    persona.Apellido +
    N', ' +
    persona.Nombre
))";

                    switch (filtro.Indicador)
                    {
                        case ReporteIndicadorAnalisis.Importe:
                            expresionValor =
                                "CAST(SUM(viatico.Monto) AS DECIMAL(18, 2))";

                            origen = @"
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        viatico.IdPersonaPagadora";

                            columnaFecha =
                                "viatico.Fecha";

                            condicionAdicional = @"
    AND viatico.EstadoViatico =
        @EstadoViaticoVigente";
                            break;

                        case ReporteIndicadorAnalisis.CantidadVisitas:
                            expresionValor = @"
CAST(
    COUNT(
        DISTINCT visita.IdVisita
    )
    AS DECIMAL(18, 2)
)";

                            origen = @"
FROM dbo.ViajeParticipante
    AS relacionParticipante
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        relacionParticipante.IdPersona
INNER JOIN dbo.Visita AS visita
    ON visita.IdViaje =
        relacionParticipante.IdViaje";

                            columnaFecha =
                                "visita.Fecha";
                            break;

                        case ReporteIndicadorAnalisis.CantidadViajes:
                            expresionValor = @"
CAST(
    COUNT(
        DISTINCT viaje.IdViaje
    )
    AS DECIMAL(18, 2)
)";

                            origen = @"
FROM dbo.ViajeParticipante
    AS relacionParticipante
INNER JOIN dbo.Persona AS persona
    ON persona.IdPersona =
        relacionParticipante.IdPersona
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        relacionParticipante.IdViaje";

                            columnaFecha =
                                "viaje.FechaInicio";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(filtro),
                                "El indicador no es válido para Empleados.");
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(filtro),
                        "El área de análisis no es válida.");
            }

            var sql =
                new StringBuilder();

            sql.Append(@"
SELECT
");

            sql.Append(
                expresionEtiqueta);

            sql.Append(@"
    AS Etiqueta,
");

            sql.Append(
                expresionValor);

            sql.Append(@"
    AS Valor
");

            sql.Append(
                origen);

            sql.Append(@"
WHERE 1 = 1");

            var parametros =
                new List<SqlParameter>();

            if (
                filtro.Area ==
                    ReporteAreaAnalisis.Gastos
                ||
                (
                    filtro.Area ==
                        ReporteAreaAnalisis.Empleados
                    &&
                    filtro.Indicador ==
                        ReporteIndicadorAnalisis.Importe
                ))
            {
                parametros.Add(
                    new SqlParameter(
                        "@EstadoViaticoVigente",
                        SqlDbType.TinyInt)
                    {
                        Value =
                            Convert.ToByte(
                                EstadoViatico.Vigente)
                    });
            }

            sql.Append(
                condicionAdicional);

            if (filtro.FechaDesde.HasValue)
            {
                sql.Append(@"
    AND ");

                sql.Append(
                    columnaFecha);

                sql.Append(@"
        >= @FechaDesde");

                parametros.Add(
                    new SqlParameter(
                        "@FechaDesde",
                        SqlDbType.Date)
                    {
                        Value =
                            filtro.FechaDesde.Value
                    });
            }

            if (filtro.FechaHasta.HasValue)
            {
                sql.Append(@"
    AND ");

                sql.Append(
                    columnaFecha);

                sql.Append(@"
        <= @FechaHasta");

                parametros.Add(
                    new SqlParameter(
                        "@FechaHasta",
                        SqlDbType.Date)
                    {
                        Value =
                            filtro.FechaHasta.Value
                    });
            }

            sql.Append(@"
GROUP BY
");

            sql.Append(
                expresionEtiqueta);

            sql.Append(@"
HAVING
");

            sql.Append(
                expresionValor);

            sql.Append(@"
    > 0
ORDER BY
    Valor DESC,
    Etiqueta;");

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
                        new List
                            <ReporteAnalisisItemDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new ReporteAnalisisItemDto(
                                    LeerTextoObligatorio(
                                        reader,
                                        "Etiqueta"),
                                    reader.GetDecimal(
                                        reader.GetOrdinal(
                                            "Valor"))));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible generar el análisis solicitado.",
                    exception);
            }
        }

        private static void ExigirSiguienteResultado(
            SqlDataReader reader,
            string descripcion)
        {
            if (!reader.NextResult())
            {
                throw new PersistenciaException(
                    "La consulta de Reportes no devolvió el conjunto esperado de " +
                    descripcion +
                    ".");
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
