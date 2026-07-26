USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
    BEGIN
        THROW 50040,
            'No existe dbo.VersionBaseDatos. Ejecute primero la migración 001.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Persona', N'U') IS NULL
    BEGIN
        THROW 50041,
            'No existe dbo.Persona. Ejecute primero la migración 002.',
            1;
    END;

    /*
        Correspondencia de enums:

        TipoViaje
        1 = Desplazamiento
        2 = EnOficina
        3 = EventoFeria

        EstadoViaje
        1 = Abierto
        2 = EnRendicion
        3 = Aprobado
        4 = Cancelado
    */

    IF OBJECT_ID(N'dbo.Viaje', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Viaje
        (
            IdViaje INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Viaje
                PRIMARY KEY,

            FechaInicio DATE NOT NULL,

            FechaFin DATE NOT NULL,

            Descripcion NVARCHAR(500) NOT NULL,

            TipoViaje TINYINT NOT NULL,

            MontoAnticipado DECIMAL(18,2) NOT NULL
                CONSTRAINT DF_Viaje_MontoAnticipado
                DEFAULT (0),

            EstadoViaje TINYINT NOT NULL
                CONSTRAINT DF_Viaje_EstadoViaje
                DEFAULT (1),

            CONSTRAINT CK_Viaje_Descripcion_NoVacia
                CHECK
                (
                    LEN(LTRIM(RTRIM(Descripcion))) > 0
                ),

            CONSTRAINT CK_Viaje_Periodo
                CHECK
                (
                    FechaInicio <= FechaFin
                ),

            CONSTRAINT CK_Viaje_TipoViaje
                CHECK
                (
                    TipoViaje IN (1, 2, 3)
                ),

            CONSTRAINT CK_Viaje_MontoAnticipado
                CHECK
                (
                    MontoAnticipado >= 0
                ),

            CONSTRAINT CK_Viaje_EstadoViaje
                CHECK
                (
                    EstadoViaje IN (1, 2, 3, 4)
                )
        );
    END;

    IF OBJECT_ID(N'dbo.ViajeParticipante', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ViajeParticipante
        (
            IdViaje INT NOT NULL,

            IdPersona INT NOT NULL,

            CONSTRAINT PK_ViajeParticipante
                PRIMARY KEY
                (
                    IdViaje,
                    IdPersona
                ),

            CONSTRAINT FK_ViajeParticipante_Viaje
                FOREIGN KEY
                (
                    IdViaje
                )
                REFERENCES dbo.Viaje
                (
                    IdViaje
                ),

            CONSTRAINT FK_ViajeParticipante_Persona
                FOREIGN KEY
                (
                    IdPersona
                )
                REFERENCES dbo.Persona
                (
                    IdPersona
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id = OBJECT_ID(N'dbo.Viaje')
            AND name = N'IX_Viaje_FechaInicio'
    )
    BEGIN
        CREATE INDEX IX_Viaje_FechaInicio
            ON dbo.Viaje
            (
                FechaInicio
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id = OBJECT_ID(N'dbo.Viaje')
            AND name = N'IX_Viaje_EstadoViaje'
    )
    BEGIN
        CREATE INDEX IX_Viaje_EstadoViaje
            ON dbo.Viaje
            (
                EstadoViaje
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.ViajeParticipante')
            AND name =
                N'IX_ViajeParticipante_IdPersona'
    )
    BEGIN
        CREATE INDEX IX_ViajeParticipante_IdPersona
            ON dbo.ViajeParticipante
            (
                IdPersona
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'004'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'004',
            N'Creación del modelo relacional de viajes y participantes'
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Migración 004 aplicada correctamente.';
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
WHERE NumeroVersion = N'004';
GO
