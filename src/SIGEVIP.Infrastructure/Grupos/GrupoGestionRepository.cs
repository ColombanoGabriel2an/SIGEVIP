using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SIGEVIP.Application.Grupos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Grupos
{
    public sealed class GrupoGestionRepository
        : IGrupoGestionRepository
    {
        private const int ErrorIndiceDuplicado = 2601;
        private const int ErrorRestriccionUnica = 2627;

        private readonly SqlConnectionFactory
            _connectionFactory;

        public GrupoGestionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<GrupoListadoDto> Listar(
            GrupoFiltro filtro)
        {
            GrupoFiltro filtroEfectivo =
                filtro
                ?? GrupoFiltro.CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    (
        SELECT COUNT(*)
        FROM dbo.GrupoPermiso AS gp
        WHERE gp.IdGrupo = g.IdGrupo
    ) AS CantidadPermisos,
    (
        SELECT COUNT(*)
        FROM dbo.UsuarioGrupo AS ug
        WHERE ug.IdGrupo = g.IdGrupo
    ) AS CantidadUsuarios,
    g.Activo
FROM dbo.Grupo AS g
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
    AND g.Activo = @Activo");

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
    g.Nombre,
    g.IdGrupo;");

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

                    List<GrupoListadoDto> resultados =
                        new List<GrupoListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                CrearGrupoListadoDto(
                                    reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los grupos.",
                    exception);
            }
        }

        public GrupoDetalleDto ObtenerDetallePorId(
            int idGrupo)
        {
            ValidarIdGrupo(
                idGrupo);

            const string sql = @"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
WHERE g.IdGrupo = @IdGrupo;

SELECT
    gp.IdPermiso
FROM dbo.GrupoPermiso AS gp
WHERE gp.IdGrupo = @IdGrupo
ORDER BY gp.IdPermiso;

SELECT
    gg.IdGrupoHijo
FROM dbo.GrupoGrupo AS gg
WHERE gg.IdGrupoPadre = @IdGrupo
ORDER BY gg.IdGrupoHijo;";

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
                        "@IdGrupo",
                        SqlDbType.Int).Value =
                            idGrupo;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        string codigo =
                            LeerTextoObligatorio(
                                reader,
                                "Codigo");

                        string nombre =
                            LeerTextoObligatorio(
                                reader,
                                "Nombre");

                        string descripcion =
                            LeerTextoOpcional(
                                reader,
                                "Descripcion");

                        bool activo =
                            reader.GetBoolean(
                                reader.GetOrdinal(
                                    "Activo"));

                        ExigirSiguienteResultado(
                            reader,
                            "permisos directos del grupo");

                        List<int> idsPermisos =
                            new List<int>();

                        while (reader.Read())
                        {
                            idsPermisos.Add(
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdPermiso")));
                        }

                        ExigirSiguienteResultado(
                            reader,
                            "grupos hijos directos");

                        List<int> idsGruposHijos =
                            new List<int>();

                        while (reader.Read())
                        {
                            idsGruposHijos.Add(
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdGrupoHijo")));
                        }

                        return new GrupoDetalleDto(
                            idGrupo,
                            codigo,
                            nombre,
                            descripcion,
                            activo,
                            idsPermisos,
                            idsGruposHijos);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el detalle del grupo.",
                    exception);
            }
        }

        public Grupo ObtenerPorId(
            int idGrupo)
        {
            ValidarIdGrupo(
                idGrupo);

            const string sql = @"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
WHERE g.IdGrupo = @IdGrupo;

SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    p.Activo
FROM dbo.GrupoPermiso AS gp
INNER JOIN dbo.Permiso AS p
    ON p.IdPermiso = gp.IdPermiso
WHERE gp.IdGrupo = @IdGrupo
ORDER BY
    p.Codigo,
    p.IdPermiso;

SELECT
    hijo.IdGrupo,
    hijo.Codigo,
    hijo.Nombre,
    hijo.Descripcion,
    hijo.Activo
FROM dbo.GrupoGrupo AS gg
INNER JOIN dbo.Grupo AS hijo
    ON hijo.IdGrupo = gg.IdGrupoHijo
WHERE gg.IdGrupoPadre = @IdGrupo
ORDER BY
    hijo.Codigo,
    hijo.IdGrupo;";

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
                        "@IdGrupo",
                        SqlDbType.Int).Value =
                            idGrupo;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        Grupo grupo =
                            ReconstruirGrupo(
                                reader);

                        ExigirSiguienteResultado(
                            reader,
                            "permisos directos del grupo");

                        while (reader.Read())
                        {
                            grupo.AgregarComponente(
                                ReconstruirPermiso(
                                    reader));
                        }

                        ExigirSiguienteResultado(
                            reader,
                            "grupos hijos directos");

                        while (reader.Read())
                        {
                            grupo.AgregarComponente(
                                ReconstruirGrupo(
                                    reader));
                        }

                        return grupo;
                    }
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos del grupo son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el grupo.",
                    exception);
            }
        }

        public IReadOnlyCollection<PermisoSeleccionGrupoDto>
            ListarPermisosActivos(
                IReadOnlyCollection<int> idsSeleccionados)
        {
            HashSet<int> seleccionados =
                new HashSet<int>(
                    idsSeleccionados
                    ?? new int[0]);

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion
FROM dbo.Permiso AS p
WHERE
    p.Activo = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            if (seleccionados.Count > 0)
            {
                sql.Append(@"
    OR p.IdPermiso IN
(");

                List<int> idsOrdenados =
                    seleccionados
                        .OrderBy(
                            idPermiso =>
                                idPermiso)
                        .ToList();

                parametros.AddRange(
                    CrearParametrosIds(
                        idsOrdenados,
                        "@IdPermisoSeleccionado",
                        sql));

                sql.Append(@"
)");
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

                    List<PermisoSeleccionGrupoDto> resultados =
                        new List<PermisoSeleccionGrupoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idPermiso =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdPermiso"));

                            resultados.Add(
                                new PermisoSeleccionGrupoDto(
                                    idPermiso,
                                    LeerTextoObligatorio(
                                        reader,
                                        "Codigo"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Nombre"),
                                    LeerTextoOpcional(
                                        reader,
                                        "Descripcion"),
                                    seleccionados.Contains(
                                        idPermiso)));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los permisos disponibles.",
                    exception);
            }
        }

        public IReadOnlyCollection<GrupoSeleccionGrupoDto>
            ListarGruposActivos(
                int? idGrupoPadre,
                IReadOnlyCollection<int> idsSeleccionados)
        {
            HashSet<int> seleccionados =
                new HashSet<int>(
                    idsSeleccionados
                    ?? new int[0]);

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
WHERE
    (
        g.Activo = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            if (seleccionados.Count > 0)
            {
                sql.Append(@"
        OR g.IdGrupo IN
(");

                List<int> idsOrdenados =
                    seleccionados
                        .OrderBy(
                            idGrupo =>
                                idGrupo)
                        .ToList();

                parametros.AddRange(
                    CrearParametrosIds(
                        idsOrdenados,
                        "@IdGrupoSeleccionado",
                        sql));

                sql.Append(@"
)");
            }

            sql.Append(@"
    )");

            if (idGrupoPadre.HasValue)
            {
                sql.Append(@"
    AND g.IdGrupo <> @IdGrupoPadre");

                parametros.Add(
                    new SqlParameter(
                        "@IdGrupoPadre",
                        SqlDbType.Int)
                    {
                        Value =
                            idGrupoPadre.Value
                    });
            }

            sql.Append(@"
ORDER BY
    g.Codigo,
    g.IdGrupo;");

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

                    List<GrupoSeleccionGrupoDto> resultados =
                        new List<GrupoSeleccionGrupoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGrupo =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdGrupo"));

                            resultados.Add(
                                new GrupoSeleccionGrupoDto(
                                    idGrupo,
                                    LeerTextoObligatorio(
                                        reader,
                                        "Codigo"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Nombre"),
                                    LeerTextoOpcional(
                                        reader,
                                        "Descripcion"),
                                    reader.GetBoolean(
                                        reader.GetOrdinal(
                                            "Activo")),
                                    seleccionados.Contains(
                                        idGrupo)));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los grupos hijos disponibles.",
                    exception);
            }
        }

        public IReadOnlyCollection<PermisoEfectivoGrupoDto>
            ObtenerPermisosEfectivosVistaPrevia(
                IReadOnlyCollection<int> idsPermisosDirectos,
                IReadOnlyCollection<int> idsGruposHijos)
        {
            List<int> permisosDirectos =
                ValidarIds(
                    idsPermisosDirectos,
                    nameof(idsPermisosDirectos));

            List<int> gruposHijos =
                ValidarIds(
                    idsGruposHijos,
                    nameof(idsGruposHijos));

            if (permisosDirectos.Count == 0 &&
                gruposHijos.Count == 0)
            {
                return new List<PermisoEfectivoGrupoDto>()
                    .AsReadOnly();
            }

            StringBuilder sql =
                new StringBuilder();

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            sql.Append(@"
;WITH GruposDescendientes AS
(
");

            if (gruposHijos.Count > 0)
            {
                sql.Append(@"
    SELECT
        g.IdGrupo AS IdGrupoActual,
        CAST(
            N'/' +
            CONVERT(NVARCHAR(20), g.IdGrupo) +
            N'/'
            AS NVARCHAR(MAX)
        ) AS Camino
    FROM dbo.Grupo AS g
    WHERE
        g.Activo = 1
        AND g.IdGrupo IN
(");

                parametros.AddRange(
                    CrearParametrosIds(
                        gruposHijos,
                        "@IdGrupoVistaPrevia",
                        sql));

                sql.Append(@"
)

    UNION ALL

    SELECT
        grupoHijo.IdGrupo,
        CAST(
            descendencia.Camino +
            CONVERT(NVARCHAR(20), grupoHijo.IdGrupo) +
            N'/'
            AS NVARCHAR(MAX)
        )
    FROM GruposDescendientes AS descendencia
    INNER JOIN dbo.GrupoGrupo AS gg
        ON gg.IdGrupoPadre =
            descendencia.IdGrupoActual
    INNER JOIN dbo.Grupo AS grupoHijo
        ON grupoHijo.IdGrupo =
            gg.IdGrupoHijo
        AND grupoHijo.Activo = 1
    WHERE CHARINDEX(
        N'/' +
        CONVERT(NVARCHAR(20), grupoHijo.IdGrupo) +
        N'/',
        descendencia.Camino
    ) = 0
");
            }
            else
            {
                sql.Append(@"
    SELECT
        CAST(NULL AS INT) AS IdGrupoActual,
        CAST(NULL AS NVARCHAR(MAX)) AS Camino
    WHERE 1 = 0
");
            }

            sql.Append(@"
),
PermisosCombinados AS
(
");

            bool requiereUnion =
                false;

            if (permisosDirectos.Count > 0)
            {
                sql.Append(@"
    SELECT
        p.IdPermiso,
        p.Codigo,
        p.Nombre,
        p.Descripcion,
        CAST(1 AS BIT) AS EsDirecto
    FROM dbo.Permiso AS p
    WHERE
        p.Activo = 1
        AND p.IdPermiso IN
(");

                parametros.AddRange(
                    CrearParametrosIds(
                        permisosDirectos,
                        "@IdPermisoVistaPrevia",
                        sql));

                sql.Append(@"
)");

                requiereUnion =
                    true;
            }

            if (gruposHijos.Count > 0)
            {
                if (requiereUnion)
                {
                    sql.Append(@"

    UNION ALL
");
                }

                sql.Append(@"
    SELECT
        p.IdPermiso,
        p.Codigo,
        p.Nombre,
        p.Descripcion,
        CAST(0 AS BIT) AS EsDirecto
    FROM GruposDescendientes AS descendencia
    INNER JOIN dbo.GrupoPermiso AS gp
        ON gp.IdGrupo =
            descendencia.IdGrupoActual
    INNER JOIN dbo.Permiso AS p
        ON p.IdPermiso =
            gp.IdPermiso
        AND p.Activo = 1
");
            }

            sql.Append(@"
)
SELECT
    IdPermiso,
    Codigo,
    Nombre,
    Descripcion,
    CAST(MAX(
        CASE
            WHEN EsDirecto = 1 THEN 1
            ELSE 0
        END
    ) AS BIT) AS EsDirecto
FROM PermisosCombinados
GROUP BY
    IdPermiso,
    Codigo,
    Nombre,
    Descripcion
ORDER BY
    Codigo,
    IdPermiso
OPTION (MAXRECURSION 32767);");

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

                    List<PermisoEfectivoGrupoDto> resultados =
                        new List<PermisoEfectivoGrupoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new PermisoEfectivoGrupoDto(
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
                                    reader.GetBoolean(
                                        reader.GetOrdinal(
                                            "EsDirecto"))));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible calcular los permisos efectivos del grupo.",
                    exception);
            }
        }

        public IReadOnlyCollection<Grupo>
            ObtenerGruposPorIds(
                IReadOnlyCollection<int> idsGrupos)
        {
            List<int> ids =
                ValidarIds(
                    idsGrupos,
                    nameof(idsGrupos));

            if (ids.Count == 0)
            {
                return new List<Grupo>()
                    .AsReadOnly();
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
WHERE g.IdGrupo IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    ids,
                    "@IdGrupo",
                    sql);

            sql.Append(@"
)
ORDER BY
    g.Codigo,
    g.IdGrupo;");

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

                    List<Grupo> grupos =
                        new List<Grupo>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            grupos.Add(
                                ReconstruirGrupo(
                                    reader));
                        }
                    }

                    return grupos.AsReadOnly();
                }
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de los grupos hijos son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar los grupos hijos.",
                    exception);
            }
        }

        public IReadOnlyCollection<Permiso>
            ObtenerPermisosPorIds(
                IReadOnlyCollection<int> idsPermisos)
        {
            List<int> ids =
                ValidarIds(
                    idsPermisos,
                    nameof(idsPermisos));

            if (ids.Count == 0)
            {
                return new List<Permiso>()
                    .AsReadOnly();
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    p.Activo
FROM dbo.Permiso AS p
WHERE p.IdPermiso IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    ids,
                    "@IdPermiso",
                    sql);

            sql.Append(@"
)
ORDER BY p.IdPermiso;");

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

                    List<Permiso> resultados =
                        new List<Permiso>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                ReconstruirPermiso(
                                    reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de los permisos son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar los permisos seleccionados.",
                    exception);
            }
        }

        public bool ExisteCodigo(
            string codigo,
            int? idGrupoExcluido)
        {
            string codigoNormalizado =
                NormalizarTexto(
                    codigo);

            return ExisteValor(
                "Codigo",
                codigoNormalizado,
                idGrupoExcluido,
                "No fue posible comprobar el código del grupo.");
        }

        public bool ExisteNombre(
            string nombre,
            int? idGrupoExcluido)
        {
            string nombreNormalizado =
                NormalizarTexto(
                    nombre);

            return ExisteValor(
                "Nombre",
                nombreNormalizado,
                idGrupoExcluido,
                "No fue posible comprobar el nombre del grupo.");
        }

        public bool ExistePermisoActivo(
            string codigoPermiso)
        {
            string codigoNormalizado =
                NormalizarTexto(
                    codigoPermiso);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Permiso AS p
            WHERE
                p.Codigo = @Codigo
                AND p.Activo = 1
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

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible comprobar el permiso activo.",
                    exception);
            }
        }

        public int Insertar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos)
        {
            if (grupo == null)
            {
                throw new ArgumentNullException(
                    nameof(grupo));
            }

            List<int> ids =
                ValidarIdsObligatorios(
                    idsPermisos,
                    nameof(idsPermisos));

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
                            ValidarUnicidadInterna(
                                connection,
                                transaction,
                                grupo.Codigo,
                                grupo.Nombre,
                                null);

                            ValidarPermisosActivosInterno(
                                connection,
                                transaction,
                                ids);

                            int idGrupo =
                                InsertarGrupoInterno(
                                    connection,
                                    transaction,
                                    grupo);

                            InsertarGrupoPermisosInterno(
                                connection,
                                transaction,
                                idGrupo,
                                ids);

                            transaction.Commit();

                            return idGrupo;
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
            catch (SqlException exception)
                when (EsErrorDuplicado(
                    exception))
            {
                throw CrearErrorDuplicado(
                    exception,
                    false);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar el grupo.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de registro del grupo no pudo completarse.",
                    exception);
            }
        }

        public int Insertar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos)
        {
            if (grupo == null)
            {
                throw new ArgumentNullException(
                    nameof(grupo));
            }

            List<int> permisos =
                ValidarIdsObligatorios(
                    idsPermisos,
                    nameof(idsPermisos));

            List<int> hijos =
                ValidarIds(
                    idsGruposHijos,
                    nameof(idsGruposHijos));

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
                            ValidarUnicidadInterna(
                                connection,
                                transaction,
                                grupo.Codigo,
                                grupo.Nombre,
                                null);

                            ValidarPermisosActivosInterno(
                                connection,
                                transaction,
                                permisos);

                            ValidarGruposHijosActivosInterno(
                                connection,
                                transaction,
                                hijos);

                            int idGrupo =
                                InsertarGrupoInterno(
                                    connection,
                                    transaction,
                                    grupo);

                            InsertarGrupoPermisosInterno(
                                connection,
                                transaction,
                                idGrupo,
                                permisos);

                            InsertarGrupoGruposInterno(
                                connection,
                                transaction,
                                idGrupo,
                                hijos);

                            transaction.Commit();

                            return idGrupo;
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
            catch (SqlException exception)
                when (EsErrorDuplicado(
                    exception))
            {
                throw CrearErrorDuplicado(
                    exception,
                    false);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar el grupo y su jerarquía.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de registro del grupo no pudo completarse.",
                    exception);
            }
        }

        public void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos)
        {
            if (grupo == null)
            {
                throw new ArgumentNullException(
                    nameof(grupo));
            }

            ValidarIdGrupo(
                grupo.IdGrupo);

            List<int> ids =
                ValidarIdsObligatorios(
                    idsPermisos,
                    nameof(idsPermisos));

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
                            ValidarUnicidadInterna(
                                connection,
                                transaction,
                                grupo.Codigo,
                                grupo.Nombre,
                                grupo.IdGrupo);

                            ValidarPermisosActivosInterno(
                                connection,
                                transaction,
                                ids);

                            ActualizarGrupoInterno(
                                connection,
                                transaction,
                                grupo);

                            EliminarGrupoPermisosInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo);

                            InsertarGrupoPermisosInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo,
                                ids);

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
            catch (SqlException exception)
                when (EsErrorDuplicado(
                    exception))
            {
                throw CrearErrorDuplicado(
                    exception,
                    true);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible actualizar el grupo.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de actualización del grupo no pudo completarse.",
                    exception);
            }
        }

        public void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos)
        {
            if (grupo == null)
            {
                throw new ArgumentNullException(
                    nameof(grupo));
            }

            ValidarIdGrupo(
                grupo.IdGrupo);

            List<int> permisos =
                ValidarIdsObligatorios(
                    idsPermisos,
                    nameof(idsPermisos));

            List<int> hijos =
                ValidarIds(
                    idsGruposHijos,
                    nameof(idsGruposHijos));

            if (hijos.Contains(
                grupo.IdGrupo))
            {
                throw new ReglaNegocioException(
                    "Un grupo no puede agregarse a sí mismo.");
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
                            ValidarUnicidadInterna(
                                connection,
                                transaction,
                                grupo.Codigo,
                                grupo.Nombre,
                                grupo.IdGrupo);

                            ValidarPermisosActivosInterno(
                                connection,
                                transaction,
                                permisos);

                            ValidarGruposHijosDisponiblesInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo,
                                hijos);

                            ValidarAusenciaDeCicloInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo,
                                hijos);

                            ActualizarGrupoInterno(
                                connection,
                                transaction,
                                grupo);

                            EliminarGrupoPermisosInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo);

                            InsertarGrupoPermisosInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo,
                                permisos);

                            EliminarGrupoGruposInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo);

                            InsertarGrupoGruposInterno(
                                connection,
                                transaction,
                                grupo.IdGrupo,
                                hijos);

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
            catch (SqlException exception)
                when (EsErrorDuplicado(
                    exception))
            {
                throw CrearErrorDuplicado(
                    exception,
                    true);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible actualizar el grupo y su jerarquía.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de actualización del grupo no pudo completarse.",
                    exception);
            }
        }

        public void Activar(
            int idGrupo)
        {
            ActualizarEstado(
                idGrupo,
                true);
        }

        public void Desactivar(
            int idGrupo)
        {
            ActualizarEstado(
                idGrupo,
                false);
        }

        private void ActualizarEstado(
            int idGrupo,
            bool activo)
        {
            ValidarIdGrupo(
                idGrupo);

            const string sql = @"
UPDATE dbo.Grupo
SET Activo = @Activo
WHERE IdGrupo = @IdGrupo;";

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
                        "@Activo",
                        SqlDbType.Bit).Value =
                            activo;

                    command.Parameters.Add(
                        "@IdGrupo",
                        SqlDbType.Int).Value =
                            idGrupo;

                    connection.Open();

                    int filas =
                        command.ExecuteNonQuery();

                    ExigirUnaFilaGrupo(
                        filas,
                        activo
                            ? "activar"
                            : "desactivar");
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible actualizar el estado del grupo.",
                    exception);
            }
        }

        private bool ExisteValor(
            string columna,
            string valor,
            int? idGrupoExcluido,
            string mensajeError)
        {
            string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Grupo AS g
            WHERE
                g." + columna + @" = @Valor
                AND
                (
                    @IdGrupoExcluido IS NULL
                    OR g.IdGrupo <> @IdGrupoExcluido
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
                        "@Valor",
                        SqlDbType.NVarChar,
                        columna == "Codigo"
                            ? 100
                            : 150).Value =
                            valor;

                    SqlParameter idExcluido =
                        command.Parameters.Add(
                            "@IdGrupoExcluido",
                            SqlDbType.Int);

                    idExcluido.Value =
                        idGrupoExcluido.HasValue
                            ? (object)idGrupoExcluido.Value
                            : DBNull.Value;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    mensajeError,
                    exception);
            }
        }

        private static void ValidarUnicidadInterna(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string nombre,
            int? idGrupoExcluido)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Grupo AS g
            WHERE
                g.Codigo = @Codigo
                AND
                (
                    @IdGrupoExcluido IS NULL
                    OR g.IdGrupo <> @IdGrupoExcluido
                )
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS CodigoDuplicado,
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Grupo AS g
            WHERE
                g.Nombre = @Nombre
                AND
                (
                    @IdGrupoExcluido IS NULL
                    OR g.IdGrupo <> @IdGrupoExcluido
                )
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS NombreDuplicado;";

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
                        NormalizarCodigo(codigo);

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        NormalizarTexto(
                            nombre);

                SqlParameter idExcluido =
                    command.Parameters.Add(
                        "@IdGrupoExcluido",
                        SqlDbType.Int);

                idExcluido.Value =
                    idGrupoExcluido.HasValue
                        ? (object)idGrupoExcluido.Value
                        : DBNull.Value;

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        throw new PersistenciaException(
                            "No fue posible comprobar la unicidad del grupo.");
                    }

                    bool codigoDuplicado =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "CodigoDuplicado"));

                    bool nombreDuplicado =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "NombreDuplicado"));

                    if (codigoDuplicado)
                    {
                        throw new ReglaNegocioException(
                            "Ya existe un grupo con el código indicado.");
                    }

                    if (nombreDuplicado)
                    {
                        throw new ReglaNegocioException(
                            idGrupoExcluido.HasValue
                                ? "Ya existe otro grupo con el nombre indicado."
                                : "Ya existe un grupo con el nombre indicado.");
                    }
                }
            }
        }

        private static void ValidarPermisosActivosInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<int> idsPermisos)
        {
            StringBuilder sql =
                new StringBuilder(@"
SELECT
    COUNT(DISTINCT p.IdPermiso)
FROM dbo.Permiso AS p
WHERE
    p.Activo = 1
    AND p.IdPermiso IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    idsPermisos.ToList(),
                    "@IdPermiso",
                    sql);

            sql.Append(@"
);");

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql.ToString(),
                        connection,
                        transaction))
            {
                foreach (
                    SqlParameter parametro
                    in parametros)
                {
                    command.Parameters.Add(
                        parametro);
                }

                int cantidad =
                    Convert.ToInt32(
                        command.ExecuteScalar());

                if (cantidad != idsPermisos.Count)
                {
                    throw new ReglaNegocioException(
                        "Uno o más permisos indicados no existen o se encuentran inactivos.");
                }
            }
        }

        private static int InsertarGrupoInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Grupo grupo)
        {
            const string sql = @"
INSERT INTO dbo.Grupo
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
                AgregarParametrosGrupo(
                    command,
                    grupo,
                    false);

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void ActualizarGrupoInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Grupo grupo)
        {
            const string sql = @"
UPDATE dbo.Grupo
SET
    Nombre = @Nombre,
    Descripcion = @Descripcion
WHERE IdGrupo = @IdGrupo;";

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
                        grupo.Nombre;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        grupo.Descripcion;

                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        grupo.IdGrupo;

                int filas =
                    command.ExecuteNonQuery();

                ExigirUnaFilaGrupo(
                    filas,
                    "actualizar");
            }
        }

        private static void AgregarParametrosGrupo(
            SqlCommand command,
            Grupo grupo,
            bool incluirId)
        {
            command.Parameters.Add(
                "@Codigo",
                SqlDbType.NVarChar,
                100).Value =
                    grupo.Codigo;

            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                150).Value =
                    grupo.Nombre;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                500).Value =
                    grupo.Descripcion;

            command.Parameters.Add(
                "@Activo",
                SqlDbType.Bit).Value =
                    grupo.Activo;

            if (incluirId)
            {
                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        grupo.IdGrupo;
            }
        }

        private static void EliminarGrupoPermisosInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupo)
        {
            const string sql = @"
DELETE FROM dbo.GrupoPermiso
WHERE IdGrupo = @IdGrupo;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.ExecuteNonQuery();
            }
        }

        private static void InsertarGrupoPermisosInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupo,
            IEnumerable<int> idsPermisos)
        {
            const string sql = @"
INSERT INTO dbo.GrupoPermiso
(
    IdGrupo,
    IdPermiso
)
VALUES
(
    @IdGrupo,
    @IdPermiso
);";

            foreach (int idPermiso in idsPermisos)
            {
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql,
                            connection,
                            transaction))
                {
                    command.Parameters.Add(
                        "@IdGrupo",
                        SqlDbType.Int).Value =
                            idGrupo;

                    command.Parameters.Add(
                        "@IdPermiso",
                        SqlDbType.Int).Value =
                            idPermiso;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void ValidarGruposHijosActivosInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<int> idsGruposHijos)
        {
            if (idsGruposHijos.Count == 0)
            {
                return;
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT COUNT(DISTINCT g.IdGrupo)
FROM dbo.Grupo AS g
WHERE
    g.Activo = 1
    AND g.IdGrupo IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    idsGruposHijos.ToList(),
                    "@IdGrupoHijo",
                    sql);

            sql.Append(@"
);");

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql.ToString(),
                        connection,
                        transaction))
            {
                foreach (
                    SqlParameter parametro
                    in parametros)
                {
                    command.Parameters.Add(
                        parametro);
                }

                int cantidad =
                    Convert.ToInt32(
                        command.ExecuteScalar());

                if (cantidad !=
                    idsGruposHijos.Count)
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos hijos no existen o se encuentran inactivos.");
                }
            }
        }

        private static void ValidarGruposHijosDisponiblesInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre,
            IReadOnlyCollection<int> idsGruposHijos)
        {
            if (idsGruposHijos.Count == 0)
            {
                return;
            }

            StringBuilder sql =
                new StringBuilder(@"
SELECT COUNT(DISTINCT g.IdGrupo)
FROM dbo.Grupo AS g
WHERE
    g.IdGrupo IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    idsGruposHijos.ToList(),
                    "@IdGrupoHijo",
                    sql);

            sql.Append(@"
)
AND
(
    g.Activo = 1
    OR EXISTS
    (
        SELECT 1
        FROM dbo.GrupoGrupo AS gg
        WHERE
            gg.IdGrupoPadre = @IdGrupoPadre
            AND gg.IdGrupoHijo = g.IdGrupo
    )
);");

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql.ToString(),
                        connection,
                        transaction))
            {
                foreach (
                    SqlParameter parametro
                    in parametros)
                {
                    command.Parameters.Add(
                        parametro);
                }

                command.Parameters.Add(
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                int cantidad =
                    Convert.ToInt32(
                        command.ExecuteScalar());

                if (cantidad !=
                    idsGruposHijos.Count)
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos hijos no existen, están inactivos o no pertenecían a la jerarquía.");
                }
            }
        }

        private static void ValidarAusenciaDeCicloInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre,
            IReadOnlyCollection<int> idsGruposHijos)
        {
            if (idsGruposHijos.Count == 0)
            {
                return;
            }

            StringBuilder sql =
                new StringBuilder(@"
;WITH Descendencia AS
(
    SELECT
        g.IdGrupo AS IdGrupoActual,
        CAST(
            N'/' +
            CONVERT(NVARCHAR(20), g.IdGrupo) +
            N'/'
            AS NVARCHAR(MAX)
        ) AS Camino
    FROM dbo.Grupo AS g
    WHERE g.IdGrupo IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    idsGruposHijos.ToList(),
                    "@IdGrupoHijo",
                    sql);

            sql.Append(@"
)

    UNION ALL

    SELECT
        gg.IdGrupoHijo,
        CAST(
            descendencia.Camino +
            CONVERT(NVARCHAR(20), gg.IdGrupoHijo) +
            N'/'
            AS NVARCHAR(MAX)
        )
    FROM Descendencia AS descendencia
    INNER JOIN dbo.GrupoGrupo AS gg
        ON gg.IdGrupoPadre =
            descendencia.IdGrupoActual
    WHERE CHARINDEX(
        N'/' +
        CONVERT(NVARCHAR(20), gg.IdGrupoHijo) +
        N'/',
        descendencia.Camino
    ) = 0
)
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM Descendencia
            WHERE IdGrupoActual = @IdGrupoPadre
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END
OPTION (MAXRECURSION 32767);");

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql.ToString(),
                        connection,
                        transaction))
            {
                foreach (
                    SqlParameter parametro
                    in parametros)
                {
                    command.Parameters.Add(
                        parametro);
                }

                command.Parameters.Add(
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                bool produceCiclo =
                    Convert.ToBoolean(
                        command.ExecuteScalar());

                if (produceCiclo)
                {
                    throw new ReglaNegocioException(
                        "La asociación produciría un ciclo entre grupos.");
                }
            }
        }

        private static void EliminarGrupoGruposInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre)
        {
            const string sql = @"
DELETE FROM dbo.GrupoGrupo
WHERE IdGrupoPadre = @IdGrupoPadre;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                command.ExecuteNonQuery();
            }
        }

        private static void InsertarGrupoGruposInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre,
            IEnumerable<int> idsGruposHijos)
        {
            const string sql = @"
INSERT INTO dbo.GrupoGrupo
(
    IdGrupoPadre,
    IdGrupoHijo
)
VALUES
(
    @IdGrupoPadre,
    @IdGrupoHijo
);";

            foreach (
                int idGrupoHijo
                in idsGruposHijos)
            {
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql,
                            connection,
                            transaction))
                {
                    command.Parameters.Add(
                        "@IdGrupoPadre",
                        SqlDbType.Int).Value =
                            idGrupoPadre;

                    command.Parameters.Add(
                        "@IdGrupoHijo",
                        SqlDbType.Int).Value =
                            idGrupoHijo;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static Grupo ReconstruirGrupo(
            SqlDataReader reader)
        {
            Grupo grupo =
                new Grupo(
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupo")),
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
                grupo.Desactivar();
            }

            return grupo;
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

        private static GrupoListadoDto CrearGrupoListadoDto(
            SqlDataReader reader)
        {
            return new GrupoListadoDto(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdGrupo")),
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
                        "CantidadPermisos")),
                reader.GetInt32(
                    reader.GetOrdinal(
                        "CantidadUsuarios")),
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
        g.Codigo LIKE @TextoGeneral
        OR g.Nombre LIKE @TextoGeneral
        OR g.Descripcion LIKE @TextoGeneral
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

        private static List<SqlParameter>
            CrearParametrosIds(
                IList<int> ids,
                string prefijo,
                StringBuilder sql)
        {
            List<SqlParameter> parametros =
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
                    prefijo +
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

            return parametros;
        }

        private static List<int> ValidarIds(
            IReadOnlyCollection<int> ids,
            string nombreParametro)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(
                    nombreParametro);
            }

            List<int> resultado =
                ids
                    .Distinct()
                    .ToList();

            if (resultado.Any(
                id => id <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    "Los identificadores deben ser mayores que cero.");
            }

            return resultado;
        }

        private static List<int> ValidarIdsObligatorios(
            IReadOnlyCollection<int> ids,
            string nombreParametro)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(
                    nombreParametro);
            }

            List<int> resultado =
                ids.ToList();

            if (resultado.Count == 0)
            {
                throw new ArgumentException(
                    "Debe indicar al menos un permiso.",
                    nombreParametro);
            }

            if (resultado.Any(
                id => id <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    "Los identificadores deben ser mayores que cero.");
            }

            if (resultado.Distinct().Count() !=
                resultado.Count)
            {
                throw new ArgumentException(
                    "No se admiten identificadores duplicados.",
                    nombreParametro);
            }

            return resultado;
        }

        private static void ValidarIdGrupo(
            int idGrupo)
        {
            if (idGrupo <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idGrupo));
            }
        }

        private static void ExigirUnaFilaGrupo(
            int filas,
            string operacion)
        {
            if (filas != 1)
            {
                throw new PersistenciaException(
                    "No fue posible " +
                    operacion +
                    " el grupo porque el registro no existe.");
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

        private static ReglaNegocioException CrearErrorDuplicado(
            SqlException exception,
            bool modificacion)
        {
            string mensajeSql =
                exception.Message
                ?? string.Empty;

            if (mensajeSql.IndexOf(
                "PK_GrupoPermiso",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ReglaNegocioException(
                    "No se pueden asignar permisos duplicados al grupo.",
                    exception);
            }

            if (mensajeSql.IndexOf(
                "PK_GrupoGrupo",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ReglaNegocioException(
                    "No se pueden asignar grupos hijos duplicados.",
                    exception);
            }

            return new ReglaNegocioException(
                modificacion
                    ? "Ya existe otro grupo con el código indicado."
                    : "Ya existe un grupo con el código indicado.",
                exception);
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

        private static void ExigirSiguienteResultado(
            SqlDataReader reader,
            string descripcion)
        {
            if (!reader.NextResult())
            {
                throw new PersistenciaException(
                    "La consulta no devolvió el conjunto esperado de " +
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

        private static string NormalizarTexto(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }

        private static string NormalizarCodigo(
            string codigo)
        {
            return NormalizarTexto(
                codigo)
                .ToUpperInvariant();
        }
    }
}
