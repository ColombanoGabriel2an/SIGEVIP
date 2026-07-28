using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Usuarios
{
    public sealed class UsuarioGestionRepository
        : IUsuarioGestionRepository
    {
        private const int ErrorIndiceDuplicado = 2601;
        private const int ErrorRestriccionUnica = 2627;

        private const string CodigoGrupoAdministrador =
            "ADMINISTRADOR_GENERAL";

        private readonly SqlConnectionFactory
            _connectionFactory;

        public UsuarioGestionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public IReadOnlyCollection<UsuarioListadoDto> Listar(
            UsuarioFiltro filtro)
        {
            UsuarioFiltro filtroEfectivo =
                filtro
                ?? UsuarioFiltro.CrearSinFiltros();

            StringBuilder sql =
                new StringBuilder(@"
SELECT
    u.IdUsuario,
    u.NombreUsuario,
    LTRIM(RTRIM(
        p.Nombre + N' ' + p.Apellido
    )) AS NombreCompleto,
    p.Email,
    ISNULL(
        grupos.GruposResumen,
        N''
    ) AS GruposResumen,
    u.Activo
FROM dbo.Usuario AS u
INNER JOIN dbo.Persona AS p
    ON p.IdPersona = u.IdPersona
OUTER APPLY
(
    SELECT
        STUFF
        (
            (
                SELECT
                    N', ' + g.Nombre
                FROM dbo.UsuarioGrupo AS ugResumen
                INNER JOIN dbo.Grupo AS g
                    ON g.IdGrupo = ugResumen.IdGrupo
                WHERE
                    ugResumen.IdUsuario = u.IdUsuario
                ORDER BY
                    g.Nombre,
                    g.IdGrupo
                FOR XML PATH(N''), TYPE
            ).value(
                N'.',
                N'NVARCHAR(MAX)'
            ),
            1,
            2,
            N''
        ) AS GruposResumen
) AS grupos
WHERE 1 = 1");

            List<SqlParameter> parametros =
                new List<SqlParameter>();

            AgregarFiltroTextoGeneral(
                sql,
                parametros,
                filtroEfectivo.TextoGeneral);

            if (filtroEfectivo.Activo.HasValue)
            {
                sql.Append(@"
    AND u.Activo = @Activo");

                parametros.Add(
                    new SqlParameter(
                        "@Activo",
                        SqlDbType.Bit)
                    {
                        Value =
                            filtroEfectivo.Activo.Value
                    });
            }

            if (filtroEfectivo.IdGrupo.HasValue)
            {
                sql.Append(@"
    AND EXISTS
    (
        SELECT 1
        FROM dbo.UsuarioGrupo AS ugFiltro
        WHERE
            ugFiltro.IdUsuario = u.IdUsuario
            AND ugFiltro.IdGrupo = @IdGrupo
    )");

                parametros.Add(
                    new SqlParameter(
                        "@IdGrupo",
                        SqlDbType.Int)
                    {
                        Value =
                            filtroEfectivo.IdGrupo.Value
                    });
            }

            sql.Append(@"
ORDER BY
    u.NombreUsuario,
    u.IdUsuario;");

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

                    List<UsuarioListadoDto> resultados =
                        new List<UsuarioListadoDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                CrearUsuarioListadoDto(
                                    reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar los usuarios.",
                    exception);
            }
        }

        public UsuarioDetalleDto ObtenerDetallePorId(
            int idUsuario)
        {
            ValidarIdUsuario(
                idUsuario);

            const string sql = @"
SELECT
    u.IdUsuario,
    u.IdPersona,
    u.NombreUsuario,
    LTRIM(RTRIM(
        p.Nombre + N' ' + p.Apellido
    )) AS NombreCompleto,
    p.Email,
    u.Activo
FROM dbo.Usuario AS u
INNER JOIN dbo.Persona AS p
    ON p.IdPersona = u.IdPersona
WHERE u.IdUsuario = @IdUsuario;

SELECT
    ug.IdGrupo
FROM dbo.UsuarioGrupo AS ug
WHERE ug.IdUsuario = @IdUsuario
ORDER BY ug.IdGrupo;";

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
                        "@IdUsuario",
                        SqlDbType.Int).Value =
                            idUsuario;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        int idPersona =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "IdPersona"));

                        string nombreUsuario =
                            LeerTextoObligatorio(
                                reader,
                                "NombreUsuario");

                        string nombreCompleto =
                            LeerTextoObligatorio(
                                reader,
                                "NombreCompleto");

                        string email =
                            LeerTextoObligatorio(
                                reader,
                                "Email");

                        bool activo =
                            reader.GetBoolean(
                                reader.GetOrdinal(
                                    "Activo"));

                        ExigirSiguienteResultado(
                            reader,
                            "grupos directos del usuario");

                        List<int> idsGrupos =
                            new List<int>();

                        while (reader.Read())
                        {
                            idsGrupos.Add(
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdGrupo")));
                        }

                        return new UsuarioDetalleDto(
                            idUsuario,
                            idPersona,
                            nombreUsuario,
                            nombreCompleto,
                            email,
                            activo,
                            idsGrupos);
                    }
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el detalle del usuario.",
                    exception);
            }
        }

        public Usuario ObtenerPorId(
            int idUsuario)
        {
            ValidarIdUsuario(
                idUsuario);

            const string sql = @"
SELECT
    u.IdUsuario,
    u.IdPersona,
    u.NombreUsuario,
    u.PasswordHash,
    u.PasswordSalt,
    u.IteracionesPassword,
    u.Activo
FROM dbo.Usuario AS u
WHERE u.IdUsuario = @IdUsuario;

SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.UsuarioGrupo AS ug
INNER JOIN dbo.Grupo AS g
    ON g.IdGrupo = ug.IdGrupo
WHERE ug.IdUsuario = @IdUsuario
ORDER BY
    g.Codigo,
    g.IdGrupo;";

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
                        "@IdUsuario",
                        SqlDbType.Int).Value =
                            idUsuario;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        Usuario usuario =
                            ReconstruirUsuario(
                                reader);

                        ExigirSiguienteResultado(
                            reader,
                            "grupos directos del usuario");

                        while (reader.Read())
                        {
                            usuario.AgregarGrupo(
                                ReconstruirGrupo(
                                    reader));
                        }

                        return usuario;
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
                    "Los datos persistidos del usuario son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar el usuario.",
                    exception);
            }
        }

        public Persona ObtenerPersonaPorId(
            int idPersona)
        {
            ValidarIdPersona(
                idPersona);

            const string sql = @"
SELECT
    p.IdPersona,
    p.Nombre,
    p.Apellido,
    p.Email,
    p.Activo
FROM dbo.Persona AS p
WHERE p.IdPersona = @IdPersona;";

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
                        "@IdPersona",
                        SqlDbType.Int).Value =
                            idPersona;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader(
                                CommandBehavior.SingleRow))
                    {
                        return reader.Read()
                            ? ReconstruirPersona(
                                reader)
                            : null;
                    }
                }
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de la persona son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar la persona.",
                    exception);
            }
        }

        public IReadOnlyCollection<PersonaSeleccionUsuarioDto>
            ListarPersonasDisponibles()
        {
            const string sql = @"
SELECT
    p.IdPersona,
    LTRIM(RTRIM(
        p.Nombre + N' ' + p.Apellido
    )) AS NombreCompleto,
    p.Email,
    p.Activo
FROM dbo.Persona AS p
WHERE
    p.Activo = 1
    AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuario AS u
        WHERE u.IdPersona = p.IdPersona
    )
ORDER BY
    p.Apellido,
    p.Nombre,
    p.IdPersona;";

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

                    List<PersonaSeleccionUsuarioDto> resultados =
                        new List<PersonaSeleccionUsuarioDto>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new PersonaSeleccionUsuarioDto(
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
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible listar las personas disponibles.",
                    exception);
            }
        }

        public IReadOnlyCollection<GrupoSeleccionUsuarioDto>
            ListarGruposActivos(
                IReadOnlyCollection<int> idsSeleccionados)
        {
            HashSet<int> seleccionados =
                new HashSet<int>(
                    idsSeleccionados
                    ?? new int[0]);

            const string sql = @"
SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Activo
FROM dbo.Grupo AS g
WHERE g.Activo = 1
ORDER BY
    g.Nombre,
    g.IdGrupo;";

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

                    List<GrupoSeleccionUsuarioDto> resultados =
                        new List<GrupoSeleccionUsuarioDto>();

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
                                new GrupoSeleccionUsuarioDto(
                                    idGrupo,
                                    LeerTextoObligatorio(
                                        reader,
                                        "Codigo"),
                                    LeerTextoObligatorio(
                                        reader,
                                        "Nombre"),
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
                    "No fue posible listar los grupos activos.",
                    exception);
            }
        }

        public IReadOnlyCollection<Grupo> ObtenerGruposPorIds(
            IReadOnlyCollection<int> idsGrupos)
        {
            List<int> ids =
                ValidarIdsGrupos(
                    idsGrupos);

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
ORDER BY g.IdGrupo;");

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

                    List<Grupo> resultados =
                        new List<Grupo>();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                ReconstruirGrupo(
                                    reader));
                        }
                    }

                    return resultados.AsReadOnly();
                }
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de los grupos son inválidos.",
                    exception);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible consultar los grupos seleccionados.",
                    exception);
            }
        }

        public bool ExisteNombreUsuario(
            string nombreUsuario,
            int? idUsuarioExcluido)
        {
            string nombreNormalizado =
                NormalizarNombreUsuario(
                    nombreUsuario);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE
                u.NombreUsuario = @NombreUsuario
                AND
                (
                    @IdUsuarioExcluido IS NULL
                    OR u.IdUsuario <> @IdUsuarioExcluido
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
                        "@NombreUsuario",
                        SqlDbType.NVarChar,
                        100).Value =
                            nombreNormalizado;

                    SqlParameter idExcluido =
                        command.Parameters.Add(
                            "@IdUsuarioExcluido",
                            SqlDbType.Int);

                    idExcluido.Value =
                        idUsuarioExcluido.HasValue
                            ? (object)idUsuarioExcluido.Value
                            : DBNull.Value;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible comprobar el nombre de usuario.",
                    exception);
            }
        }

        public bool PersonaTieneUsuario(
            int idPersona)
        {
            ValidarIdPersona(
                idPersona);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE u.IdPersona = @IdPersona
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
                        "@IdPersona",
                        SqlDbType.Int).Value =
                            idPersona;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible comprobar la asociación de la persona.",
                    exception);
            }
        }

        public bool ExisteOtroAdministradorActivo(
            int idUsuarioExcluido)
        {
            ValidarIdUsuario(
                idUsuarioExcluido);

            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            INNER JOIN dbo.UsuarioGrupo AS ug
                ON ug.IdUsuario = u.IdUsuario
            INNER JOIN dbo.Grupo AS g
                ON g.IdGrupo = ug.IdGrupo
            WHERE
                u.Activo = 1
                AND g.Activo = 1
                AND g.Codigo = @CodigoGrupo
                AND u.IdUsuario <> @IdUsuarioExcluido
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
                        "@CodigoGrupo",
                        SqlDbType.NVarChar,
                        100).Value =
                            CodigoGrupoAdministrador;

                    command.Parameters.Add(
                        "@IdUsuarioExcluido",
                        SqlDbType.Int).Value =
                            idUsuarioExcluido;

                    connection.Open();

                    return Convert.ToBoolean(
                        command.ExecuteScalar());
                }
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible comprobar los administradores activos.",
                    exception);
            }
        }

        public int Insertar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos)
        {
            return InsertarInterno(
                usuario,
                idsGrupos,
                null);
        }

        public int Insertar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            return InsertarInterno(
                usuario,
                idsGrupos,
                auditoria);
        }

        private int InsertarInterno(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(
                    nameof(usuario));
            }

            List<int> ids =
                ValidarIdsGruposObligatorios(
                    idsGrupos);

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
                            ValidarPersonaDisponibleInterna(
                                connection,
                                transaction,
                                usuario.IdPersona);

                            if (ExisteNombreUsuarioInterno(
                                connection,
                                transaction,
                                usuario.NombreUsuario,
                                null))
                            {
                                throw new ReglaNegocioException(
                                    "Ya existe un usuario con el nombre indicado.");
                            }

                            ValidarGruposActivosInterno(
                                connection,
                                transaction,
                                ids);

                            int idUsuario =
                                InsertarUsuarioInterno(
                                    connection,
                                    transaction,
                                    usuario);

                            InsertarUsuarioGruposInterno(
                                connection,
                                transaction,
                                idUsuario,
                                ids);

                            if (auditoria != null)
                            {
                                AuditoriaSqlWriter.Insertar(
                                    connection,
                                    transaction,
                                    auditoria.ConIdEntidad(
                                        idUsuario));
                            }

                            transaction.Commit();

                            return idUsuario;
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
                throw CrearErrorDuplicado(
                    exception,
                    false);
            }
            catch (SqlException exception)
            {
                throw CrearErrorPersistencia(
                    "No fue posible registrar el usuario.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de registro del usuario no pudo completarse.",
                    exception);
            }
        }

        public void Actualizar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos)
        {
            ActualizarInterno(
                usuario,
                idsGrupos,
                null);
        }

        public void Actualizar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarInterno(
                usuario,
                idsGrupos,
                auditoria);
        }

        private void ActualizarInterno(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(
                    nameof(usuario));
            }

            ValidarIdUsuario(
                usuario.IdUsuario);

            List<int> ids =
                ValidarIdsGruposObligatorios(
                    idsGrupos);

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
                            if (ExisteNombreUsuarioInterno(
                                connection,
                                transaction,
                                usuario.NombreUsuario,
                                usuario.IdUsuario))
                            {
                                throw new ReglaNegocioException(
                                    "Ya existe otro usuario con el nombre indicado.");
                            }

                            ValidarGruposActivosInterno(
                                connection,
                                transaction,
                                ids);

                            ActualizarUsuarioInterno(
                                connection,
                                transaction,
                                usuario);

                            EliminarUsuarioGruposInterno(
                                connection,
                                transaction,
                                usuario.IdUsuario);

                            InsertarUsuarioGruposInterno(
                                connection,
                                transaction,
                                usuario.IdUsuario,
                                ids);

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
                    "No fue posible actualizar el usuario.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de actualización del usuario no pudo completarse.",
                    exception);
            }
        }

        public void Activar(
            int idUsuario)
        {
            ActualizarEstadoInterno(
                idUsuario,
                true,
                null);
        }

        public void Activar(
            int idUsuario,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idUsuario,
                true,
                auditoria);
        }

        public void Desactivar(
            int idUsuario)
        {
            ActualizarEstadoInterno(
                idUsuario,
                false,
                null);
        }

        public void Desactivar(
            int idUsuario,
            AuditoriaRegistro auditoria)
        {
            if (auditoria == null)
            {
                throw new ArgumentNullException(
                    nameof(auditoria));
            }

            ActualizarEstadoInterno(
                idUsuario,
                false,
                auditoria);
        }

        private void ActualizarEstadoInterno(
            int idUsuario,
            bool activo,
            AuditoriaRegistro auditoria)
        {
            ValidarIdUsuario(
                idUsuario);

            const string sql = @"
UPDATE dbo.Usuario
SET Activo = @Activo
WHERE IdUsuario = @IdUsuario;";

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
                                    "@IdUsuario",
                                    SqlDbType.Int).Value =
                                        idUsuario;

                                ExigirUnaFilaUsuario(
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
                    "No fue posible actualizar el estado del usuario.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de estado del usuario no pudo completarse.",
                    exception);
            }
        }

        private static void ValidarPersonaDisponibleInterna(
            SqlConnection connection,
            SqlTransaction transaction,
            int idPersona)
        {
            const string sql = @"
SELECT
    p.Activo,
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE u.IdPersona = p.IdPersona
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS TieneUsuario
FROM dbo.Persona AS p
WHERE p.IdPersona = @IdPersona;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        throw new ReglaNegocioException(
                            "La persona indicada no existe.");
                    }

                    bool personaActiva =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "Activo"));

                    bool tieneUsuario =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "TieneUsuario"));

                    if (!personaActiva)
                    {
                        throw new ReglaNegocioException(
                            "La persona indicada se encuentra inactiva.");
                    }

                    if (tieneUsuario)
                    {
                        throw new ReglaNegocioException(
                            "La persona indicada ya posee un usuario.");
                    }
                }
            }
        }

        private static bool ExisteNombreUsuarioInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            string nombreUsuario,
            int? idUsuarioExcluido)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Usuario AS u
            WHERE
                u.NombreUsuario = @NombreUsuario
                AND
                (
                    @IdUsuarioExcluido IS NULL
                    OR u.IdUsuario <> @IdUsuarioExcluido
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
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        NormalizarNombreUsuario(
                            nombreUsuario);

                SqlParameter idExcluido =
                    command.Parameters.Add(
                        "@IdUsuarioExcluido",
                        SqlDbType.Int);

                idExcluido.Value =
                    idUsuarioExcluido.HasValue
                        ? (object)idUsuarioExcluido.Value
                        : DBNull.Value;

                return Convert.ToBoolean(
                    command.ExecuteScalar());
            }
        }

        private static void ValidarGruposActivosInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<int> idsGrupos)
        {
            StringBuilder sql =
                new StringBuilder(@"
SELECT
    COUNT(DISTINCT g.IdGrupo)
FROM dbo.Grupo AS g
WHERE
    g.Activo = 1
    AND g.IdGrupo IN
(");

            List<SqlParameter> parametros =
                CrearParametrosIds(
                    idsGrupos.ToList(),
                    "@IdGrupo",
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

                if (cantidad != idsGrupos.Count)
                {
                    throw new ReglaNegocioException(
                        "Uno o más grupos indicados no existen o se encuentran inactivos.");
                }
            }
        }

        private static int InsertarUsuarioInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Usuario usuario)
        {
            const string sql = @"
INSERT INTO dbo.Usuario
(
    IdPersona,
    NombreUsuario,
    PasswordHash,
    PasswordSalt,
    IteracionesPassword,
    Activo
)
VALUES
(
    @IdPersona,
    @NombreUsuario,
    @PasswordHash,
    @PasswordSalt,
    @IteracionesPassword,
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
                byte[] passwordHash =
                    usuario.PasswordHash;

                byte[] passwordSalt =
                    usuario.PasswordSalt;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        usuario.IdPersona;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        usuario.NombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    passwordHash.Length).Value =
                        passwordHash;

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    passwordSalt.Length).Value =
                        passwordSalt;

                command.Parameters.Add(
                    "@IteracionesPassword",
                    SqlDbType.Int).Value =
                        usuario.IteracionesPassword;

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        usuario.Activo;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void InsertarUsuarioGruposInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idUsuario,
            IEnumerable<int> idsGrupos)
        {
            const string sql = @"
INSERT INTO dbo.UsuarioGrupo
(
    IdUsuario,
    IdGrupo
)
VALUES
(
    @IdUsuario,
    @IdGrupo
);";

            foreach (int idGrupo in idsGrupos)
            {
                using (
                    SqlCommand command =
                        new SqlCommand(
                            sql,
                            connection,
                            transaction))
                {
                    command.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int).Value =
                            idUsuario;

                    command.Parameters.Add(
                        "@IdGrupo",
                        SqlDbType.Int).Value =
                            idGrupo;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void ActualizarUsuarioInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            Usuario usuario)
        {
            const string sql = @"
UPDATE dbo.Usuario
SET NombreUsuario = @NombreUsuario
WHERE IdUsuario = @IdUsuario;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        usuario.NombreUsuario;

                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        usuario.IdUsuario;

                int filas =
                    command.ExecuteNonQuery();

                ExigirUnaFilaUsuario(
                    filas,
                    "actualizar");
            }
        }

        private static void EliminarUsuarioGruposInterno(
            SqlConnection connection,
            SqlTransaction transaction,
            int idUsuario)
        {
            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.ExecuteNonQuery();
            }
        }

        private static Usuario ReconstruirUsuario(
            SqlDataReader reader)
        {
            Usuario usuario =
                new Usuario(
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdUsuario")),
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdPersona")),
                    LeerTextoObligatorio(
                        reader,
                        "NombreUsuario"),
                    LeerBytesObligatorios(
                        reader,
                        "PasswordHash"),
                    LeerBytesObligatorios(
                        reader,
                        "PasswordSalt"),
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IteracionesPassword")));

            bool activo =
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo"));

            if (!activo)
            {
                usuario.Desactivar();
            }

            return usuario;
        }

        private static Persona ReconstruirPersona(
            SqlDataReader reader)
        {
            Persona persona =
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

            bool activo =
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo"));

            if (!activo)
            {
                persona.Desactivar();
            }

            return persona;
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

        private static UsuarioListadoDto CrearUsuarioListadoDto(
            SqlDataReader reader)
        {
            return new UsuarioListadoDto(
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IdUsuario")),
                LeerTextoObligatorio(
                    reader,
                    "NombreUsuario"),
                LeerTextoObligatorio(
                    reader,
                    "NombreCompleto"),
                LeerTextoObligatorio(
                    reader,
                    "Email"),
                LeerTextoOpcional(
                    reader,
                    "GruposResumen"),
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "Activo")));
        }

        private static void AgregarFiltroTextoGeneral(
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
        u.NombreUsuario LIKE @TextoGeneral
        OR p.Nombre LIKE @TextoGeneral
        OR p.Apellido LIKE @TextoGeneral
        OR p.Email LIKE @TextoGeneral
        OR EXISTS
        (
            SELECT 1
            FROM dbo.UsuarioGrupo AS ugTexto
            INNER JOIN dbo.Grupo AS gTexto
                ON gTexto.IdGrupo = ugTexto.IdGrupo
            WHERE
                ugTexto.IdUsuario = u.IdUsuario
                AND
                (
                    gTexto.Codigo LIKE @TextoGeneral
                    OR gTexto.Nombre LIKE @TextoGeneral
                )
        )
    )");

            parametros.Add(
                new SqlParameter(
                    "@TextoGeneral",
                    SqlDbType.NVarChar,
                    254)
                {
                    Value =
                        "%" +
                        texto.Trim() +
                        "%"
                });
        }

        private static List<SqlParameter> CrearParametrosIds(
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

        private static List<int> ValidarIdsGrupos(
            IReadOnlyCollection<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGrupos));
            }

            List<int> ids =
                idsGrupos
                    .Distinct()
                    .ToList();

            if (ids.Any(
                idGrupo => idGrupo <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idsGrupos),
                    "Los identificadores de grupos deben ser mayores que cero.");
            }

            return ids;
        }

        private static List<int> ValidarIdsGruposObligatorios(
            IReadOnlyCollection<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGrupos));
            }

            List<int> ids =
                idsGrupos.ToList();

            if (ids.Count == 0)
            {
                throw new ArgumentException(
                    "Debe indicar al menos un grupo.",
                    nameof(idsGrupos));
            }

            if (ids.Any(
                idGrupo => idGrupo <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idsGrupos),
                    "Los identificadores de grupos deben ser mayores que cero.");
            }

            if (ids.Distinct().Count() !=
                ids.Count)
            {
                throw new ArgumentException(
                    "No se admiten identificadores de grupos duplicados.",
                    nameof(idsGrupos));
            }

            return ids;
        }

        private static void ValidarIdUsuario(
            int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario));
            }
        }

        private static void ValidarIdPersona(
            int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona));
            }
        }

        private static void ExigirUnaFilaUsuario(
            int filas,
            string operacion)
        {
            if (filas != 1)
            {
                throw new PersistenciaException(
                    "No fue posible " +
                    operacion +
                    " el usuario porque el registro no existe.");
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
                "UX_Usuario_IdPersona",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ReglaNegocioException(
                    "La persona indicada ya posee un usuario.",
                    exception);
            }

            if (mensajeSql.IndexOf(
                "PK_UsuarioGrupo",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ReglaNegocioException(
                    "No se pueden asignar grupos duplicados al usuario.",
                    exception);
            }

            return new ReglaNegocioException(
                modificacion
                    ? "Ya existe otro usuario con el nombre indicado."
                    : "Ya existe un usuario con el nombre indicado.",
                exception);
        }

        private static PersistenciaException CrearErrorPersistencia(
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

        private static byte[] LeerBytesObligatorios(
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

            byte[] valor =
                (byte[])reader.GetValue(
                    ordinal);

            if (valor.Length == 0)
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede estar vacía.");
            }

            return valor;
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

        private static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return string.IsNullOrWhiteSpace(
                nombreUsuario)
                ? string.Empty
                : nombreUsuario
                    .Trim()
                    .ToLowerInvariant();
        }
    }
}
