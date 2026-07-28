USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(
        N'dbo.AuditoriaCambio',
        N'U'
    ) IS NOT NULL
    BEGIN
        DROP TABLE dbo.AuditoriaCambio;
    END;

    DELETE FROM dbo.VersionBaseDatos
    WHERE NumeroVersion = N'008';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO

SELECT
    OBJECT_ID(
        N'dbo.AuditoriaCambio',
        N'U'
    ) AS IdTablaAuditoriaCambio;

SELECT
    COUNT(*) AS Versiones008
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'008';
GO