USE master;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @BackupFile NVARCHAR(4000) =
    N'REEMPLAZAR_CON_LA_RUTA_COMPLETA_DEL_ARCHIVO_BAK';

DECLARE @SourceDatabase SYSNAME =
    N'SIGEVIP';

DECLARE @TargetDatabase SYSNAME =
    N'SIGEVIP_RESTORE_TEST';

IF @BackupFile =
    N'REEMPLAZAR_CON_LA_RUTA_COMPLETA_DEL_ARCHIVO_BAK'
BEGIN
    THROW 51100,
        'Debe indicar la ruta completa del archivo .bak.',
        1;
END;

IF DB_ID(@SourceDatabase) IS NULL
BEGIN
    THROW 51101,
        'La base SIGEVIP de origen no existe.',
        1;
END;

IF DB_ID(@TargetDatabase) IS NOT NULL
BEGIN
    THROW 51102,
        'La base SIGEVIP_RESTORE_TEST ya existe. Elimínela o cambie el nombre de destino.',
        1;
END;

DECLARE @CantidadDatos INT;
DECLARE @CantidadLogs INT;

SELECT
    @CantidadDatos =
        COUNT(*)
FROM sys.master_files
WHERE
    database_id =
        DB_ID(@SourceDatabase)
    AND type = 0;

SELECT
    @CantidadLogs =
        COUNT(*)
FROM sys.master_files
WHERE
    database_id =
        DB_ID(@SourceDatabase)
    AND type = 1;

IF @CantidadDatos <> 1
   OR @CantidadLogs <> 1
BEGIN
    THROW 51103,
        'El script automático requiere una base con un archivo de datos y uno de log.',
        1;
END;

DECLARE @LogicalDataName SYSNAME;
DECLARE @LogicalLogName SYSNAME;

SELECT
    @LogicalDataName =
        name
FROM sys.master_files
WHERE
    database_id =
        DB_ID(@SourceDatabase)
    AND type = 0;

SELECT
    @LogicalLogName =
        name
FROM sys.master_files
WHERE
    database_id =
        DB_ID(@SourceDatabase)
    AND type = 1;

DECLARE @DataPath NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultDataPath')
    );

DECLARE @LogPath NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultLogPath')
    );

IF NULLIF(
    LTRIM(
        RTRIM(
            @DataPath
        )
    ),
    N''
) IS NULL
   OR NULLIF(
        LTRIM(
            RTRIM(
                @LogPath
            )
        ),
        N''
   ) IS NULL
BEGIN
    THROW 51104,
        'SQL Server no informó las rutas predeterminadas de datos o log.',
        1;
END;

IF RIGHT(
    @DataPath,
    1
) NOT IN
(
    N'\',
    N'/'
)
BEGIN
    SET @DataPath =
        @DataPath + N'\';
END;

IF RIGHT(
    @LogPath,
    1
) NOT IN
(
    N'\',
    N'/'
)
BEGIN
    SET @LogPath =
        @LogPath + N'\';
END;

DECLARE @TargetDataFile NVARCHAR(4000) =
    @DataPath
    + @TargetDatabase
    + N'.mdf';

DECLARE @TargetLogFile NVARCHAR(4000) =
    @LogPath
    + @TargetDatabase
    + N'_log.ldf';

RESTORE VERIFYONLY
FROM DISK = @BackupFile
WITH CHECKSUM;

DECLARE @RestoreSql NVARCHAR(MAX) =
    N'RESTORE DATABASE '
    + QUOTENAME(@TargetDatabase)
    + N' FROM DISK = N'''
    + REPLACE(
        @BackupFile,
        N'''',
        N''''''
    )
    + N''' WITH '
    + N'MOVE N'''
    + REPLACE(
        @LogicalDataName,
        N'''',
        N''''''
    )
    + N''' TO N'''
    + REPLACE(
        @TargetDataFile,
        N'''',
        N''''''
    )
    + N''', '
    + N'MOVE N'''
    + REPLACE(
        @LogicalLogName,
        N'''',
        N''''''
    )
    + N''' TO N'''
    + REPLACE(
        @TargetLogFile,
        N'''',
        N''''''
    )
    + N''', '
    + N'RECOVERY, CHECKSUM, STATS = 10;';

EXEC sys.sp_executesql
    @RestoreSql;

DECLARE @CheckDbSql NVARCHAR(MAX) =
    N'DBCC CHECKDB ('
    + QUOTENAME(@TargetDatabase)
    + N') WITH NO_INFOMSGS;';

EXEC sys.sp_executesql
    @CheckDbSql;

DECLARE @ValidationSql NVARCHAR(MAX) =
    N'USE '
    + QUOTENAME(@TargetDatabase)
    + N';

    SELECT
        DB_NAME() AS BaseRestaurada,
        MAX(
            TRY_CONVERT(
                INT,
                NumeroVersion
            )
        ) AS UltimaVersion,
        COUNT(*) AS CantidadVersiones
    FROM dbo.VersionBaseDatos;

    SELECT
        tabla.name AS Tabla
    FROM sys.tables AS tabla
    WHERE tabla.name IN
    (
        N''Persona'',
        N''Usuario'',
        N''Grupo'',
        N''Permiso'',
        N''Cliente'',
        N''Viaje'',
        N''Visita'',
        N''Viatico'',
        N''Comprobante'',
        N''Auditoria''
    )
    ORDER BY
        tabla.name;

    SELECT
        N''RESTAURACION Y VALIDACION CORRECTAS''
            AS Resultado;';

EXEC sys.sp_executesql
    @ValidationSql;
GO
