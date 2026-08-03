USE master;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    Recrea SIGEVIP_TEST como copia controlada de SIGEVIP.

    Finalidad:
    - separar la base usada por la aplicacion y la demostracion;
    - impedir que las pruebas integrales consuman los IDENTITY de SIGEVIP;
    - permitir repetir las pruebas sin alterar los datos de demostracion.

    Advertencia:
    - si SIGEVIP_TEST ya existe, se elimina y se vuelve a crear;
    - SIGEVIP no se elimina ni se modifica;
    - el backup intermedio se escribe en la carpeta predeterminada
      de backups de SQL Server.
*/

DECLARE @SourceDatabase SYSNAME = N'SIGEVIP';
DECLARE @TargetDatabase SYSNAME = N'SIGEVIP_TEST';

IF DB_ID(@SourceDatabase) IS NULL
BEGIN
    THROW 51200,
        'No existe la base SIGEVIP.',
        1;
END;

DECLARE @BackupDirectory NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultBackupPath')
    );

DECLARE @DataDirectory NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultDataPath')
    );

DECLARE @LogDirectory NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultLogPath')
    );

IF NULLIF(LTRIM(RTRIM(@BackupDirectory)), N'') IS NULL
   OR NULLIF(LTRIM(RTRIM(@DataDirectory)), N'') IS NULL
   OR NULLIF(LTRIM(RTRIM(@LogDirectory)), N'') IS NULL
BEGIN
    THROW 51201,
        'SQL Server no informo sus rutas predeterminadas.',
        1;
END;

IF RIGHT(@BackupDirectory, 1) NOT IN (N'\', N'/')
BEGIN
    SET @BackupDirectory = @BackupDirectory + N'\';
END;

IF RIGHT(@DataDirectory, 1) NOT IN (N'\', N'/')
BEGIN
    SET @DataDirectory = @DataDirectory + N'\';
END;

IF RIGHT(@LogDirectory, 1) NOT IN (N'\', N'/')
BEGIN
    SET @LogDirectory = @LogDirectory + N'\';
END;

DECLARE @BackupFile NVARCHAR(4000) =
    @BackupDirectory + N'SIGEVIP_TEST_REFRESH.bak';

DECLARE @LogicalDataName SYSNAME;
DECLARE @LogicalLogName SYSNAME;
DECLARE @DataFileCount INT;
DECLARE @LogFileCount INT;

SELECT
    @DataFileCount = COUNT(*)
FROM sys.master_files
WHERE database_id = DB_ID(@SourceDatabase)
  AND type = 0;

SELECT
    @LogFileCount = COUNT(*)
FROM sys.master_files
WHERE database_id = DB_ID(@SourceDatabase)
  AND type = 1;

IF @DataFileCount <> 1 OR @LogFileCount <> 1
BEGIN
    THROW 51202,
        'El script requiere un archivo de datos y uno de log en SIGEVIP.',
        1;
END;

SELECT
    @LogicalDataName = name
FROM sys.master_files
WHERE database_id = DB_ID(@SourceDatabase)
  AND type = 0;

SELECT
    @LogicalLogName = name
FROM sys.master_files
WHERE database_id = DB_ID(@SourceDatabase)
  AND type = 1;

DECLARE @TargetDataFile NVARCHAR(4000) =
    @DataDirectory + @TargetDatabase + N'.mdf';

DECLARE @TargetLogFile NVARCHAR(4000) =
    @LogDirectory + @TargetDatabase + N'_log.ldf';

PRINT N'Creando backup temporal: ' + @BackupFile;

BACKUP DATABASE [SIGEVIP]
TO DISK = @BackupFile
WITH
    COPY_ONLY,
    INIT,
    CHECKSUM,
    STATS = 10;

RESTORE VERIFYONLY
FROM DISK = @BackupFile
WITH CHECKSUM;

IF DB_ID(@TargetDatabase) IS NOT NULL
BEGIN
    PRINT N'Eliminando la copia anterior SIGEVIP_TEST.';

    ALTER DATABASE [SIGEVIP_TEST]
        SET SINGLE_USER
        WITH ROLLBACK IMMEDIATE;

    DROP DATABASE [SIGEVIP_TEST];
END;

DECLARE @RestoreSql NVARCHAR(MAX) =
    N'RESTORE DATABASE '
    + QUOTENAME(@TargetDatabase)
    + N' FROM DISK = N'''
    + REPLACE(@BackupFile, N'''', N'''''')
    + N''' WITH '
    + N'MOVE N'''
    + REPLACE(@LogicalDataName, N'''', N'''''')
    + N''' TO N'''
    + REPLACE(@TargetDataFile, N'''', N'''''')
    + N''', '
    + N'MOVE N'''
    + REPLACE(@LogicalLogName, N'''', N'''''')
    + N''' TO N'''
    + REPLACE(@TargetLogFile, N'''', N'''''')
    + N''', '
    + N'RECOVERY, CHECKSUM, STATS = 10;';

EXEC sys.sp_executesql @RestoreSql;

ALTER DATABASE [SIGEVIP_TEST]
    SET RECOVERY SIMPLE;

ALTER DATABASE [SIGEVIP_TEST]
    SET MULTI_USER;

DBCC CHECKDB ([SIGEVIP_TEST])
WITH NO_INFOMSGS;

DECLARE @ValidationSql NVARCHAR(MAX) =
    N'USE [SIGEVIP_TEST];

    SELECT
        DB_NAME() AS BasePruebas,
        MAX(TRY_CONVERT(INT, NumeroVersion))
            AS UltimaVersion,
        COUNT(*) AS CantidadVersiones
    FROM dbo.VersionBaseDatos;

    SELECT
        COUNT(*) AS CantidadTablasEsperadas
    FROM sys.tables
    WHERE name IN
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
        N''Auditoria'',
        N''AuditoriaCambio''
    );

    SELECT
        N''SIGEVIP_TEST CREADA CORRECTAMENTE''
            AS Resultado;';

EXEC sys.sp_executesql @ValidationSql;
GO
