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
        THROW 50080,
            'No existe dbo.VersionBaseDatos. Ejecute primero la migración 001.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Persona', N'U') IS NULL
    BEGIN
        THROW 50081,
            'No existe dbo.Persona. Ejecute primero la migración 002.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
    BEGIN
        THROW 50082,
            'No existe dbo.Usuario. Ejecute primero la migración 002.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Viaje', N'U') IS NULL
    BEGIN
        THROW 50083,
            'No existe dbo.Viaje. Ejecute primero la migración 004.',
            1;
    END;

    /*
        Correspondencia de enums:

        CategoriaGasto
        1 = Combustible
        2 = Alojamiento
        3 = Alimentacion
        4 = Peaje
        5 = Estacionamiento
        6 = Transporte
        7 = Otros

        MetodoPago
        1 = PagoPersonal
        2 = TarjetaCorporativa
        3 = EfectivoEmpresa

        EstadoViatico
        1 = Vigente
        2 = Excluido

        TipoComprobante
        1 = FacturaA
        2 = FacturaB
        3 = FacturaC
        4 = Ticket
        5 = Recibo
        6 = Otro

        SituacionFiscal
        1 = ResponsableInscripto
        2 = Monotributista
        3 = Exento
        4 = ConsumidorFinal
        5 = NoInformada
    */

    /*
        Auditoría del proceso de rendición en Viaje.
    */

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'IdUsuarioEnvioRendicion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD IdUsuarioEnvioRendicion INT NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'FechaEnvioRendicion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD FechaEnvioRendicion DATETIME2(0) NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'IdUsuarioAprobador'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD IdUsuarioAprobador INT NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'FechaAprobacion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD FechaAprobacion DATETIME2(0) NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'MotivoCancelacion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD MotivoCancelacion NVARCHAR(500) NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'IdUsuarioCancelacion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD IdUsuarioCancelacion INT NULL;
    END;

    IF COL_LENGTH(
        N'dbo.Viaje',
        N'FechaCancelacion'
    ) IS NULL
    BEGIN
        ALTER TABLE dbo.Viaje
        ADD FechaCancelacion DATETIME2(0) NULL;
    END;

    /*
        Claves foráneas de auditoría de Viaje.
    */

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'FK_Viaje_UsuarioEnvioRendicion'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT FK_Viaje_UsuarioEnvioRendicion
    FOREIGN KEY
    (
        IdUsuarioEnvioRendicion
    )
    REFERENCES dbo.Usuario
    (
        IdUsuario
    );'
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'FK_Viaje_UsuarioAprobador'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT FK_Viaje_UsuarioAprobador
    FOREIGN KEY
    (
        IdUsuarioAprobador
    )
    REFERENCES dbo.Usuario
    (
        IdUsuario
    );'
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'FK_Viaje_UsuarioCancelacion'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT FK_Viaje_UsuarioCancelacion
    FOREIGN KEY
    (
        IdUsuarioCancelacion
    )
    REFERENCES dbo.Usuario
    (
        IdUsuario
    );'
        );
    END;

    /*
        Los pares de auditoría deben encontrarse completos.
        No se exige auditoría según EstadoViaje en esta migración
        para mantener compatibilidad con registros históricos.
    */

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'CK_Viaje_AuditoriaEnvio_Completa'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT CK_Viaje_AuditoriaEnvio_Completa
    CHECK
    (
        (
            IdUsuarioEnvioRendicion IS NULL
            AND FechaEnvioRendicion IS NULL
        )
        OR
        (
            IdUsuarioEnvioRendicion IS NOT NULL
            AND FechaEnvioRendicion IS NOT NULL
        )
    );'
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'CK_Viaje_AuditoriaAprobacion_Completa'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT CK_Viaje_AuditoriaAprobacion_Completa
    CHECK
    (
        (
            IdUsuarioAprobador IS NULL
            AND FechaAprobacion IS NULL
        )
        OR
        (
            IdUsuarioAprobador IS NOT NULL
            AND FechaAprobacion IS NOT NULL
        )
    );'
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE
            parent_object_id =
                OBJECT_ID(N'dbo.Viaje')
            AND name =
                N'CK_Viaje_AuditoriaCancelacion_Completa'
    )
    BEGIN
        EXEC
        (
            N'ALTER TABLE dbo.Viaje
WITH CHECK
ADD CONSTRAINT CK_Viaje_AuditoriaCancelacion_Completa
    CHECK
    (
        (
            MotivoCancelacion IS NULL
            AND IdUsuarioCancelacion IS NULL
            AND FechaCancelacion IS NULL
        )
        OR
        (
            MotivoCancelacion IS NOT NULL
            AND LEN(
                LTRIM(
                    RTRIM(
                        MotivoCancelacion
                    )
                )
            ) > 0
            AND IdUsuarioCancelacion IS NOT NULL
            AND FechaCancelacion IS NOT NULL
        )
    );'
        );
    END;

    /*
        Viatico.
    */

    IF OBJECT_ID(N'dbo.Viatico', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Viatico
        (
            IdViatico INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Viatico
                PRIMARY KEY,

            IdViaje INT NOT NULL,

            Fecha DATE NOT NULL,

            CategoriaGasto TINYINT NOT NULL,

            MetodoPago TINYINT NOT NULL,

            IdPersonaPagadora INT NULL,

            Monto DECIMAL(18,2) NOT NULL,

            Descripcion NVARCHAR(1000) NOT NULL
                CONSTRAINT DF_Viatico_Descripcion
                DEFAULT (N''),

            EstadoViatico TINYINT NOT NULL
                CONSTRAINT DF_Viatico_EstadoViatico
                DEFAULT (1),

            MotivoExclusion NVARCHAR(500) NULL,

            IdUsuarioExclusion INT NULL,

            FechaExclusion DATETIME2(0) NULL,

            IdUsuarioReactivacion INT NULL,

            FechaReactivacion DATETIME2(0) NULL,

            CONSTRAINT FK_Viatico_Viaje
                FOREIGN KEY
                (
                    IdViaje
                )
                REFERENCES dbo.Viaje
                (
                    IdViaje
                ),

            CONSTRAINT FK_Viatico_PersonaPagadora
                FOREIGN KEY
                (
                    IdPersonaPagadora
                )
                REFERENCES dbo.Persona
                (
                    IdPersona
                ),

            CONSTRAINT FK_Viatico_UsuarioExclusion
                FOREIGN KEY
                (
                    IdUsuarioExclusion
                )
                REFERENCES dbo.Usuario
                (
                    IdUsuario
                ),

            CONSTRAINT FK_Viatico_UsuarioReactivacion
                FOREIGN KEY
                (
                    IdUsuarioReactivacion
                )
                REFERENCES dbo.Usuario
                (
                    IdUsuario
                ),

            CONSTRAINT CK_Viatico_CategoriaGasto
                CHECK
                (
                    CategoriaGasto IN
                    (
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7
                    )
                ),

            CONSTRAINT CK_Viatico_MetodoPago
                CHECK
                (
                    MetodoPago IN
                    (
                        1,
                        2,
                        3
                    )
                ),

            CONSTRAINT CK_Viatico_Monto
                CHECK
                (
                    Monto > 0
                ),

            CONSTRAINT CK_Viatico_EstadoViatico
                CHECK
                (
                    EstadoViatico IN
                    (
                        1,
                        2
                    )
                ),

            CONSTRAINT CK_Viatico_PagadorSegunMetodo
                CHECK
                (
                    (
                        MetodoPago IN
                        (
                            1,
                            2
                        )
                        AND IdPersonaPagadora
                            IS NOT NULL
                    )
                    OR
                    (
                        MetodoPago = 3
                        AND IdPersonaPagadora
                            IS NULL
                    )
                ),

            CONSTRAINT CK_Viatico_AuditoriaExclusion_Completa
                CHECK
                (
                    (
                        MotivoExclusion IS NULL
                        AND IdUsuarioExclusion IS NULL
                        AND FechaExclusion IS NULL
                    )
                    OR
                    (
                        MotivoExclusion IS NOT NULL
                        AND LEN(
                            LTRIM(
                                RTRIM(
                                    MotivoExclusion
                                )
                            )
                        ) > 0
                        AND IdUsuarioExclusion IS NOT NULL
                        AND FechaExclusion IS NOT NULL
                    )
                ),

            CONSTRAINT CK_Viatico_ExcluidoConAuditoria
                CHECK
                (
                    EstadoViatico <> 2
                    OR
                    (
                        MotivoExclusion IS NOT NULL
                        AND LEN(
                            LTRIM(
                                RTRIM(
                                    MotivoExclusion
                                )
                            )
                        ) > 0
                        AND IdUsuarioExclusion IS NOT NULL
                        AND FechaExclusion IS NOT NULL
                    )
                ),

            CONSTRAINT CK_Viatico_AuditoriaReactivacion_Completa
                CHECK
                (
                    (
                        IdUsuarioReactivacion IS NULL
                        AND FechaReactivacion IS NULL
                    )
                    OR
                    (
                        IdUsuarioReactivacion IS NOT NULL
                        AND FechaReactivacion IS NOT NULL
                    )
                )
        );
    END;

    /*
        Comprobante.
        La clave única sobre IdViatico establece una relación 1 a 0..1.
    */

    IF OBJECT_ID(N'dbo.Comprobante', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Comprobante
        (
            IdComprobante INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Comprobante
                PRIMARY KEY,

            IdViatico INT NOT NULL,

            TipoComprobante TINYINT NOT NULL,

            CuitProveedor NVARCHAR(20) NOT NULL,

            RazonSocialProveedor NVARCHAR(200) NOT NULL,

            SituacionFiscal TINYINT NOT NULL,

            Sucursal CHAR(4) NOT NULL,

            Numero CHAR(8) NOT NULL,

            MontoGravado DECIMAL(18,2) NOT NULL,

            MontoImpuestos DECIMAL(18,2) NOT NULL,

            CONSTRAINT FK_Comprobante_Viatico
                FOREIGN KEY
                (
                    IdViatico
                )
                REFERENCES dbo.Viatico
                (
                    IdViatico
                ),

            CONSTRAINT CK_Comprobante_TipoComprobante
                CHECK
                (
                    TipoComprobante IN
                    (
                        1,
                        2,
                        3,
                        4,
                        5,
                        6
                    )
                ),

            CONSTRAINT CK_Comprobante_CuitProveedor_NoVacio
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                CuitProveedor
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Comprobante_RazonSocial_NoVacia
                CHECK
                (
                    LEN(
                        LTRIM(
                            RTRIM(
                                RazonSocialProveedor
                            )
                        )
                    ) > 0
                ),

            CONSTRAINT CK_Comprobante_SituacionFiscal
                CHECK
                (
                    SituacionFiscal IN
                    (
                        1,
                        2,
                        3,
                        4,
                        5
                    )
                ),

            CONSTRAINT CK_Comprobante_Sucursal
                CHECK
                (
                    Sucursal NOT LIKE '%[^0-9]%'
                    AND LEN(Sucursal) = 4
                ),

            CONSTRAINT CK_Comprobante_Numero
                CHECK
                (
                    Numero NOT LIKE '%[^0-9]%'
                    AND LEN(Numero) = 8
                ),

            CONSTRAINT CK_Comprobante_MontoGravado
                CHECK
                (
                    MontoGravado >= 0
                ),

            CONSTRAINT CK_Comprobante_MontoImpuestos
                CHECK
                (
                    MontoImpuestos >= 0
                )
        );
    END;

    /*
        Índices.
    */

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Viatico')
            AND name =
                N'IX_Viatico_IdViaje_Fecha'
    )
    BEGIN
        CREATE INDEX IX_Viatico_IdViaje_Fecha
            ON dbo.Viatico
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
                OBJECT_ID(N'dbo.Viatico')
            AND name =
                N'IX_Viatico_IdViaje_Estado'
    )
    BEGIN
        CREATE INDEX IX_Viatico_IdViaje_Estado
            ON dbo.Viatico
            (
                IdViaje,
                EstadoViatico
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Viatico')
            AND name =
                N'IX_Viatico_CategoriaGasto'
    )
    BEGIN
        CREATE INDEX IX_Viatico_CategoriaGasto
            ON dbo.Viatico
            (
                CategoriaGasto
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Viatico')
            AND name =
                N'IX_Viatico_IdPersonaPagadora'
    )
    BEGIN
        CREATE INDEX IX_Viatico_IdPersonaPagadora
            ON dbo.Viatico
            (
                IdPersonaPagadora
            )
            WHERE IdPersonaPagadora IS NOT NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE
            object_id =
                OBJECT_ID(N'dbo.Comprobante')
            AND name =
                N'UX_Comprobante_IdViatico'
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Comprobante_IdViatico
            ON dbo.Comprobante
            (
                IdViatico
            );
    END;

    /*
        Registro de versión.
    */

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'006'
    )
    BEGIN
        INSERT INTO dbo.VersionBaseDatos
        (
            NumeroVersion,
            Descripcion
        )
        VALUES
        (
            N'006',
            N'Creación del modelo relacional de viáticos, comprobantes y auditoría de rendiciones'
        );
    END;

    COMMIT TRANSACTION;

    PRINT N'Migración 006 aplicada correctamente.';
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
WHERE NumeroVersion = N'006';
GO
