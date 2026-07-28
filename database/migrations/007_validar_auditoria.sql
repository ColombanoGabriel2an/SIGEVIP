USE SIGEVIP;
GO

SET NOCOUNT ON;
GO

SELECT
    @@SERVERNAME AS Servidor,
    DB_NAME() AS BaseActual;
GO

IF OBJECT_ID(
    N'dbo.Auditoria',
    N'U'
) IS NULL
BEGIN
    THROW 50130,
        'La tabla dbo.Auditoria no existe.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.VersionBaseDatos
    WHERE NumeroVersion = N'007'
)
BEGIN
    THROW 50131,
        'La version 007 no se encuentra registrada.',
        1;
END;
GO

/*
    1. Version registrada.
*/

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'007';
GO

/*
    2. Columnas.
*/

SELECT
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,

    CASE
        WHEN tipo.name IN
        (
            N'nvarchar',
            N'nchar',
            N'varchar',
            N'char'
        )
        THEN
            CASE
                WHEN tipo.name IN
                (
                    N'nvarchar',
                    N'nchar'
                )
                THEN columna.max_length / 2
                ELSE columna.max_length
            END
        ELSE columna.max_length
    END AS Longitud,

    columna.precision AS Precision,
    columna.scale AS Escala,
    columna.is_nullable AS PermiteNulos,
    columna.is_identity AS EsIdentity
FROM sys.columns AS columna
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id =
        columna.user_type_id
WHERE columna.object_id =
    OBJECT_ID(
        N'dbo.Auditoria'
    )
ORDER BY columna.column_id;
GO

IF
(
    SELECT COUNT(*)
    FROM sys.columns
    WHERE object_id =
        OBJECT_ID(
            N'dbo.Auditoria'
        )
      AND name IN
      (
          N'IdAuditoria',
          N'FechaHora',
          N'IdUsuario',
          N'NombreUsuario',
          N'Modulo',
          N'Accion',
          N'Entidad',
          N'IdEntidad',
          N'Descripcion'
      )
) <> 9
BEGIN
    THROW 50132,
        'La tabla dbo.Auditoria no contiene todas las columnas esperadas.',
        1;
END;
GO

/*
    3. Clave primaria.
*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND name =
            N'PK_Auditoria'
        AND is_primary_key = 1
)
BEGIN
    THROW 50133,
        'No existe la clave primaria PK_Auditoria.',
        1;
END;
GO

/*
    4. Clave foranea.
*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE
        parent_object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND name =
            N'FK_Auditoria_Usuario'
        AND delete_referential_action_desc =
            N'NO_ACTION'
)
BEGIN
    THROW 50134,
        'No existe la clave foranea FK_Auditoria_Usuario con NO_ACTION.',
        1;
END;
GO

SELECT
    clave.name AS ClaveForanea,
    clave.delete_referential_action_desc
        AS AccionEliminacion,
    clave.update_referential_action_desc
        AS AccionActualizacion,
    clave.is_disabled AS Deshabilitada,
    clave.is_not_trusted AS NoConfiable
FROM sys.foreign_keys AS clave
WHERE clave.parent_object_id =
    OBJECT_ID(
        N'dbo.Auditoria'
    );
GO

/*
    5. Indices.
*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND name =
            N'IX_Auditoria_FechaHora'
)
BEGIN
    THROW 50135,
        'No existe IX_Auditoria_FechaHora.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND name =
            N'IX_Auditoria_IdUsuario_FechaHora'
)
BEGIN
    THROW 50136,
        'No existe IX_Auditoria_IdUsuario_FechaHora.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND name =
            N'IX_Auditoria_Modulo_Accion_FechaHora'
)
BEGIN
    THROW 50137,
        'No existe IX_Auditoria_Modulo_Accion_FechaHora.',
        1;
END;
GO

SELECT
    indice.name AS Indice,
    indice.is_primary_key AS EsClavePrimaria,
    indice.is_unique AS EsUnico,
    columnaIndice.key_ordinal AS Orden,
    columna.name AS Columna,
    columnaIndice.is_descending_key
        AS OrdenDescendente
FROM sys.indexes AS indice
INNER JOIN sys.index_columns
    AS columnaIndice
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
    indice.object_id =
        OBJECT_ID(
            N'dbo.Auditoria'
        )
    AND indice.name IN
    (
        N'PK_Auditoria',
        N'IX_Auditoria_FechaHora',
        N'IX_Auditoria_IdUsuario_FechaHora',
        N'IX_Auditoria_Modulo_Accion_FechaHora'
    )
ORDER BY
    indice.name,
    columnaIndice.key_ordinal;
GO

/*
    6. Restricciones CHECK.
*/

DECLARE @CantidadChecksEsperados INT = 6;

IF
(
    SELECT COUNT(*)
    FROM sys.check_constraints
    WHERE parent_object_id =
        OBJECT_ID(
            N'dbo.Auditoria'
        )
      AND name IN
      (
          N'CK_Auditoria_NombreUsuario_NoVacio',
          N'CK_Auditoria_Modulo_NoVacio',
          N'CK_Auditoria_Accion_NoVacia',
          N'CK_Auditoria_Entidad_NoVacia',
          N'CK_Auditoria_Descripcion_NoVacia',
          N'CK_Auditoria_IdEntidad_Valido'
      )
) <> @CantidadChecksEsperados
BEGIN
    THROW 50138,
        'No se encontraron todas las restricciones CHECK de Auditoria.',
        1;
END;
GO

SELECT
    name AS Restriccion,
    definition AS Definicion,
    is_disabled AS Deshabilitada,
    is_not_trusted AS NoConfiable
FROM sys.check_constraints
WHERE parent_object_id =
    OBJECT_ID(
        N'dbo.Auditoria'
    )
ORDER BY name;
GO

/*
    7. Default de fecha.
*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints AS defecto
    INNER JOIN sys.columns AS columna
        ON columna.object_id =
            defecto.parent_object_id
        AND columna.column_id =
            defecto.parent_column_id
    WHERE
        defecto.parent_object_id =
            OBJECT_ID(
                N'dbo.Auditoria'
            )
        AND defecto.name =
            N'DF_Auditoria_FechaHora'
        AND columna.name =
            N'FechaHora'
)
BEGIN
    THROW 50139,
        'No existe DF_Auditoria_FechaHora.',
        1;
END;
GO

/*
    8. Registros sin Usuario.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadAuditoriasSinUsuario
FROM dbo.Auditoria AS auditoria
LEFT JOIN dbo.Usuario AS usuario
    ON usuario.IdUsuario =
        auditoria.IdUsuario
WHERE usuario.IdUsuario IS NULL;
GO

/*
    9. Registros con textos vacios.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadAuditoriasConTextoInvalido
FROM dbo.Auditoria
WHERE
    LEN(
        LTRIM(
            RTRIM(
                NombreUsuario
            )
        )
    ) = 0
    OR LEN(
        LTRIM(
            RTRIM(
                Modulo
            )
        )
    ) = 0
    OR LEN(
        LTRIM(
            RTRIM(
                Accion
            )
        )
    ) = 0
    OR LEN(
        LTRIM(
            RTRIM(
                Entidad
            )
        )
    ) = 0
    OR LEN(
        LTRIM(
            RTRIM(
                Descripcion
            )
        )
    ) = 0;
GO

/*
    10. Identificadores de entidad invalidos.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadIdsEntidadInvalidos
FROM dbo.Auditoria
WHERE
    IdEntidad IS NOT NULL
    AND IdEntidad <= 0;
GO

SELECT
    N'VALIDACION CORRECTA'
        AS Resultado;
GO