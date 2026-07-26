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
WHERE NumeroVersion = N'005';
GO

/*
    2. Existencia de las tablas.
*/
SELECT
    CASE
        WHEN OBJECT_ID(
            N'dbo.Visita',
            N'U'
        ) IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaVisita,

    CASE
        WHEN OBJECT_ID(
            N'dbo.VisitaCliente',
            N'U'
        ) IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaVisitaCliente;
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
    OBJECT_ID(N'dbo.Visita'),
    OBJECT_ID(N'dbo.VisitaCliente')
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
        OBJECT_ID(N'dbo.Visita'),
        OBJECT_ID(N'dbo.VisitaCliente')
    )
    AND
    (
        indice.is_primary_key = 1
        OR indice.name IN
        (
            N'IX_Visita_IdViaje_Fecha',
            N'IX_Visita_Fecha',
            N'IX_VisitaCliente_IdCliente'
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
    ) +
    N'.' +
    OBJECT_NAME(
        clave.parent_object_id
    ) AS TablaOrigen,

    columnaOrigen.name AS ColumnaOrigen,

    OBJECT_SCHEMA_NAME(
        clave.referenced_object_id
    ) +
    N'.' +
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
WHERE clave.parent_object_id IN
(
    OBJECT_ID(N'dbo.Visita'),
    OBJECT_ID(N'dbo.VisitaCliente')
)
ORDER BY clave.name;
GO

/*
    6. Restricciones CHECK de Visita.
*/
SELECT
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion,
    restriccion.is_disabled AS Deshabilitada,
    restriccion.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS restriccion
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Visita')
ORDER BY restriccion.name;
GO

/*
    7. Cantidades generales.
*/
SELECT
    COUNT(*) AS CantidadVisitas
FROM dbo.Visita;
GO

SELECT
    COUNT(*) AS CantidadRelacionesVisitaCliente
FROM dbo.VisitaCliente;
GO

/*
    8. Visitas sin Viaje.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadVisitasSinViaje
FROM dbo.Visita AS visita
LEFT JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        visita.IdViaje
WHERE viaje.IdViaje IS NULL;
GO

/*
    9. Visitas sin Clientes.
    El resultado esperado es cero una vez que los datos
    se persistan mediante VisitaRepository.
*/
SELECT
    COUNT(*) AS CantidadVisitasSinClientes
FROM dbo.Visita AS visita
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.VisitaCliente AS relacion
    WHERE relacion.IdVisita =
        visita.IdVisita
);
GO

/*
    10. Relaciones con Clientes inexistentes.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadRelacionesSinCliente
FROM dbo.VisitaCliente AS relacion
LEFT JOIN dbo.Cliente AS cliente
    ON cliente.IdCliente =
        relacion.IdCliente
WHERE cliente.IdCliente IS NULL;
GO

/*
    11. Relaciones duplicadas.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadClientesDuplicados
FROM
(
    SELECT
        IdVisita,
        IdCliente
    FROM dbo.VisitaCliente
    GROUP BY
        IdVisita,
        IdCliente
    HAVING COUNT(*) > 1
) AS duplicados;
GO

/*
    12. Textos inválidos.
    Ambos resultados esperados son cero.
*/
SELECT
    COUNT(*) AS CantidadObservacionesInvalidas
FROM dbo.Visita
WHERE
    Observacion IS NULL
    OR LEN(
        LTRIM(
            RTRIM(
                Observacion
            )
        )
    ) = 0;
GO

SELECT
    COUNT(*) AS CantidadLocalidadesInvalidas
FROM dbo.Visita
WHERE
    LocalidadEncuentro IS NULL
    OR LEN(
        LTRIM(
            RTRIM(
                LocalidadEncuentro
            )
        )
    ) = 0;
GO

/*
    13. Visitas fuera del período del Viaje.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadVisitasFueraDelPeriodo
FROM dbo.Visita AS visita
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        visita.IdViaje
WHERE
    visita.Fecha <
        viaje.FechaInicio
    OR visita.Fecha >
        viaje.FechaFin;
GO

/*
    14. Permiso del administrador de demostración.
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
    AND permiso.Codigo =
        N'VISITA_REGISTRAR'
GROUP BY permiso.Codigo;
GO

/*
    15. Resumen final.
*/
SELECT
    CASE
        WHEN
            EXISTS
            (
                SELECT 1
                FROM dbo.VersionBaseDatos
                WHERE NumeroVersion = N'005'
            )

            AND OBJECT_ID(
                N'dbo.Visita',
                N'U'
            ) IS NOT NULL

            AND OBJECT_ID(
                N'dbo.VisitaCliente',
                N'U'
            ) IS NOT NULL

            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(N'dbo.Visita')
                    AND name =
                        N'IX_Visita_IdViaje_Fecha'
            )

            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(N'dbo.Visita')
                    AND name =
                        N'IX_Visita_Fecha'
            )

            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id =
                        OBJECT_ID(
                            N'dbo.VisitaCliente'
                        )
                    AND name =
                        N'IX_VisitaCliente_IdCliente'
            )

            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.Visita AS visita
                LEFT JOIN dbo.Viaje AS viaje
                    ON viaje.IdViaje =
                        visita.IdViaje
                WHERE viaje.IdViaje IS NULL
            )

            AND NOT EXISTS
            (
                SELECT
                    IdVisita,
                    IdCliente
                FROM dbo.VisitaCliente
                GROUP BY
                    IdVisita,
                    IdCliente
                HAVING COUNT(*) > 1
            )

            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.Visita
                WHERE
                    Observacion IS NULL
                    OR LEN(
                        LTRIM(
                            RTRIM(
                                Observacion
                            )
                        )
                    ) = 0
                    OR LocalidadEncuentro IS NULL
                    OR LEN(
                        LTRIM(
                            RTRIM(
                                LocalidadEncuentro
                            )
                        )
                    ) = 0
            )

            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.Visita AS visita
                INNER JOIN dbo.Viaje AS viaje
                    ON viaje.IdViaje =
                        visita.IdViaje
                WHERE
                    visita.Fecha <
                        viaje.FechaInicio
                    OR visita.Fecha >
                        viaje.FechaFin
            )

            AND EXISTS
            (
                SELECT 1
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
                    AND permiso.Codigo =
                        N'VISITA_REGISTRAR'
            )

        THEN N'VALIDACIÓN CORRECTA'
        ELSE N'VALIDACIÓN INCOMPLETA'
    END AS ResultadoValidacion;
GO
