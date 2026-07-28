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

    IF OBJECT_ID(
        N'dbo.VersionBaseDatos',
        N'U'
    ) IS NULL
    BEGIN
        THROW 50120,
            'No existe dbo.VersionBaseDatos. Ejecute primero las migraciones anteriores.',
            1;
    END;

    IF OBJECT_ID(
        N'dbo.Usuario',
        N'U'
    ) IS NULL
    BEGIN
        THROW 50121,
            'No existe dbo.Usuario. Ejecute primero la migracion de seguridad.',
            1;
    END;

    IF OBJECT_ID(
        N'dbo.Auditoria',
        N'U'
    ) IS NULL
    BEGIN
        CREATE TABLE dbo.Auditoria
        (
            IdAuditoria BIGINT IDENTITY(1,1)
                NOT NULL
                CONSTRAINT PK_Auditoria
                PRIMARY KEY,

            FechaHora DATETIME2(0)
                NOT NULL
                CONSTRAINT DF_Auditoria_FechaHora
                DEFAULT (SYSDATETIME()),

            IdUsuario INT
                NOT NULL,

            NombreUsuario NVARCHAR(100)
                NOT NULL,

            Modulo NVARCHAR(50)
                NOT NULL,

            Accion NVARCHAR(50)
                NOT NULL,

            Entidad NVARCHAR(100)
                NOT NULL,

            IdEntidad INT
                NULL,

            Descripcion NVARCHAR(1000)
                NOT NULL,

            CONSTRAINT FK_Auditoria_Usuario
                FOREIGN KEY (IdUsuario)
                REFERENCES dbo.Usuario
                (
                    IdUsuario
                ),

            CONSTRAINT CK_Auditoria_NombreUsuario_NoVacio
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                NombreUsuario
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Auditoria_Modulo_NoVacio
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                Modulo
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Auditoria_Accion_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                Accion
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Auditoria_Entidad_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                Entidad
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Auditoria_Descripcion_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                Descripcion
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Auditoria_IdEntidad_Valido
                CHECK
                (
                    IdEntidad IS NULL
                    OR IdEntidad > 0
                )
        );
    END;

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
        CREATE INDEX IX_Auditoria_FechaHora
            ON dbo.Auditoria
            (
                FechaHora DESC,
                IdAuditoria DESC
            );
    END;

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
        CREATE INDEX IX_Auditoria_IdUsuario_FechaHora
            ON dbo.Auditoria
            (
                IdUsuario,
                FechaHora DESC
            );
    END;

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
        CREATE INDEX IX_Auditoria_Modulo_Accion_FechaHora
            ON dbo.Auditoria
            (
                Modulo,
                Accion,
                FechaHora DESC
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'007'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'007',
            N'Crea la auditoria general consultable del sistema.'
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
WHERE NumeroVersion = N'007';
GO