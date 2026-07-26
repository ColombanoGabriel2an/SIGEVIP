USE SIGEVIP;
GO

SET NOCOUNT ON;
GO

SELECT
    @@SERVERNAME AS Servidor,
    DB_NAME() AS BaseActual;
GO

/*
    1. Migración registrada.
*/
SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'004';
GO

/*
    2. Existencia de tablas.
*/
SELECT
    CASE
        WHEN OBJECT_ID(N'dbo.Viaje', N'U')
            IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaViaje,

    CASE
        WHEN OBJECT_ID(
            N'dbo.ViajeParticipante',
            N'U'
        ) IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaViajeParticipante;
GO

/*
    3. Columnas, tipos, longitudes y nulabilidad.
*/
SELECT
    tabla.name AS Tabla,
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,
    CASE
        WHEN tipo.name IN
        (
            N'nvarchar',
            N'nchar'
        )
            THEN columna.max_length / 2
        ELSE columna.max_length
    END AS Longitud,
    columna.precision AS Precision,
    columna.scale AS Escala,
    columna.is_nullable AS PermiteNulos,
    columna.is_identity AS EsIdentity
FROM sys.columns AS columna
INNER JOIN sys.tables AS tabla
    ON tabla.object_id =
        columna.object_id
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id =
        columna.user_type_id
WHERE tabla.object_id IN
(
    OBJECT_ID(N'dbo.Viaje'),
    OBJECT_ID(N'dbo.ViajeParticipante')
)
ORDER BY
    tabla.name,
    columna.column_id;
GO

/*
    4. Claves primarias e índices.
*/
SELECT
    tabla.name AS Tabla,
    indice.name AS Indice,
    indice.is_primary_key AS EsClavePrimaria,
    indice.is_unique AS EsUnico,
    columnaIndice.key_ordinal AS OrdenColumna,
    columna.name AS Columna
FROM sys.indexes AS indice
INNER JOIN sys.tables AS tabla
    ON tabla.object_id =
        indice.object_id
INNER JOIN sys.index_columns AS columnaIndice
    ON columnaIndice.object_id =
        indice.object_id
    AND columnaIndice.index_id =
        indice.index_id
INNER JOIN sys.columns AS columna
    ON columna.object_id =
        columnaIndice.object_id
    AND columna.column_id =
        columnaIndice.column_id
WHERE
    indice.object_id IN
    (
        OBJECT_ID(N'dbo.Viaje'),
        OBJECT_ID(N'dbo.ViajeParticipante')
    )
    AND
    (
        indice.is_primary_key = 1
        OR indice.name IN
        (
            N'IX_Viaje_FechaInicio',
            N'IX_Viaje_EstadoViaje',
            N'IX_ViajeParticipante_IdPersona'
        )
    )
ORDER BY
    tabla.name,
    indice.name,
    columnaIndice.key_ordinal;
GO

/*
    5. Claves foráneas.
*/
SELECT
    clave.name AS ClaveForanea,
    OBJECT_SCHEMA_NAME(
        clave.parent_object_id
    ) + N'.' +
    OBJECT_NAME(
        clave.parent_object_id
    ) AS TablaOrigen,
    columnaOrigen.name AS ColumnaOrigen,
    OBJECT_SCHEMA_NAME(
        clave.referenced_object_id
    ) + N'.' +
    OBJECT_NAME(
        clave.referenced_object_id
    ) AS TablaDestino,
    columnaDestino.name AS ColumnaDestino,
    clave.delete_referential_action_desc
        AS AccionEliminacion,
    clave.is_disabled AS Deshabilitada,
    clave.is_not_trusted AS NoConfiable
FROM sys.foreign_keys AS clave
INNER JOIN sys.foreign_key_columns AS relacion
    ON relacion.constraint_object_id =
        clave.object_id
INNER JOIN sys.columns AS columnaOrigen
    ON columnaOrigen.object_id =
        relacion.parent_object_id
    AND columnaOrigen.column_id =
        relacion.parent_column_id
INNER JOIN sys.columns AS columnaDestino
    ON columnaDestino.object_id =
        relacion.referenced_object_id
    AND columnaDestino.column_id =
        relacion.referenced_column_id
WHERE clave.parent_object_id =
    OBJECT_ID(N'dbo.ViajeParticipante')
ORDER BY clave.name;
GO

/*
    6. Restricciones CHECK.
*/
SELECT
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion,
    restriccion.is_disabled AS Deshabilitada,
    restriccion.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS restriccion
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Viaje')
ORDER BY restriccion.name;
GO

/*
    7. Restricciones DEFAULT.
*/
SELECT
    restriccion.name AS Restriccion,
    columna.name AS Columna,
    restriccion.definition AS Definicion
FROM sys.default_constraints AS restriccion
INNER JOIN sys.columns AS columna
    ON columna.object_id =
        restriccion.parent_object_id
    AND columna.column_id =
        restriccion.parent_column_id
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Viaje')
ORDER BY restriccion.name;
GO

/*
    8. Correspondencia de TipoViaje.
*/
SELECT
    valor.TipoViaje,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'Desplazamiento'),
        (CAST(2 AS TINYINT), N'EnOficina'),
        (CAST(3 AS TINYINT), N'EventoFeria')
) AS valor
(
    TipoViaje,
    Nombre
);
GO

/*
    9. Correspondencia de EstadoViaje.
*/
SELECT
    valor.EstadoViaje,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'Abierto'),
        (CAST(2 AS TINYINT), N'EnRendicion'),
        (CAST(3 AS TINYINT), N'Aprobado'),
        (CAST(4 AS TINYINT), N'Cancelado')
) AS valor
(
    EstadoViaje,
    Nombre
);
GO

/*
    10. Cantidades generales.
*/
SELECT
    COUNT(*) AS CantidadViajes
FROM dbo.Viaje;
GO

SELECT
    COUNT(*) AS CantidadParticipaciones
FROM dbo.ViajeParticipante;
GO

/*
    11. Cantidades por tipo y estado.
*/
SELECT
    TipoViaje,
    COUNT(*) AS Cantidad
FROM dbo.Viaje
GROUP BY TipoViaje
ORDER BY TipoViaje;
GO

SELECT
    EstadoViaje,
    COUNT(*) AS Cantidad
FROM dbo.Viaje
GROUP BY EstadoViaje
ORDER BY EstadoViaje;
GO

/*
    12. Viajes sin participantes.
    El resultado esperado es cero una vez que el módulo
    persista datos exclusivamente mediante ViajeRepository.
*/
SELECT
    COUNT(*) AS CantidadViajesSinParticipantes
FROM dbo.Viaje AS viaje
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ViajeParticipante AS participante
    WHERE participante.IdViaje =
        viaje.IdViaje
);
GO

/*
    13. Asociaciones duplicadas.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadParticipantesDuplicados
FROM
(
    SELECT
        IdViaje,
        IdPersona
    FROM dbo.ViajeParticipante
    GROUP BY
        IdViaje,
        IdPersona
    HAVING COUNT(*) > 1
) AS duplicados;
GO

/*
    14. Fechas inválidas.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadPeriodosInvalidos
FROM dbo.Viaje
WHERE FechaInicio > FechaFin;
GO

/*
    15. Montos anticipados negativos.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadMontosNegativos
FROM dbo.Viaje
WHERE MontoAnticipado < 0;
GO

/*
    16. Tipos o estados fuera de catálogo.
    Ambos resultados esperados son cero.
*/
SELECT
    COUNT(*) AS CantidadTiposInvalidos
FROM dbo.Viaje
WHERE TipoViaje NOT IN
(
    1,
    2,
    3
);
GO

SELECT
    COUNT(*) AS CantidadEstadosInvalidos
FROM dbo.Viaje
WHERE EstadoViaje NOT IN
(
    1,
    2,
    3,
    4
);
GO

/*
    17. Permisos del administrador de demostración.
*/
SELECT
    permiso.Codigo AS CodigoPermiso,
    COUNT(*) AS CantidadAsignaciones
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo =
        grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso =
        grupoPermiso.IdPermiso
WHERE
    grupo.Codigo =
        N'ADMINISTRADOR_GENERAL'
    AND permiso.Codigo IN
    (
        N'VIAJE_CONSULTAR',
        N'VIAJE_CREAR',
        N'VIAJE_CANCELAR'
    )
GROUP BY permiso.Codigo
ORDER BY permiso.Codigo;
GO

/*
    18. Resumen final.
*/
SELECT
    CASE
        WHEN
            EXISTS
            (
                SELECT 1
                FROM dbo.VersionBaseDatos
                WHERE NumeroVersion = N'004'
            )
            AND OBJECT_ID(
                N'dbo.Viaje',
                N'U'
            ) IS NOT NULL
            AND OBJECT_ID(
                N'dbo.ViajeParticipante',
                N'U'
            ) IS NOT NULL
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(N'dbo.Viaje')
                    AND name =
                        N'IX_Viaje_FechaInicio'
            )
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(N'dbo.Viaje')
                    AND name =
                        N'IX_Viaje_EstadoViaje'
            )
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(
                            N'dbo.ViajeParticipante'
                        )
                    AND name =
                        N'IX_ViajeParticipante_IdPersona'
            )
            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.Viaje
                WHERE
                    FechaInicio > FechaFin
                    OR MontoAnticipado < 0
                    OR TipoViaje NOT IN
                    (
                        1,
                        2,
                        3
                    )
                    OR EstadoViaje NOT IN
                    (
                        1,
                        2,
                        3,
                        4
                    )
            )
            AND NOT EXISTS
            (
                SELECT
                    IdViaje,
                    IdPersona
                FROM dbo.ViajeParticipante
                GROUP BY
                    IdViaje,
                    IdPersona
                HAVING COUNT(*) > 1
            )
            AND
            (
                SELECT COUNT(DISTINCT permiso.Codigo)
                FROM dbo.GrupoPermiso AS grupoPermiso
                INNER JOIN dbo.Grupo AS grupo
                    ON grupo.IdGrupo =
                        grupoPermiso.IdGrupo
                INNER JOIN dbo.Permiso AS permiso
                    ON permiso.IdPermiso =
                        grupoPermiso.IdPermiso
                WHERE
                    grupo.Codigo =
                        N'ADMINISTRADOR_GENERAL'
                    AND permiso.Codigo IN
                    (
                        N'VIAJE_CONSULTAR',
                        N'VIAJE_CREAR',
                        N'VIAJE_CANCELAR'
                    )
            ) = 3
        THEN N'VALIDACIÓN CORRECTA'
        ELSE N'VALIDACIÓN INCOMPLETA'
    END AS ResultadoValidacion;
GO
