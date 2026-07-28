USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
    BEGIN
        THROW 50140,
            'No existe dbo.VersionBaseDatos. Ejecute primero las migraciones anteriores.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NULL
    BEGIN
        THROW 50141,
            'No existe dbo.Auditoria. Ejecute primero la migracion 007.',
            1;
    END;

    IF OBJECT_ID(N'dbo.AuditoriaCambio', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AuditoriaCambio
        (
            IdAuditoriaCambio BIGINT IDENTITY(1,1)
                NOT NULL
                CONSTRAINT PK_AuditoriaCambio
                PRIMARY KEY,

            IdAuditoria BIGINT
                NOT NULL,

            Campo NVARCHAR(100)
                NOT NULL,

            ValorAnterior NVARCHAR(MAX)
                NULL,

            ValorNuevo NVARCHAR(MAX)
                NULL,

            CONSTRAINT FK_AuditoriaCambio_Auditoria
                FOREIGN KEY (IdAuditoria)
                REFERENCES dbo.Auditoria(IdAuditoria)
                ON DELETE CASCADE,

            CONSTRAINT UQ_AuditoriaCambio_IdAuditoria_Campo
                UNIQUE (IdAuditoria, Campo),

            CONSTRAINT CK_AuditoriaCambio_Campo_NoVacio
                CHECK
                (
                    LEN(LTRIM(RTRIM(Campo))) > 0
                ),

            CONSTRAINT CK_AuditoriaCambio_ValoresDiferentes
                CHECK
                (
                    (
                        ValorAnterior IS NULL
                        AND ValorNuevo IS NOT NULL
                    )
                    OR
                    (
                        ValorAnterior IS NOT NULL
                        AND ValorNuevo IS NULL
                    )
                    OR
                    (
                        ValorAnterior IS NOT NULL
                        AND ValorNuevo IS NOT NULL
                        AND ValorAnterior <> ValorNuevo
                    )
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'008'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'008',
            N'Agrega el detalle de valores anteriores y nuevos de la Auditoria.'
        );
    END;

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
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'008';
GO
