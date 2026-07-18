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
        THROW 50010,
            'No existe el esquema de seguridad. Ejecute primero la migración 002.',
            1;
    END;

    /*
        Catálogo temporal de grupos iniciales.
    */
    DECLARE @Grupos TABLE
    (
        Codigo NVARCHAR(100) NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(500) NULL
    );

    INSERT INTO @Grupos
    (
        Codigo,
        Nombre,
        Descripcion
    )
    VALUES
        (
            N'COMERCIAL',
            N'Comercial',
            N'Grupo para operaciones comerciales, clientes, viajes y visitas.'
        ),
        (
            N'ADMINISTRATIVO',
            N'Administrativo',
            N'Grupo para carga y gestión administrativa de viáticos y rendiciones.'
        ),
        (
            N'GERENTE',
            N'Gerente',
            N'Grupo para revisión, exclusión y aprobación de rendiciones.'
        ),
        (
            N'ADMINISTRADOR_GENERAL',
            N'Administrador General',
            N'Grupo para administración de usuarios, grupos, permisos y auditoría.'
        );

    /*
        Actualiza los grupos existentes sin alterar su estado lógico.
    */
    UPDATE grupoExistente
    SET
        grupoExistente.Nombre = grupoOrigen.Nombre,
        grupoExistente.Descripcion = grupoOrigen.Descripcion
    FROM dbo.Grupo AS grupoExistente
    INNER JOIN @Grupos AS grupoOrigen
        ON grupoOrigen.Codigo = grupoExistente.Codigo;

    /*
        Inserta únicamente los grupos faltantes.
    */
    INSERT INTO dbo.Grupo
    (
        Codigo,
        Nombre,
        Descripcion,
        Activo
    )
    SELECT
        grupoOrigen.Codigo,
        grupoOrigen.Nombre,
        grupoOrigen.Descripcion,
        1
    FROM @Grupos AS grupoOrigen
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Grupo AS grupoExistente
        WHERE grupoExistente.Codigo = grupoOrigen.Codigo
    );

    /*
        Catálogo temporal de permisos iniciales.
    */
    DECLARE @Permisos TABLE
    (
        Codigo NVARCHAR(100) NOT NULL PRIMARY KEY,
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
            N'CLIENTE_CONSULTAR',
            N'Consultar clientes',
            N'Permite consultar información y listados de clientes.'
        ),
        (
            N'CLIENTE_GESTIONAR',
            N'Gestionar clientes',
            N'Permite registrar y modificar clientes.'
        ),
        (
            N'VIAJE_CREAR',
            N'Crear viajes',
            N'Permite registrar nuevos viajes.'
        ),
        (
            N'VIAJE_CONSULTAR',
            N'Consultar viajes',
            N'Permite consultar viajes y su información relacionada.'
        ),
        (
            N'VISITA_REGISTRAR',
            N'Registrar visitas',
            N'Permite registrar visitas comerciales asociadas a viajes.'
        ),
        (
            N'VIATICO_CARGAR',
            N'Cargar viáticos',
            N'Permite incorporar viáticos a un viaje.'
        ),
        (
            N'VIATICO_MODIFICAR',
            N'Modificar viáticos',
            N'Permite modificar información de viáticos habilitados.'
        ),
        (
            N'VIAJE_ENVIAR_RENDICION',
            N'Enviar viaje a rendición',
            N'Permite enviar un viaje abierto al proceso de rendición.'
        ),
        (
            N'RENDICION_REVISAR',
            N'Revisar rendiciones',
            N'Permite revisar los datos y gastos de una rendición.'
        ),
        (
            N'VIATICO_EXCLUIR',
            N'Excluir viáticos',
            N'Permite excluir lógicamente un viático durante la revisión.'
        ),
        (
            N'VIATICO_REACTIVAR',
            N'Reactivar viáticos',
            N'Permite reactivar un viático previamente excluido.'
        ),
        (
            N'VIAJE_APROBAR',
            N'Aprobar viajes',
            N'Permite aprobar un viaje que se encuentra en rendición.'
        ),
        (
            N'VIAJE_CANCELAR',
            N'Cancelar viajes',
            N'Permite cancelar viajes cuando las reglas de negocio lo admiten.'
        ),
        (
            N'USUARIO_GESTIONAR',
            N'Gestionar usuarios',
            N'Permite registrar, modificar, activar y desactivar usuarios.'
        ),
        (
            N'GRUPO_GESTIONAR',
            N'Gestionar grupos',
            N'Permite administrar grupos y sus asociaciones.'
        ),
        (
            N'PERMISO_GESTIONAR',
            N'Gestionar permisos',
            N'Permite administrar el catálogo de permisos.'
        ),
        (
            N'AUDITORIA_CONSULTAR',
            N'Consultar auditoría',
            N'Permite consultar registros de auditoría del sistema.'
        );

    /*
        Actualiza los permisos existentes sin alterar su estado lógico.
    */
    UPDATE permisoExistente
    SET
        permisoExistente.Nombre = permisoOrigen.Nombre,
        permisoExistente.Descripcion = permisoOrigen.Descripcion
    FROM dbo.Permiso AS permisoExistente
    INNER JOIN @Permisos AS permisoOrigen
        ON permisoOrigen.Codigo = permisoExistente.Codigo;

    /*
        Inserta únicamente los permisos faltantes.
    */
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
        WHERE permisoExistente.Codigo = permisoOrigen.Codigo
    );

    /*
        Asignaciones iniciales de permisos por grupo.
    */
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
            Comercial
        */
        (N'COMERCIAL', N'CLIENTE_CONSULTAR'),
        (N'COMERCIAL', N'VIAJE_CREAR'),
        (N'COMERCIAL', N'VIAJE_CONSULTAR'),
        (N'COMERCIAL', N'VISITA_REGISTRAR'),

        /*
            Administrativo
        */
        (N'ADMINISTRATIVO', N'CLIENTE_CONSULTAR'),
        (N'ADMINISTRATIVO', N'VIAJE_CONSULTAR'),
        (N'ADMINISTRATIVO', N'VIATICO_CARGAR'),
        (N'ADMINISTRATIVO', N'VIATICO_MODIFICAR'),
        (N'ADMINISTRATIVO', N'VIAJE_ENVIAR_RENDICION'),

        /*
            Gerente
        */
        (N'GERENTE', N'CLIENTE_CONSULTAR'),
        (N'GERENTE', N'VIAJE_CONSULTAR'),
        (N'GERENTE', N'RENDICION_REVISAR'),
        (N'GERENTE', N'VIATICO_EXCLUIR'),
        (N'GERENTE', N'VIATICO_REACTIVAR'),
        (N'GERENTE', N'VIAJE_APROBAR'),
        (N'GERENTE', N'VIAJE_CANCELAR'),

        /*
            Administrador General
        */
        (N'ADMINISTRADOR_GENERAL', N'CLIENTE_CONSULTAR'),
        (N'ADMINISTRADOR_GENERAL', N'VIAJE_CONSULTAR'),
        (N'ADMINISTRADOR_GENERAL', N'USUARIO_GESTIONAR'),
        (N'ADMINISTRADOR_GENERAL', N'GRUPO_GESTIONAR'),
        (N'ADMINISTRADOR_GENERAL', N'PERMISO_GESTIONAR'),
        (N'ADMINISTRADOR_GENERAL', N'AUDITORIA_CONSULTAR');

    /*
        Inserta únicamente asociaciones inexistentes.
    */
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
        ON grupo.Codigo = asignacion.CodigoGrupo
    INNER JOIN dbo.Permiso AS permiso
        ON permiso.Codigo = asignacion.CodigoPermiso
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GrupoPermiso AS relacionExistente
        WHERE
            relacionExistente.IdGrupo = grupo.IdGrupo
            AND relacionExistente.IdPermiso = permiso.IdPermiso
    );

    COMMIT TRANSACTION;

    PRINT N'Catálogos iniciales de seguridad aplicados correctamente.';
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
    Codigo,
    Nombre,
    Activo
FROM dbo.Grupo
WHERE Codigo IN
(
    N'COMERCIAL',
    N'ADMINISTRATIVO',
    N'GERENTE',
    N'ADMINISTRADOR_GENERAL'
)
ORDER BY Codigo;
GO

SELECT
    Codigo,
    Nombre,
    Activo
FROM dbo.Permiso
WHERE Codigo IN
(
    N'CLIENTE_CONSULTAR',
    N'CLIENTE_GESTIONAR',
    N'VIAJE_CREAR',
    N'VIAJE_CONSULTAR',
    N'VISITA_REGISTRAR',
    N'VIATICO_CARGAR',
    N'VIATICO_MODIFICAR',
    N'VIAJE_ENVIAR_RENDICION',
    N'RENDICION_REVISAR',
    N'VIATICO_EXCLUIR',
    N'VIATICO_REACTIVAR',
    N'VIAJE_APROBAR',
    N'VIAJE_CANCELAR',
    N'USUARIO_GESTIONAR',
    N'GRUPO_GESTIONAR',
    N'PERMISO_GESTIONAR',
    N'AUDITORIA_CONSULTAR'
)
ORDER BY Codigo;
GO

SELECT
    grupo.Codigo AS CodigoGrupo,
    permiso.Codigo AS CodigoPermiso
FROM dbo.GrupoPermiso AS grupoPermiso
INNER JOIN dbo.Grupo AS grupo
    ON grupo.IdGrupo = grupoPermiso.IdGrupo
INNER JOIN dbo.Permiso AS permiso
    ON permiso.IdPermiso = grupoPermiso.IdPermiso
WHERE grupo.Codigo IN
(
    N'COMERCIAL',
    N'ADMINISTRATIVO',
    N'GERENTE',
    N'ADMINISTRADOR_GENERAL'
)
ORDER BY
    grupo.Codigo,
    permiso.Codigo;
GO
