USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
    BEGIN
        THROW 50020,
            'No existe dbo.VersionBaseDatos. Ejecute primero la migración 001.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Cliente
        (
            IdCliente INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Cliente
                PRIMARY KEY,

            RazonSocial NVARCHAR(150) NOT NULL,

            Cuit NVARCHAR(20) NOT NULL,

            Email NVARCHAR(150) NULL,

            Telefono NVARCHAR(50) NULL,

            Localidad NVARCHAR(100) NULL,

            Provincia NVARCHAR(100) NULL,

            Activo BIT NOT NULL
                CONSTRAINT DF_Cliente_Activo
                DEFAULT (1),

            CONSTRAINT CK_Cliente_RazonSocial_NoVacia
                CHECK
                (
                    LEN(LTRIM(RTRIM(RazonSocial))) > 0
                ),

            CONSTRAINT CK_Cliente_Cuit_NoVacio
                CHECK
                (
                    LEN(LTRIM(RTRIM(Cuit))) > 0
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'UX_Cliente_Cuit'
            AND object_id = OBJECT_ID(N'dbo.Cliente')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Cliente_Cuit
            ON dbo.Cliente (Cuit);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'IX_Cliente_RazonSocial'
            AND object_id = OBJECT_ID(N'dbo.Cliente')
    )
    BEGIN
        CREATE INDEX IX_Cliente_RazonSocial
            ON dbo.Cliente (RazonSocial);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'IX_Cliente_Activo'
            AND object_id = OBJECT_ID(N'dbo.Cliente')
    )
    BEGIN
        CREATE INDEX IX_Cliente_Activo
            ON dbo.Cliente (Activo);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'003'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'003',
            N'Creación del modelo relacional de clientes'
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Migración 003 aplicada correctamente.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    DECLARE @MensajeError NVARCHAR(4000) =
        ERROR_MESSAGE();

    DECLARE @NumeroError INT =
        ERROR_NUMBER();

    DECLARE @EstadoError INT =
        ERROR_STATE();

    THROW @NumeroError, @MensajeError, @EstadoError;
END CATCH;
GO

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'003';
GO
