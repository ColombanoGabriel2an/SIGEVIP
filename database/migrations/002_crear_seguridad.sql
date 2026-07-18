USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
    BEGIN
        THROW 50001,
            'No existe dbo.VersionBaseDatos. Ejecute primero la migración 001.',
            1;
    END;

    /*
        Persona
        Una Persona puede existir sin Usuario.
        La relación Persona 1 - 0..1 Usuario se completa mediante
        un índice único sobre Usuario.IdPersona.
    */
    IF OBJECT_ID(N'dbo.Persona', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Persona
        (
            IdPersona INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Persona PRIMARY KEY,

            Nombre NVARCHAR(100) NOT NULL,

            Apellido NVARCHAR(100) NOT NULL,

            Email NVARCHAR(254) NOT NULL,

            Activo BIT NOT NULL
                CONSTRAINT DF_Persona_Activo
                DEFAULT (1),

            CONSTRAINT CK_Persona_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),

            CONSTRAINT CK_Persona_Apellido_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0),

            CONSTRAINT CK_Persona_Email_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Email))) > 0)
        );
    END;

    /*
        Grupo
        Representa una agrupación configurable de permisos.
    */
    IF OBJECT_ID(N'dbo.Grupo', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Grupo
        (
            IdGrupo INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Grupo PRIMARY KEY,

            Codigo NVARCHAR(100) NOT NULL,

            Nombre NVARCHAR(150) NOT NULL,

            Descripcion NVARCHAR(500) NULL,

            Activo BIT NOT NULL
                CONSTRAINT DF_Grupo_Activo
                DEFAULT (1),

            CONSTRAINT CK_Grupo_Codigo_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),

            CONSTRAINT CK_Grupo_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
        );
    END;

    /*
        Permiso
        Representa una autorización funcional concreta.
    */
    IF OBJECT_ID(N'dbo.Permiso', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Permiso
        (
            IdPermiso INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Permiso PRIMARY KEY,

            Codigo NVARCHAR(100) NOT NULL,

            Nombre NVARCHAR(150) NOT NULL,

            Descripcion NVARCHAR(500) NULL,

            Activo BIT NOT NULL
                CONSTRAINT DF_Permiso_Activo
                DEFAULT (1),

            CONSTRAINT CK_Permiso_Codigo_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),

            CONSTRAINT CK_Permiso_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
        );
    END;

    /*
        Usuario
        No almacena contraseñas en texto plano.
        PasswordHash y PasswordSalt corresponden a PBKDF2-HMAC-SHA256.
    */
    IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Usuario
        (
            IdUsuario INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Usuario PRIMARY KEY,

            IdPersona INT NOT NULL,

            NombreUsuario NVARCHAR(100) NOT NULL,

            PasswordHash VARBINARY(32) NOT NULL,

            PasswordSalt VARBINARY(32) NOT NULL,

            IteracionesPassword INT NOT NULL,

            Activo BIT NOT NULL
                CONSTRAINT DF_Usuario_Activo
                DEFAULT (1),

            CONSTRAINT FK_Usuario_Persona
                FOREIGN KEY (IdPersona)
                REFERENCES dbo.Persona (IdPersona),

            CONSTRAINT CK_Usuario_NombreUsuario_NoVacio
                CHECK (
                    LEN(LTRIM(RTRIM(NombreUsuario))) > 0
                ),

            CONSTRAINT CK_Usuario_PasswordHash_Longitud
                CHECK (
                    DATALENGTH(PasswordHash) = 32
                ),

            CONSTRAINT CK_Usuario_PasswordSalt_Longitud
                CHECK (
                    DATALENGTH(PasswordSalt) = 32
                ),

            CONSTRAINT CK_Usuario_IteracionesPassword
                CHECK (
                    IteracionesPassword > 0
                )
        );
    END;

    /*
        UsuarioGrupo
        Relación muchos a muchos entre Usuario y Grupo.
    */
    IF OBJECT_ID(N'dbo.UsuarioGrupo', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.UsuarioGrupo
        (
            IdUsuario INT NOT NULL,

            IdGrupo INT NOT NULL,

            CONSTRAINT PK_UsuarioGrupo
                PRIMARY KEY (
                    IdUsuario,
                    IdGrupo
                ),

            CONSTRAINT FK_UsuarioGrupo_Usuario
                FOREIGN KEY (IdUsuario)
                REFERENCES dbo.Usuario (IdUsuario),

            CONSTRAINT FK_UsuarioGrupo_Grupo
                FOREIGN KEY (IdGrupo)
                REFERENCES dbo.Grupo (IdGrupo)
        );
    END;

    /*
        GrupoPermiso
        Relación muchos a muchos entre Grupo y Permiso.
    */
    IF OBJECT_ID(N'dbo.GrupoPermiso', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.GrupoPermiso
        (
            IdGrupo INT NOT NULL,

            IdPermiso INT NOT NULL,

            CONSTRAINT PK_GrupoPermiso
                PRIMARY KEY (
                    IdGrupo,
                    IdPermiso
                ),

            CONSTRAINT FK_GrupoPermiso_Grupo
                FOREIGN KEY (IdGrupo)
                REFERENCES dbo.Grupo (IdGrupo),

            CONSTRAINT FK_GrupoPermiso_Permiso
                FOREIGN KEY (IdPermiso)
                REFERENCES dbo.Permiso (IdPermiso)
        );
    END;

    /*
        GrupoGrupo
        Persiste la relación jerárquica del patrón Composite.

        Los ciclos indirectos se validarán posteriormente en Application
        y durante la reconstrucción del Composite.
    */
    IF OBJECT_ID(N'dbo.GrupoGrupo', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.GrupoGrupo
        (
            IdGrupoPadre INT NOT NULL,

            IdGrupoHijo INT NOT NULL,

            CONSTRAINT PK_GrupoGrupo
                PRIMARY KEY (
                    IdGrupoPadre,
                    IdGrupoHijo
                ),

            CONSTRAINT FK_GrupoGrupo_GrupoPadre
                FOREIGN KEY (IdGrupoPadre)
                REFERENCES dbo.Grupo (IdGrupo),

            CONSTRAINT FK_GrupoGrupo_GrupoHijo
                FOREIGN KEY (IdGrupoHijo)
                REFERENCES dbo.Grupo (IdGrupo),

            CONSTRAINT CK_GrupoGrupo_GruposDiferentes
                CHECK (
                    IdGrupoPadre <> IdGrupoHijo
                )
        );
    END;

    /*
        Índices únicos.
    */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'UX_Usuario_NombreUsuario'
            AND object_id = OBJECT_ID(N'dbo.Usuario')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Usuario_NombreUsuario
            ON dbo.Usuario (NombreUsuario);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'UX_Usuario_IdPersona'
            AND object_id = OBJECT_ID(N'dbo.Usuario')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Usuario_IdPersona
            ON dbo.Usuario (IdPersona);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'UX_Grupo_Codigo'
            AND object_id = OBJECT_ID(N'dbo.Grupo')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Grupo_Codigo
            ON dbo.Grupo (Codigo);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'UX_Permiso_Codigo'
            AND object_id = OBJECT_ID(N'dbo.Permiso')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Permiso_Codigo
            ON dbo.Permiso (Codigo);
    END;

    /*
        Índices auxiliares para consultas inversas sobre tablas asociativas.
        Las claves primarias compuestas ya indexan el primer campo.
    */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'IX_UsuarioGrupo_IdGrupo'
            AND object_id = OBJECT_ID(N'dbo.UsuarioGrupo')
    )
    BEGIN
        CREATE INDEX IX_UsuarioGrupo_IdGrupo
            ON dbo.UsuarioGrupo (IdGrupo);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'IX_GrupoPermiso_IdPermiso'
            AND object_id = OBJECT_ID(N'dbo.GrupoPermiso')
    )
    BEGIN
        CREATE INDEX IX_GrupoPermiso_IdPermiso
            ON dbo.GrupoPermiso (IdPermiso);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            name = N'IX_GrupoGrupo_IdGrupoHijo'
            AND object_id = OBJECT_ID(N'dbo.GrupoGrupo')
    )
    BEGIN
        CREATE INDEX IX_GrupoGrupo_IdGrupoHijo
            ON dbo.GrupoGrupo (IdGrupoHijo);
    END;

    /*
        Registro idempotente de la migración.
    */
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'002'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'002',
            N'Creación del modelo relacional de seguridad'
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Migración 002 aplicada correctamente.';
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
WHERE NumeroVersion = N'002';
GO
