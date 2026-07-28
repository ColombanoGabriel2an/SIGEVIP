USE SIGEVIP;
GO

SET NOCOUNT ON;
GO

SELECT
    @@SERVERNAME AS Servidor,
    DB_NAME() AS BaseActual;
GO

IF OBJECT_ID(N'dbo.AuditoriaCambio', N'U') IS NULL
BEGIN
    THROW 50150,
        'La tabla dbo.AuditoriaCambio no existe.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.VersionBaseDatos
    WHERE NumeroVersion = N'008'
)
BEGIN
    THROW 50151,
        'La version 008 no se encuentra registrada.',
        1;
END;
GO

IF
(
    SELECT COUNT(*)
    FROM sys.columns
    WHERE object_id =
        OBJECT_ID(N'dbo.AuditoriaCambio')
      AND name IN
      (
          N'IdAuditoriaCambio',
          N'IdAuditoria',
          N'Campo',
          N'ValorAnterior',
          N'ValorNuevo'
      )
) <> 5
BEGIN
    THROW 50152,
        'La tabla dbo.AuditoriaCambio no contiene todas las columnas esperadas.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE
        parent_object_id =
            OBJECT_ID(N'dbo.AuditoriaCambio')
        AND name =
            N'FK_AuditoriaCambio_Auditoria'
        AND delete_referential_action_desc =
            N'CASCADE'
)
BEGIN
    THROW 50153,
        'No existe la clave foranea esperada con eliminacion en cascada.',
        1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id =
            OBJECT_ID(N'dbo.AuditoriaCambio')
        AND name =
            N'UQ_AuditoriaCambio_IdAuditoria_Campo'
        AND is_unique = 1
)
BEGIN
    THROW 50154,
        'No existe la restriccion unica esperada.',
        1;
END;
GO

IF
(
    SELECT COUNT(*)
    FROM sys.check_constraints
    WHERE parent_object_id =
        OBJECT_ID(N'dbo.AuditoriaCambio')
      AND name IN
      (
          N'CK_AuditoriaCambio_Campo_NoVacio',
          N'CK_AuditoriaCambio_ValoresDiferentes'
      )
) <> 2
BEGIN
    THROW 50155,
        'No se encontraron todas las restricciones CHECK esperadas.',
        1;
END;
GO

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'008';
GO

SELECT
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,
    columna.max_length AS LongitudBytes,
    columna.is_nullable AS PermiteNulos,
    columna.is_identity AS EsIdentity
FROM sys.columns AS columna
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id =
        columna.user_type_id
WHERE columna.object_id =
    OBJECT_ID(N'dbo.AuditoriaCambio')
ORDER BY columna.column_id;
GO

SELECT
    clave.name AS ClaveForanea,
    clave.delete_referential_action_desc
        AS AccionEliminacion,
    clave.is_disabled AS Deshabilitada,
    clave.is_not_trusted AS NoConfiable
FROM sys.foreign_keys AS clave
WHERE clave.parent_object_id =
    OBJECT_ID(N'dbo.AuditoriaCambio');
GO

SELECT
    name AS Restriccion,
    definition AS Definicion,
    is_disabled AS Deshabilitada,
    is_not_trusted AS NoConfiable
FROM sys.check_constraints
WHERE parent_object_id =
    OBJECT_ID(N'dbo.AuditoriaCambio')
ORDER BY name;
GO

SELECT
    COUNT(*) AS CambiosHuerfanos
FROM dbo.AuditoriaCambio AS cambio
LEFT JOIN dbo.Auditoria AS auditoria
    ON auditoria.IdAuditoria =
        cambio.IdAuditoria
WHERE auditoria.IdAuditoria IS NULL;
GO
