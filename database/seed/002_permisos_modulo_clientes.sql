USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Grupo', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Permiso', N'U') IS NULL
       OR OBJECT_ID(N'dbo.GrupoPermiso', N'U') IS NULL
    BEGIN
        THROW 50030,
            'No existe el esquema de seguridad. Ejecute primero la migración 002.',
            1;
    END;

    DECLARE @IdGrupo INT;
    DECLARE @IdPermiso INT;

    SELECT
        @IdGrupo = IdGrupo
    FROM dbo.Grupo
    WHERE Codigo = N'ADMINISTRADOR_GENERAL';

    IF @IdGrupo IS NULL
    BEGIN
        THROW 50031,
            'No existe el grupo ADMINISTRADOR_GENERAL.',
            1;
    END;

    SELECT
        @IdPermiso = IdPermiso
    FROM dbo.Permiso
    WHERE Codigo = N'CLIENTE_GESTIONAR';

    IF @IdPermiso IS NULL
    BEGIN
        THROW 50032,
            'No existe el permiso CLIENTE_GESTIONAR.',
            1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.GrupoPermiso
        WHERE
            IdGrupo = @IdGrupo
            AND IdPermiso = @IdPermiso
    )
    BEGIN
        INSERT INTO dbo.GrupoPermiso
        (
            IdGrupo,
            IdPermiso
        )
        VALUES
        (
            @IdGrupo,
            @IdPermiso
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Permiso CLIENTE_GESTIONAR asignado correctamente.';
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
    grupo.Codigo AS CodigoGrupo,
    permiso.Codigo AS CodigoPermiso
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo = grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso = grupoPermiso.IdPermiso
WHERE
    grupo.Codigo = N'ADMINISTRADOR_GENERAL'
    AND permiso.Codigo = N'CLIENTE_GESTIONAR';
GO
