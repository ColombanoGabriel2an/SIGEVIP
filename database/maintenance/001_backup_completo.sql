USE master;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @DatabaseName SYSNAME =
    N'SIGEVIP';

IF DB_ID(@DatabaseName) IS NULL
BEGIN
    THROW 51000,
        'La base de datos SIGEVIP no existe en esta instancia.',
        1;
END;

DECLARE @BackupDirectory NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultBackupPath')
    );

IF NULLIF(
    LTRIM(
        RTRIM(
            @BackupDirectory
        )
    ),
    N''
) IS NULL
BEGIN
    THROW 51001,
        'SQL Server no informó el directorio predeterminado de backup.',
        1;
END;

IF RIGHT(
    @BackupDirectory,
    1
) NOT IN
(
    N'\',
    N'/'
)
BEGIN
    SET @BackupDirectory =
        @BackupDirectory + N'\';
END;

DECLARE @MarcaTemporal NVARCHAR(20) =
    CONVERT(
        CHAR(8),
        SYSDATETIME(),
        112
    )
    + N'_'
    + REPLACE(
        CONVERT(
            CHAR(8),
            SYSDATETIME(),
            108
        ),
        N':',
        N''
    );

DECLARE @BackupFile NVARCHAR(4000) =
    @BackupDirectory
    + N'SIGEVIP_FULL_'
    + @MarcaTemporal
    + N'.bak';

PRINT N'Base: ' + @DatabaseName;
PRINT N'Archivo: ' + @BackupFile;

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

SELECT
    N'BACKUP Y VERIFICACION CORRECTOS'
        AS Resultado,
    @BackupFile
        AS ArchivoGenerado;
GO
