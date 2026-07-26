USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
    BEGIN
        THROW 50060,
            'No existe dbo.VersionBaseDatos. Ejecute primero la migración 001.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Viaje', N'U') IS NULL
    BEGIN
        THROW 50061,
            'No existe dbo.Viaje. Ejecute primero la migración 004.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
    BEGIN
        THROW 50062,
            'No existe dbo.Cliente. Ejecute primero la migración 003.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Visita', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Visita
        (
            IdVisita INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Visita
                PRIMARY KEY,

            IdViaje INT NOT NULL,

            Fecha DATE NOT NULL,

            Observacion NVARCHAR(1000) NOT NULL,

            LocalidadEncuentro NVARCHAR(150) NOT NULL,

            CONSTRAINT FK_Visita_Viaje
                FOREIGN KEY
                (
                    IdViaje
                )
                REFERENCES dbo.Viaje
                (
                    IdViaje
                ),

            CONSTRAINT CK_Visita_Observacion_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                Observacion
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Visita_LocalidadEncuentro_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                LocalidadEncuentro
                            )
                        )
                    ) > 0
                )
        );
    END;

    IF OBJECT_ID(N'dbo.VisitaCliente', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.VisitaCliente
        (
            IdVisita INT NOT NULL,

            IdCliente INT NOT NULL,

            CONSTRAINT PK_VisitaCliente
                PRIMARY KEY
                (
                    IdVisita,
                    IdCliente
                ),

            CONSTRAINT FK_VisitaCliente_Visita
                FOREIGN KEY
                (
                    IdVisita
                )
                REFERENCES dbo.Visita
                (
                    IdVisita
                ),

            CONSTRAINT FK_VisitaCliente_Cliente
                FOREIGN KEY
                (
                    IdCliente
                )
                REFERENCES dbo.Cliente
                (
                    IdCliente
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Visita')
            AND name =
                N'IX_Visita_IdViaje_Fecha'
    )
    BEGIN
        CREATE INDEX IX_Visita_IdViaje_Fecha
            ON dbo.Visita
            (
                IdViaje,
                Fecha
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Visita')
            AND name =
                N'IX_Visita_Fecha'
    )
    BEGIN
        CREATE INDEX IX_Visita_Fecha
            ON dbo.Visita
            (
                Fecha
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.VisitaCliente')
            AND name =
                N'IX_VisitaCliente_IdCliente'
    )
    BEGIN
        CREATE INDEX IX_VisitaCliente_IdCliente
            ON dbo.VisitaCliente
            (
                IdCliente
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'005'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'005',
            N'Creación del modelo relacional de visitas comerciales'
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Migración 005 aplicada correctamente.';
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

    THROW
        @NumeroError,
        @MensajeError,
        @EstadoError;
END CATCH;
GO

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'005';
GO
