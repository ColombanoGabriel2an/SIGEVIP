USE SIGEVIP;
GO

SET NOCOUNT ON;
GO

/*
    1. Servidor, base actual y migración 002.
*/
SELECT
    @@SERVERNAME AS Servidor,
    DB_NAME() AS BaseActual;
GO

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'002';
GO

/*
    2. Existencia de las siete tablas de seguridad.
*/
DECLARE @TablasEsperadas TABLE
(
    NombreTabla SYSNAME NOT NULL PRIMARY KEY
);

INSERT INTO @TablasEsperadas
(
    NombreTabla
)
VALUES
    (N'Persona'),
    (N'Usuario'),
    (N'Grupo'),
    (N'Permiso'),
    (N'UsuarioGrupo'),
    (N'GrupoPermiso'),
    (N'GrupoGrupo');

SELECT
    tablaEsperada.NombreTabla,
    CASE
        WHEN tablaReal.object_id IS NOT NULL THEN N'SÍ'
        ELSE N'NO'
    END AS Existe
FROM @TablasEsperadas AS tablaEsperada
LEFT JOIN sys.tables AS tablaReal
    ON tablaReal.name = tablaEsperada.NombreTabla
    AND SCHEMA_NAME(tablaReal.schema_id) = N'dbo'
ORDER BY tablaEsperada.NombreTabla;
GO

/*
    3. Cantidades generales.
*/
SELECT
    (SELECT COUNT(*) FROM dbo.Persona)
        AS CantidadPersonas,

    (SELECT COUNT(*) FROM dbo.Usuario)
        AS CantidadUsuarios,

    (SELECT COUNT(*) FROM dbo.Grupo)
        AS CantidadGrupos,

    (SELECT COUNT(*) FROM dbo.Permiso)
        AS CantidadPermisos,

    (SELECT COUNT(*) FROM dbo.UsuarioGrupo)
        AS CantidadUsuarioGrupo,

    (SELECT COUNT(*) FROM dbo.GrupoPermiso)
        AS CantidadGrupoPermiso,

    (SELECT COUNT(*) FROM dbo.GrupoGrupo)
        AS CantidadGrupoGrupo;
GO

/*
    4. Grupos iniciales.
*/
SELECT
    IdGrupo,
    Codigo,
    Nombre,
    Descripcion,
    Activo
FROM dbo.Grupo
ORDER BY Codigo;
GO

/*
    5. Permisos iniciales.
*/
SELECT
    IdPermiso,
    Codigo,
    Nombre,
    Descripcion,
    Activo
FROM dbo.Permiso
ORDER BY Codigo;
GO

/*
    6. Permisos asignados a cada grupo.
*/
SELECT
    grupo.Codigo AS CodigoGrupo,
    grupo.Nombre AS NombreGrupo,
    permiso.Codigo AS CodigoPermiso,
    permiso.Nombre AS NombrePermiso
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo = grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso = grupoPermiso.IdPermiso
ORDER BY
    grupo.Codigo,
    permiso.Codigo;
GO

/*
    7. Cantidad de permisos por grupo.
*/
SELECT
    grupo.Codigo AS CodigoGrupo,
    COUNT(grupoPermiso.IdPermiso) AS CantidadPermisos
FROM dbo.Grupo AS grupo
LEFT JOIN dbo.GrupoPermiso AS grupoPermiso
    ON grupoPermiso.IdGrupo = grupo.IdGrupo
GROUP BY
    grupo.Codigo
ORDER BY
    grupo.Codigo;
GO

/*
    8. Relaciones entre grupos.
    Puede estar vacío en el seed inicial.
*/
SELECT
    grupoPadre.Codigo AS GrupoPadre,
    grupoHijo.Codigo AS GrupoHijo
FROM dbo.GrupoGrupo AS grupoGrupo
INNER JOIN dbo.Grupo AS grupoPadre
    ON grupoPadre.IdGrupo = grupoGrupo.IdGrupoPadre
INNER JOIN dbo.Grupo AS grupoHijo
    ON grupoHijo.IdGrupo = grupoGrupo.IdGrupoHijo
ORDER BY
    grupoPadre.Codigo,
    grupoHijo.Codigo;
GO

/*
    9. Índices únicos y auxiliares relevantes.
*/
SELECT
    tabla.name AS Tabla,
    indice.name AS Indice,
    indice.is_unique AS EsUnico,
    indice.is_primary_key AS EsClavePrimaria,
    columnaIndice.key_ordinal AS OrdenColumna,
    columna.name AS Columna
FROM sys.indexes AS indice
INNER JOIN sys.tables AS tabla
    ON tabla.object_id = indice.object_id
INNER JOIN sys.index_columns AS columnaIndice
    ON columnaIndice.object_id = indice.object_id
    AND columnaIndice.index_id = indice.index_id
INNER JOIN sys.columns AS columna
    ON columna.object_id = columnaIndice.object_id
    AND columna.column_id = columnaIndice.column_id
WHERE
    SCHEMA_NAME(tabla.schema_id) = N'dbo'
    AND tabla.name IN
    (
        N'Persona',
        N'Usuario',
        N'Grupo',
        N'Permiso',
        N'UsuarioGrupo',
        N'GrupoPermiso',
        N'GrupoGrupo'
    )
    AND
    (
        indice.is_primary_key = 1
        OR indice.name LIKE N'UX[_]%'
        OR indice.name LIKE N'IX[_]%'
    )
ORDER BY
    tabla.name,
    indice.name,
    columnaIndice.key_ordinal;
GO

/*
    10. Claves foráneas.
*/
SELECT
    claveForanea.name AS ClaveForanea,
    tablaOrigen.name AS TablaOrigen,
    columnaOrigen.name AS ColumnaOrigen,
    tablaDestino.name AS TablaDestino,
    columnaDestino.name AS ColumnaDestino,
    claveForanea.delete_referential_action_desc
        AS AccionAlEliminar,
    claveForanea.update_referential_action_desc
        AS AccionAlActualizar
FROM sys.foreign_keys AS claveForanea
INNER JOIN sys.foreign_key_columns AS columnaClaveForanea
    ON columnaClaveForanea.constraint_object_id =
        claveForanea.object_id
INNER JOIN sys.tables AS tablaOrigen
    ON tablaOrigen.object_id =
        columnaClaveForanea.parent_object_id
INNER JOIN sys.columns AS columnaOrigen
    ON columnaOrigen.object_id =
        columnaClaveForanea.parent_object_id
    AND columnaOrigen.column_id =
        columnaClaveForanea.parent_column_id
INNER JOIN sys.tables AS tablaDestino
    ON tablaDestino.object_id =
        columnaClaveForanea.referenced_object_id
INNER JOIN sys.columns AS columnaDestino
    ON columnaDestino.object_id =
        columnaClaveForanea.referenced_object_id
    AND columnaDestino.column_id =
        columnaClaveForanea.referenced_column_id
WHERE
    SCHEMA_NAME(tablaOrigen.schema_id) = N'dbo'
    AND tablaOrigen.name IN
    (
        N'Usuario',
        N'UsuarioGrupo',
        N'GrupoPermiso',
        N'GrupoGrupo'
    )
ORDER BY
    tablaOrigen.name,
    claveForanea.name,
    columnaClaveForanea.constraint_column_id;
GO

/*
    11. Restricciones CHECK.
*/
SELECT
    tabla.name AS Tabla,
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion,
    restriccion.is_disabled AS Deshabilitada,
    restriccion.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS restriccion
INNER JOIN sys.tables AS tabla
    ON tabla.object_id = restriccion.parent_object_id
WHERE
    SCHEMA_NAME(tabla.schema_id) = N'dbo'
    AND tabla.name IN
    (
        N'Persona',
        N'Usuario',
        N'Grupo',
        N'Permiso',
        N'GrupoGrupo'
    )
ORDER BY
    tabla.name,
    restriccion.name;
GO

/*
    12. Restricciones DEFAULT.
*/
SELECT
    tabla.name AS Tabla,
    columna.name AS Columna,
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion
FROM sys.default_constraints AS restriccion
INNER JOIN sys.tables AS tabla
    ON tabla.object_id = restriccion.parent_object_id
INNER JOIN sys.columns AS columna
    ON columna.object_id = restriccion.parent_object_id
    AND columna.column_id = restriccion.parent_column_id
WHERE
    SCHEMA_NAME(tabla.schema_id) = N'dbo'
    AND tabla.name IN
    (
        N'Persona',
        N'Usuario',
        N'Grupo',
        N'Permiso'
    )
ORDER BY
    tabla.name,
    columna.name;
GO

/*
    13. Verificación de duplicados en catálogos y asociaciones.
    Todos los resultados esperados son cero.
*/
SELECT
    N'Grupo.Codigo' AS Validacion,
    COUNT(*) AS CantidadDuplicados
FROM
(
    SELECT Codigo
    FROM dbo.Grupo
    GROUP BY Codigo
    HAVING COUNT(*) > 1
) AS duplicadosGrupo

UNION ALL

SELECT
    N'Permiso.Codigo',
    COUNT(*)
FROM
(
    SELECT Codigo
    FROM dbo.Permiso
    GROUP BY Codigo
    HAVING COUNT(*) > 1
) AS duplicadosPermiso

UNION ALL

SELECT
    N'Usuario.NombreUsuario',
    COUNT(*)
FROM
(
    SELECT NombreUsuario
    FROM dbo.Usuario
    GROUP BY NombreUsuario
    HAVING COUNT(*) > 1
) AS duplicadosUsuario

UNION ALL

SELECT
    N'Usuario.IdPersona',
    COUNT(*)
FROM
(
    SELECT IdPersona
    FROM dbo.Usuario
    GROUP BY IdPersona
    HAVING COUNT(*) > 1
) AS duplicadosPersonaUsuario

UNION ALL

SELECT
    N'UsuarioGrupo',
    COUNT(*)
FROM
(
    SELECT
        IdUsuario,
        IdGrupo
    FROM dbo.UsuarioGrupo
    GROUP BY
        IdUsuario,
        IdGrupo
    HAVING COUNT(*) > 1
) AS duplicadosUsuarioGrupo

UNION ALL

SELECT
    N'GrupoPermiso',
    COUNT(*)
FROM
(
    SELECT
        IdGrupo,
        IdPermiso
    FROM dbo.GrupoPermiso
    GROUP BY
        IdGrupo,
        IdPermiso
    HAVING COUNT(*) > 1
) AS duplicadosGrupoPermiso

UNION ALL

SELECT
    N'GrupoGrupo',
    COUNT(*)
FROM
(
    SELECT
        IdGrupoPadre,
        IdGrupoHijo
    FROM dbo.GrupoGrupo
    GROUP BY
        IdGrupoPadre,
        IdGrupoHijo
    HAVING COUNT(*) > 1
) AS duplicadosGrupoGrupo;
GO

/*
    14. Resumen final esperado para el seed inicial.
*/
SELECT
    CASE
        WHEN
            (SELECT COUNT(*) FROM dbo.Grupo) = 4
            AND (SELECT COUNT(*) FROM dbo.Permiso) = 17
            AND (SELECT COUNT(*) FROM dbo.GrupoPermiso) = 22
            AND (SELECT COUNT(*) FROM dbo.GrupoGrupo) = 0
            AND (SELECT COUNT(*) FROM dbo.Usuario) = 0
            AND (SELECT COUNT(*) FROM dbo.UsuarioGrupo) = 0
        THEN N'VALIDACIÓN CORRECTA'
        ELSE N'REVISAR CANTIDADES'
    END AS ResultadoValidacion;
GO
