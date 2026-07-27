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
        THROW 50090,
            'No existe el esquema de seguridad. Ejecute primero la migración 002.',
            1;
    END;

    DECLARE @Permisos TABLE
    (
        Codigo NVARCHAR(100) NOT NULL
            PRIMARY KEY,

        Nombre NVARCHAR(150) NOT NULL,

        Descripcion NVARCHAR(500) NULL
    );

    INSERT INTO @Permisos
    (
        Codigo,
        Nombre,
        Descripcion
    )
    VALUES
        (
            N'VIATICO_CONSULTAR',
            N'Consultar viáticos',
            N'Permite consultar viáticos asociados a viajes.'
        ),
        (
            N'VIATICO_REGISTRAR',
            N'Registrar viáticos',
            N'Permite registrar nuevos viáticos en viajes abiertos.'
        ),
        (
            N'VIATICO_MODIFICAR',
            N'Modificar viáticos',
            N'Permite modificar viáticos de viajes abiertos.'
        ),
        (
            N'VIAJE_ENVIAR_RENDICION',
            N'Enviar viaje a rendición',
            N'Permite enviar un viaje abierto al proceso de rendición.'
        ),
        (
            N'RENDICION_REVISAR',
            N'Revisar rendiciones',
            N'Permite consultar y revisar rendiciones pendientes.'
        ),
        (
            N'RENDICION_EXCLUIR_VIATICO',
            N'Excluir viáticos de rendición',
            N'Permite excluir lógicamente un viático durante la revisión.'
        ),
        (
            N'RENDICION_REACTIVAR_VIATICO',
            N'Reactivar viáticos de rendición',
            N'Permite reactivar un viático previamente excluido.'
        ),
        (
            N'RENDICION_AJUSTAR_ANTICIPO',
            N'Ajustar anticipo de rendición',
            N'Permite ajustar el monto anticipado durante la revisión.'
        ),
        (
            N'RENDICION_APROBAR',
            N'Aprobar rendiciones',
            N'Permite aprobar una rendición completa.'
        ),
        (
            N'RENDICION_CANCELAR',
            N'Cancelar rendiciones',
            N'Permite cancelar un viaje en rendición cuando las reglas lo admiten.'
        );

    UPDATE permisoExistente
    SET
        permisoExistente.Nombre =
            permisoOrigen.Nombre,
        permisoExistente.Descripcion =
            permisoOrigen.Descripcion
    FROM dbo.Permiso AS permisoExistente
    INNER JOIN @Permisos AS permisoOrigen
        ON permisoOrigen.Codigo =
            permisoExistente.Codigo;

    INSERT INTO dbo.Permiso
    (
        Codigo,
        Nombre,
        Descripcion,
        Activo
    )
    SELECT
        permisoOrigen.Codigo,
        permisoOrigen.Nombre,
        permisoOrigen.Descripcion,
        1
    FROM @Permisos AS permisoOrigen
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Permiso AS permisoExistente
        WHERE permisoExistente.Codigo =
            permisoOrigen.Codigo
    );

    DECLARE @Asignaciones TABLE
    (
        CodigoGrupo NVARCHAR(100) NOT NULL,

        CodigoPermiso NVARCHAR(100) NOT NULL,

        PRIMARY KEY
        (
            CodigoGrupo,
            CodigoPermiso
        )
    );

    INSERT INTO @Asignaciones
    (
        CodigoGrupo,
        CodigoPermiso
    )
    VALUES
        /*
            Administrativo
        */
        (N'ADMINISTRATIVO', N'VIATICO_CONSULTAR'),
        (N'ADMINISTRATIVO', N'VIATICO_REGISTRAR'),
        (N'ADMINISTRATIVO', N'VIATICO_MODIFICAR'),
        (N'ADMINISTRATIVO', N'VIAJE_ENVIAR_RENDICION'),

        /*
            Gerente
        */
        (N'GERENTE', N'VIATICO_CONSULTAR'),
        (N'GERENTE', N'RENDICION_REVISAR'),
        (N'GERENTE', N'RENDICION_EXCLUIR_VIATICO'),
        (N'GERENTE', N'RENDICION_REACTIVAR_VIATICO'),
        (N'GERENTE', N'RENDICION_AJUSTAR_ANTICIPO'),
        (N'GERENTE', N'RENDICION_APROBAR'),
        (N'GERENTE', N'RENDICION_CANCELAR'),

        /*
            Administrador General
        */
        (N'ADMINISTRADOR_GENERAL', N'VIATICO_CONSULTAR'),
        (N'ADMINISTRADOR_GENERAL', N'VIATICO_REGISTRAR'),
        (N'ADMINISTRADOR_GENERAL', N'VIATICO_MODIFICAR'),
        (N'ADMINISTRADOR_GENERAL', N'VIAJE_ENVIAR_RENDICION'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_REVISAR'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_EXCLUIR_VIATICO'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_REACTIVAR_VIATICO'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_AJUSTAR_ANTICIPO'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_APROBAR'),
        (N'ADMINISTRADOR_GENERAL', N'RENDICION_CANCELAR');

    IF EXISTS
    (
        SELECT 1
        FROM @Asignaciones AS asignacion
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.Grupo AS grupo
            WHERE grupo.Codigo =
                asignacion.CodigoGrupo
        )
    )
    BEGIN
        THROW 50091,
            'No existen todos los grupos requeridos. Ejecute primero el seed 001.',
            1;
    END;

    INSERT INTO dbo.GrupoPermiso
    (
        IdGrupo,
        IdPermiso
    )
    SELECT
        grupo.IdGrupo,
        permiso.IdPermiso
    FROM @Asignaciones AS asignacion
    INNER JOIN dbo.Grupo AS grupo
        ON grupo.Codigo =
            asignacion.CodigoGrupo
    INNER JOIN dbo.Permiso AS permiso
        ON permiso.Codigo =
            asignacion.CodigoPermiso
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GrupoPermiso AS existente
        WHERE
            existente.IdGrupo =
                grupo.IdGrupo
            AND existente.IdPermiso =
                permiso.IdPermiso
    );

    COMMIT TRANSACTION;

    PRINT N'Permisos de Viáticos y Rendiciones aplicados correctamente.';
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
    grupo.Codigo AS CodigoGrupo,
    permiso.Codigo AS CodigoPermiso,
    permiso.Activo
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo =
        grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso =
        grupoPermiso.IdPermiso
WHERE permiso.Codigo IN
(
    N'VIATICO_CONSULTAR',
    N'VIATICO_REGISTRAR',
    N'VIATICO_MODIFICAR',
    N'VIAJE_ENVIAR_RENDICION',
    N'RENDICION_REVISAR',
    N'RENDICION_EXCLUIR_VIATICO',
    N'RENDICION_REACTIVAR_VIATICO',
    N'RENDICION_AJUSTAR_ANTICIPO',
    N'RENDICION_APROBAR',
    N'RENDICION_CANCELAR'
)
ORDER BY
    grupo.Codigo,
    permiso.Codigo;
GO