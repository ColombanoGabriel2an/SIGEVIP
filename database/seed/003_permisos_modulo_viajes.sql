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
        THROW 50050,
            'No existe el esquema de seguridad. Ejecute primero la migración 002.',
            1;
    END;

    DECLARE @IdGrupo INT;

    SELECT
        @IdGrupo = IdGrupo
    FROM dbo.Grupo
    WHERE Codigo = N'ADMINISTRADOR_GENERAL';

    IF @IdGrupo IS NULL
    BEGIN
        THROW 50051,
            'No existe el grupo ADMINISTRADOR_GENERAL.',
            1;
    END;

    DECLARE @PermisosRequeridos TABLE
    (
        Codigo NVARCHAR(100) NOT NULL
            PRIMARY KEY
    );

    INSERT INTO @PermisosRequeridos
    (
        Codigo
    )
    VALUES
        (N'VIAJE_CREAR'),
        (N'VIAJE_CANCELAR');

    IF EXISTS
    (
        SELECT 1
        FROM @PermisosRequeridos AS requerido
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.Permiso AS permiso
            WHERE permiso.Codigo = requerido.Codigo
        )
    )
    BEGIN
        THROW 50052,
            'No existen todos los permisos requeridos para el módulo Viajes. Ejecute el seed 001.',
            1;
    END;

    INSERT INTO dbo.GrupoPermiso
    (
        IdGrupo,
        IdPermiso
    )
    SELECT
        @IdGrupo,
        permiso.IdPermiso
    FROM @PermisosRequeridos AS requerido
    INNER JOIN dbo.Permiso AS permiso
        ON permiso.Codigo = requerido.Codigo
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GrupoPermiso AS existente
        WHERE
            existente.IdGrupo = @IdGrupo
            AND existente.IdPermiso =
                permiso.IdPermiso
    );

    COMMIT TRANSACTION;

    PRINT N'Permisos del módulo Viajes asignados correctamente.';
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
    AND permiso.Codigo IN
    (
        N'VIAJE_CONSULTAR',
        N'VIAJE_CREAR',
        N'VIAJE_CANCELAR'
    )
ORDER BY permiso.Codigo;
GO
