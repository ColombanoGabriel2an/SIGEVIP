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
WHERE NumeroVersion = N'003';
GO

/*
    2. Existencia de la tabla.
*/
SELECT
    CASE
        WHEN OBJECT_ID(N'dbo.Cliente', N'U') IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaCliente;
GO

/*
    3. Columnas, tipos, longitudes y nulabilidad.
*/
SELECT
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,
    CASE
        WHEN tipo.name IN (N'nvarchar', N'nchar')
            THEN columna.max_length / 2
        ELSE columna.max_length
    END AS Longitud,
    columna.is_nullable AS PermiteNulos,
    columna.is_identity AS EsIdentity
FROM sys.columns AS columna
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id = columna.user_type_id
WHERE columna.object_id = OBJECT_ID(N'dbo.Cliente')
ORDER BY columna.column_id;
GO

/*
    4. Clave primaria e índices.
*/
SELECT
    indice.name AS Indice,
    indice.is_primary_key AS EsClavePrimaria,
    indice.is_unique AS EsUnico,
    columnaIndice.key_ordinal AS OrdenColumna,
    columna.name AS Columna
FROM sys.indexes AS indice
INNER JOIN sys.index_columns AS columnaIndice
    ON columnaIndice.object_id = indice.object_id
    AND columnaIndice.index_id = indice.index_id
INNER JOIN sys.columns AS columna
    ON columna.object_id = columnaIndice.object_id
    AND columna.column_id = columnaIndice.column_id
WHERE
    indice.object_id = OBJECT_ID(N'dbo.Cliente')
    AND
    (
        indice.is_primary_key = 1
        OR indice.name IN
        (
            N'UX_Cliente_Cuit',
            N'IX_Cliente_RazonSocial',
            N'IX_Cliente_Activo'
        )
    )
ORDER BY
    indice.name,
    columnaIndice.key_ordinal;
GO

/*
    5. Restricciones CHECK.
*/
SELECT
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion,
    restriccion.is_disabled AS Deshabilitada,
    restriccion.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS restriccion
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Cliente')
ORDER BY restriccion.name;
GO

/*
    6. Restricción DEFAULT.
*/
SELECT
    restriccion.name AS Restriccion,
    columna.name AS Columna,
    restriccion.definition AS Definicion
FROM sys.default_constraints AS restriccion
INNER JOIN sys.columns AS columna
    ON columna.object_id = restriccion.parent_object_id
    AND columna.column_id = restriccion.parent_column_id
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Cliente');
GO

/*
    7. Cantidades por estado.
*/
SELECT
    COUNT(*) AS CantidadTotal,
    SUM(CASE WHEN Activo = 1 THEN 1 ELSE 0 END)
        AS CantidadActivos,
    SUM(CASE WHEN Activo = 0 THEN 1 ELSE 0 END)
        AS CantidadInactivos
FROM dbo.Cliente;
GO

/*
    8. CUIT duplicados.
    El resultado esperado es cero.
*/
SELECT
    COUNT(*) AS CantidadCuitDuplicados
FROM
(
    SELECT Cuit
    FROM dbo.Cliente
    GROUP BY Cuit
    HAVING COUNT(*) > 1
) AS duplicados;
GO

/*
    9. Permiso funcional del administrador.
*/
SELECT
    COUNT(*) AS CantidadAsignacionesClienteGestionar
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo = grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso = grupoPermiso.IdPermiso
WHERE
    grupo.Codigo = N'ADMINISTRADOR_GENERAL'
    AND permiso.Codigo = N'CLIENTE_GESTIONAR';
GO

/*
    10. Resumen final.
*/
SELECT
    CASE
        WHEN
            EXISTS
            (
                SELECT 1
                FROM dbo.VersionBaseDatos
                WHERE NumeroVersion = N'003'
            )
            AND OBJECT_ID(N'dbo.Cliente', N'U') IS NOT NULL
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id = OBJECT_ID(N'dbo.Cliente')
                    AND name = N'UX_Cliente_Cuit'
                    AND is_unique = 1
            )
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id = OBJECT_ID(N'dbo.Cliente')
                    AND name = N'IX_Cliente_RazonSocial'
            )
            AND EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE
                    object_id = OBJECT_ID(N'dbo.Cliente')
                    AND name = N'IX_Cliente_Activo'
            )
            AND NOT EXISTS
            (
                SELECT Cuit
                FROM dbo.Cliente
                GROUP BY Cuit
                HAVING COUNT(*) > 1
            )
            AND EXISTS
            (
                SELECT 1
                FROM dbo.GrupoPermiso AS grupoPermiso
                INNER JOIN dbo.Grupo AS grupo
                    ON grupo.IdGrupo = grupoPermiso.IdGrupo
                INNER JOIN dbo.Permiso AS permiso
                    ON permiso.IdPermiso = grupoPermiso.IdPermiso
                WHERE
                    grupo.Codigo = N'ADMINISTRADOR_GENERAL'
                    AND permiso.Codigo = N'CLIENTE_GESTIONAR'
            )
        THEN N'VALIDACIÓN CORRECTA'
        ELSE N'VALIDACIÓN INCOMPLETA'
    END AS ResultadoValidacion;
GO
