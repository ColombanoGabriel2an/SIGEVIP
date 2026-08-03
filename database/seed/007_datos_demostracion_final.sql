USE master;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    Dataset final de demostración para SIGEVIP.

    Operación autorizada:
    - elimina todos los datos operativos actuales de SIGEVIP;
    - preserva Usuarios, Grupos, Permisos, credenciales y configuración;
    - preserva la Persona asociada al administrador real;
    - elimina Personas de demostración sin Usuario;
    - reinicia los IDENTITY operativos;
    - carga 30 Clientes, 8 empleados de demostración, 20 Viajes,
      45 Visitas, 60 Viáticos y 45 Comprobantes;
    - agrega Auditoría representativa para la demostración.

    La base SIGEVIP_TEST no se modifica.
*/

IF DB_ID(N'SIGEVIP') IS NULL
BEGIN
    THROW 52300,
        'No existe la base SIGEVIP.',
        1;
END;
GO

IF DB_ID(N'SIGEVIP_TEST') IS NULL
BEGIN
    THROW 52301,
        'No existe SIGEVIP_TEST. Complete primero el aislamiento de pruebas.',
        1;
END;
GO

DECLARE @BackupDirectory NVARCHAR(4000) =
    CONVERT(
        NVARCHAR(4000),
        SERVERPROPERTY('InstanceDefaultBackupPath')
    );

IF NULLIF(LTRIM(RTRIM(@BackupDirectory)), N'') IS NULL
BEGIN
    THROW 52302,
        'SQL Server no informó su carpeta predeterminada de backups.',
        1;
END;

IF RIGHT(@BackupDirectory, 1) NOT IN (N'\', N'/')
BEGIN
    SET @BackupDirectory = @BackupDirectory + N'\';
END;

DECLARE @Sufijo NVARCHAR(32) =
    CONVERT(CHAR(8), SYSDATETIME(), 112)
    + N'_'
    + REPLACE(
        CONVERT(CHAR(8), SYSDATETIME(), 108),
        N':',
        N''
    );

DECLARE @BackupFile NVARCHAR(4000) =
    @BackupDirectory
    + N'SIGEVIP_ANTES_DATASET_FINAL_'
    + @Sufijo
    + N'.bak';

PRINT N'Backup previo: ' + @BackupFile;

BACKUP DATABASE [SIGEVIP]
TO DISK = @BackupFile
WITH
    COPY_ONLY,
    INIT,
    CHECKSUM,
    STATS = 10;

RESTORE VERIFYONLY
FROM DISK = @BackupFile
WITH CHECKSUM;

PRINT N'Backup previo verificado correctamente.';
GO

USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    IF DB_NAME() <> N'SIGEVIP'
    BEGIN
        THROW 52303,
            'El script debe ejecutarse sobre SIGEVIP.',
            1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.VersionBaseDatos
        WHERE NumeroVersion = N'008'
    )
    BEGIN
        THROW 52304,
            'La migración 008 no está aplicada.',
            1;
    END;

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Persona', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Viaje', N'U') IS NULL
       OR OBJECT_ID(N'dbo.ViajeParticipante', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Visita', N'U') IS NULL
       OR OBJECT_ID(N'dbo.VisitaCliente', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Viatico', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Comprobante', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Auditoria', N'U') IS NULL
       OR OBJECT_ID(N'dbo.AuditoriaCambio', N'U') IS NULL
    BEGIN
        THROW 52305,
            'Faltan tablas obligatorias de SIGEVIP.',
            1;
    END;

    DECLARE @IdUsuarioActor INT;
    DECLARE @NombreUsuarioActor NVARCHAR(100);

    SELECT
        @IdUsuarioActor = usuario.IdUsuario,
        @NombreUsuarioActor = usuario.NombreUsuario
    FROM dbo.Usuario AS usuario
    WHERE
        LOWER(
            LTRIM(
                RTRIM(
                    usuario.NombreUsuario
                )
            )
        ) = N'gabriel'
        AND usuario.Activo = 1;

    IF @IdUsuarioActor IS NULL
    BEGIN
        THROW 52306,
            'No se encontró el administrador activo gabriel. No se modificaron datos.',
            1;
    END;

    BEGIN TRANSACTION;

    /*
        1. Limpieza operativa autorizada.
    */

    DELETE FROM dbo.AuditoriaCambio;
    DELETE FROM dbo.Auditoria;
    DELETE FROM dbo.Comprobante;
    DELETE FROM dbo.VisitaCliente;
    DELETE FROM dbo.Visita;
    DELETE FROM dbo.Viatico;
    DELETE FROM dbo.ViajeParticipante;
    DELETE FROM dbo.Viaje;
    DELETE FROM dbo.Cliente;

    /*
        Se eliminan únicamente Personas de demostración sin Usuario.
        No se elimina ninguna Persona vinculada a credenciales.
    */
    DELETE persona
    FROM dbo.Persona AS persona
    LEFT JOIN dbo.Usuario AS usuario
        ON usuario.IdPersona = persona.IdPersona
    WHERE
        usuario.IdUsuario IS NULL
        AND
        (
            persona.Email LIKE N'demo.%@sigevip.local'
            OR persona.Email LIKE N'demo.final.%@sigevip.local'
        );

    /*
        2. Reinicio de identificadores operativos.
        Los identificadores de seguridad no se reinician.
    */

    DBCC CHECKIDENT (N'dbo.AuditoriaCambio', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Auditoria', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Comprobante', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Visita', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Viatico', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Viaje', RESEED, 0)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Cliente', RESEED, 0)
        WITH NO_INFOMSGS;

    /*
        3. Clientes: 30 registros, ID 1 a 30.
    */

    SET IDENTITY_INSERT dbo.Cliente ON;

    INSERT INTO dbo.Cliente
    (
        IdCliente,
        RazonSocial,
        Cuit,
        Email,
        Telefono,
        Localidad,
        Provincia,
        Activo
    )
    VALUES
        (1, N'Agropecuaria Los Aromos SA', N'30-71000001-1', N'contacto@losaromos.demo', N'341-555-1001', N'Funes', N'Santa Fe', 1),
        (2, N'Campos del Sur SRL', N'30-71000002-2', N'administracion@camposdelsur.demo', N'341-555-1002', N'Roldán', N'Santa Fe', 1),
        (3, N'Cooperativa Horizonte', N'30-71000003-3', N'info@horizonte.demo', N'3464-555-1003', N'Casilda', N'Santa Fe', 1),
        (4, N'Productores Unidos SA', N'30-71000004-4', N'contacto@productoresunidos.demo', N'3471-555-1004', N'Las Rosas', N'Santa Fe', 1),
        (5, N'Estancias del Paraná SRL', N'30-71000005-5', N'gestion@estanciasparana.demo', N'3476-555-1005', N'San Lorenzo', N'Santa Fe', 1),
        (6, N'Cereales del Centro SA', N'30-71000006-6', N'comercial@cerealescentro.demo', N'341-555-1006', N'Rosario', N'Santa Fe', 1),
        (7, N'Lácteos del Litoral SRL', N'30-71000007-7', N'compras@lacteoslitoral.demo', N'3492-555-1007', N'Rafaela', N'Santa Fe', 1),
        (8, N'Semillas del Norte SA', N'30-71000008-8', N'ventas@semillasnorte.demo', N'3482-555-1008', N'Reconquista', N'Santa Fe', 1),
        (9, N'Maquinarias Pampeanas SRL', N'30-71000009-9', N'servicio@maquinariaspampeanas.demo', N'3462-555-1009', N'Venado Tuerto', N'Santa Fe', 1),
        (10, N'Transporte Rural del Centro', N'30-71000010-0', N'trafico@transporterural.demo', N'3471-555-1010', N'Cañada de Gómez', N'Santa Fe', 0),
        (11, N'Agroservicios Córdoba SA', N'30-71000011-1', N'info@agrocordoba.demo', N'351-555-1011', N'Córdoba', N'Córdoba', 1),
        (12, N'Cultivos del Este SRL', N'30-71000012-2', N'contacto@cultivoseste.demo', N'353-555-1012', N'Villa María', N'Córdoba', 1),
        (13, N'Ganadera del Plata SA', N'30-71000013-3', N'administracion@ganaderaplata.demo', N'2477-555-1013', N'Pergamino', N'Buenos Aires', 1),
        (14, N'Insumos Bonaerenses SRL', N'30-71000014-4', N'ventas@insumosba.demo', N'336-555-1014', N'San Nicolás', N'Buenos Aires', 1),
        (15, N'Frutos del Delta SA', N'30-71000015-5', N'compras@frutosdelta.demo', N'11-5555-1015', N'Tigre', N'Buenos Aires', 1),
        (16, N'Cooperativa Entrerriana', N'30-71000016-6', N'info@coopentrerriana.demo', N'343-555-1016', N'Paraná', N'Entre Ríos', 1),
        (17, N'Arrozales del Uruguay SA', N'30-71000017-7', N'contacto@arrozales.demo', N'3442-555-1017', N'Concepción del Uruguay', N'Entre Ríos', 1),
        (18, N'Bodegas Cuyanas SRL', N'30-71000018-8', N'comercial@bodegascuyanas.demo', N'261-555-1018', N'Mendoza', N'Mendoza', 0),
        (19, N'Olivares de Cuyo SA', N'30-71000019-9', N'ventas@olivarescuyo.demo', N'264-555-1019', N'San Juan', N'San Juan', 1),
        (20, N'Fruticultura Patagónica SRL', N'30-71000020-0', N'info@fruticulturapatagonica.demo', N'299-555-1020', N'Neuquén', N'Neuquén', 1),
        (21, N'Campos de La Pampa SA', N'30-71000021-1', N'gestion@camposlapampa.demo', N'2954-555-1021', N'Santa Rosa', N'La Pampa', 1),
        (22, N'Azucarera del Norte SRL', N'30-71000022-2', N'compras@azucareranorte.demo', N'381-555-1022', N'San Miguel de Tucumán', N'Tucumán', 1),
        (23, N'Tabacaleros de Salta SA', N'30-71000023-3', N'contacto@tabacalerossalta.demo', N'387-555-1023', N'Salta', N'Salta', 1),
        (24, N'Productores del Chaco SRL', N'30-71000024-4', N'administracion@productoreschaco.demo', N'362-555-1024', N'Resistencia', N'Chaco', 0),
        (25, N'Algodonera Formoseña SA', N'30-71000025-5', N'ventas@algodonerafsa.demo', N'370-555-1025', N'Formosa', N'Formosa', 1),
        (26, N'Yerbatera Misionera SRL', N'30-71000026-6', N'info@yerbateramisionera.demo', N'376-555-1026', N'Posadas', N'Misiones', 1),
        (27, N'Valles Riojanos SA', N'30-71000027-7', N'contacto@vallesriojanos.demo', N'380-555-1027', N'La Rioja', N'La Rioja', 1),
        (28, N'Viñedos de Cafayate SRL', N'30-71000028-8', N'comercial@vinedoscafayate.demo', N'3868-555-1028', N'Cafayate', N'Salta', 1),
        (29, N'Agroindustrial Jujeña SA', N'30-71000029-9', N'ventas@agrojujuy.demo', N'388-555-1029', N'San Salvador de Jujuy', N'Jujuy', 0),
        (30, N'Servicios Rurales del Sur SRL', N'30-71000030-0', N'servicios@ruralessur.demo', N'291-555-1030', N'Bahía Blanca', N'Buenos Aires', 1);

    SET IDENTITY_INSERT dbo.Cliente OFF;

    /*
        4. Empleados de demostración: 8 Personas.
        No se crean credenciales nuevas.
    */

    INSERT INTO dbo.Persona
    (
        Nombre,
        Apellido,
        Email,
        Activo
    )
    VALUES
        (N'Lucía', N'Fernández', N'demo.final.lucia@sigevip.local', 1),
        (N'Martín', N'Pereyra', N'demo.final.martin@sigevip.local', 1),
        (N'Sofía', N'Gómez', N'demo.final.sofia@sigevip.local', 1),
        (N'Diego', N'Ramírez', N'demo.final.diego@sigevip.local', 1),
        (N'Valentina', N'López', N'demo.final.valentina@sigevip.local', 1),
        (N'Nicolás', N'Acosta', N'demo.final.nicolas@sigevip.local', 1),
        (N'Camila', N'Benítez', N'demo.final.camila@sigevip.local', 1),
        (N'Joaquín', N'Herrera', N'demo.final.joaquin@sigevip.local', 1);

    DECLARE @Empleados TABLE
    (
        NumeroEmpleado INT NOT NULL PRIMARY KEY,
        IdPersona INT NOT NULL UNIQUE
    );

    INSERT INTO @Empleados
    (
        NumeroEmpleado,
        IdPersona
    )
    SELECT
        CASE persona.Email
            WHEN N'demo.final.lucia@sigevip.local' THEN 1
            WHEN N'demo.final.martin@sigevip.local' THEN 2
            WHEN N'demo.final.sofia@sigevip.local' THEN 3
            WHEN N'demo.final.diego@sigevip.local' THEN 4
            WHEN N'demo.final.valentina@sigevip.local' THEN 5
            WHEN N'demo.final.nicolas@sigevip.local' THEN 6
            WHEN N'demo.final.camila@sigevip.local' THEN 7
            WHEN N'demo.final.joaquin@sigevip.local' THEN 8
        END,
        persona.IdPersona
    FROM dbo.Persona AS persona
    WHERE persona.Email LIKE N'demo.final.%@sigevip.local';

    IF (SELECT COUNT(*) FROM @Empleados) <> 8
    BEGIN
        THROW 52307,
            'No se pudieron preparar los 8 empleados de demostración.',
            1;
    END;

    /*
        5. Viajes: 20 registros.
        Estados:
        1 Abierto: 8
        2 EnRendicion: 5
        3 Aprobado: 4
        4 Cancelado: 3
    */

    SET IDENTITY_INSERT dbo.Viaje ON;

    INSERT INTO dbo.Viaje
    (
        IdViaje,
        FechaInicio,
        FechaFin,
        Descripcion,
        TipoViaje,
        MontoAnticipado,
        EstadoViaje,
        IdUsuarioEnvioRendicion,
        FechaEnvioRendicion,
        IdUsuarioAprobador,
        FechaAprobacion,
        MotivoCancelacion,
        IdUsuarioCancelacion,
        FechaCancelacion
    )
    VALUES
        (1, '2026-01-12', '2026-01-15', N'DEMO FINAL — Gira comercial Rosario y Funes', 1, 180000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (2, '2026-02-02', '2026-02-05', N'DEMO FINAL — Relevamiento de clientes del cordón industrial', 1, 60000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (3, '2026-02-23', '2026-02-26', N'DEMO FINAL — Presentación de campaña agrícola centro', 2, 90000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (4, '2026-03-16', '2026-03-19', N'DEMO FINAL — Visitas técnicas a cooperativas regionales', 1, 200000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (5, '2026-04-06', '2026-04-09', N'DEMO FINAL — Desarrollo comercial en Córdoba', 2, 80000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (6, '2026-05-04', '2026-05-07', N'DEMO FINAL — Seguimiento de cuentas estratégicas', 1, 240000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (7, '2026-06-08', '2026-06-11', N'DEMO FINAL — Jornada comercial del litoral', 3, 100000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (8, '2026-07-06', '2026-07-09', N'DEMO FINAL — Prospección de clientes del norte', 1, 275000.00, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
        (9, '2026-01-26', '2026-01-29', N'DEMO FINAL — Ronda de negocios de verano', 3, 120000.00, 2, @IdUsuarioActor, '2026-01-30T09:00:00', NULL, NULL, NULL, NULL, NULL),
        (10, '2026-03-02', '2026-03-05', N'DEMO FINAL — Gira comercial sur santafesino', 1, 100000.00, 2, @IdUsuarioActor, '2026-03-06T09:00:00', NULL, NULL, NULL, NULL, NULL),
        (11, '2026-04-20', '2026-04-23', N'DEMO FINAL — Encuentros con distribuidores del centro', 2, 180000.00, 2, @IdUsuarioActor, '2026-04-24T09:00:00', NULL, NULL, NULL, NULL, NULL),
        (12, '2026-06-22', '2026-06-25', N'DEMO FINAL — Visita integral a clientes del Paraná', 1, 130000.00, 2, @IdUsuarioActor, '2026-06-26T09:00:00', NULL, NULL, NULL, NULL, NULL),
        (13, '2026-07-20', '2026-07-23', N'DEMO FINAL — Relevamiento comercial de Cuyo', 2, 200000.00, 2, @IdUsuarioActor, '2026-07-24T09:00:00', NULL, NULL, NULL, NULL, NULL),
        (14, '2026-02-09', '2026-02-12', N'DEMO FINAL — Feria regional de productores', 3, 168000.00, 3, @IdUsuarioActor, '2026-02-13T09:00:00', @IdUsuarioActor, '2026-02-14T11:30:00', NULL, NULL, NULL),
        (15, '2026-03-23', '2026-03-26', N'DEMO FINAL — Misión comercial bonaerense', 1, 210000.00, 3, @IdUsuarioActor, '2026-03-27T09:00:00', @IdUsuarioActor, '2026-03-28T11:30:00', NULL, NULL, NULL),
        (16, '2026-05-18', '2026-05-21', N'DEMO FINAL — Encuentro agroindustrial del litoral', 3, 200000.00, 3, @IdUsuarioActor, '2026-05-22T09:00:00', @IdUsuarioActor, '2026-05-23T11:30:00', NULL, NULL, NULL),
        (17, '2026-06-29', '2026-07-02', N'DEMO FINAL — Gira de consolidación de cuentas', 1, 260000.00, 3, @IdUsuarioActor, '2026-07-03T09:00:00', @IdUsuarioActor, '2026-07-04T11:30:00', NULL, NULL, NULL),
        (18, '2026-01-05', '2026-01-08', N'DEMO FINAL — Viaje cancelado por reprogramación operativa', 1, 150000.00, 4, NULL, NULL, NULL, NULL, N'Reprogramación operativa confirmada por coordinación.', @IdUsuarioActor, '2026-01-06T08:30:00'),
        (19, '2026-04-13', '2026-04-16', N'DEMO FINAL — Viaje cancelado por condiciones climáticas', 2, 185000.00, 4, NULL, NULL, NULL, NULL, N'Condiciones climáticas adversas para el traslado.', @IdUsuarioActor, '2026-04-14T08:30:00'),
        (20, '2026-07-13', '2026-07-16', N'DEMO FINAL — Viaje cancelado por indisponibilidad del cliente', 1, 170000.00, 4, NULL, NULL, NULL, NULL, N'Indisponibilidad informada por el cliente principal.', @IdUsuarioActor, '2026-07-14T08:30:00');

    SET IDENTITY_INSERT dbo.Viaje OFF;

    /*
        Dos participantes para cada Viaje y un tercero en Viajes pares.
    */

    INSERT INTO dbo.ViajeParticipante
    (
        IdViaje,
        IdPersona
    )
    SELECT
        viaje.IdViaje,
        empleado.IdPersona
    FROM dbo.Viaje AS viaje
    INNER JOIN @Empleados AS empleado
        ON empleado.NumeroEmpleado IN
        (
            ((viaje.IdViaje - 1) % 8) + 1,
            (viaje.IdViaje % 8) + 1
        );

    INSERT INTO dbo.ViajeParticipante
    (
        IdViaje,
        IdPersona
    )
    SELECT
        viaje.IdViaje,
        empleado.IdPersona
    FROM dbo.Viaje AS viaje
    INNER JOIN @Empleados AS empleado
        ON empleado.NumeroEmpleado =
            ((viaje.IdViaje + 2) % 8) + 1
    WHERE
        viaje.IdViaje % 2 = 0
        AND NOT EXISTS
        (
            SELECT 1
            FROM dbo.ViajeParticipante AS existente
            WHERE
                existente.IdViaje = viaje.IdViaje
                AND existente.IdPersona = empleado.IdPersona
        );

    /*
        6. Visitas: 45 registros, ID 1 a 45.
        Los Viajes cancelados no tienen Visitas.
    */

    DECLARE @NumerosVisita TABLE
    (
        Numero INT NOT NULL PRIMARY KEY
    );

    INSERT INTO @NumerosVisita
    (
        Numero
    )
    VALUES
        (1),
        (2),
        (3);

    ;WITH PlanVisitas AS
    (
        SELECT
            ROW_NUMBER() OVER
            (
                ORDER BY
                    viaje.IdViaje,
                    numero.Numero
            ) AS IdVisita,
            viaje.IdViaje,
            DATEADD(
                DAY,
                numero.Numero - 1,
                viaje.FechaInicio
            ) AS Fecha,
            CONCAT(
                N'DEMO FINAL — Visita ',
                numero.Numero,
                N' del viaje ',
                viaje.IdViaje,
                N': reunión comercial y relevamiento de necesidades.'
            ) AS Observacion,
            CASE ((viaje.IdViaje - 1) % 10)
                WHEN 0 THEN N'Rosario'
                WHEN 1 THEN N'Funes'
                WHEN 2 THEN N'Casilda'
                WHEN 3 THEN N'Rafaela'
                WHEN 4 THEN N'Córdoba'
                WHEN 5 THEN N'Pergamino'
                WHEN 6 THEN N'Paraná'
                WHEN 7 THEN N'Mendoza'
                WHEN 8 THEN N'Salta'
                ELSE N'Bahía Blanca'
            END AS LocalidadEncuentro
        FROM dbo.Viaje AS viaje
        CROSS JOIN @NumerosVisita AS numero
        WHERE
            viaje.IdViaje <= 17
            AND numero.Numero <=
                CASE
                    WHEN viaje.IdViaje BETWEEN 1 AND 8
                        THEN 3
                    WHEN viaje.IdViaje BETWEEN 9 AND 13
                        THEN 2
                    WHEN viaje.IdViaje BETWEEN 14 AND 16
                        THEN 3
                    WHEN viaje.IdViaje = 17
                        THEN 2
                    ELSE 0
                END
    )
    SELECT
        *
    INTO #PlanVisitas
    FROM PlanVisitas;

    IF (SELECT COUNT(*) FROM #PlanVisitas) <> 45
    BEGIN
        THROW 52308,
            'El plan de Visitas no contiene 45 registros.',
            1;
    END;

    SET IDENTITY_INSERT dbo.Visita ON;

    INSERT INTO dbo.Visita
    (
        IdVisita,
        IdViaje,
        Fecha,
        Observacion,
        LocalidadEncuentro
    )
    SELECT
        IdVisita,
        IdViaje,
        Fecha,
        Observacion,
        LocalidadEncuentro
    FROM #PlanVisitas
    ORDER BY IdVisita;

    SET IDENTITY_INSERT dbo.Visita OFF;

    DROP TABLE #PlanVisitas;

    /*
        Cada Visita se asocia a un Cliente.
        Cada tercera Visita se asocia también a un segundo Cliente.
    */

    INSERT INTO dbo.VisitaCliente
    (
        IdVisita,
        IdCliente
    )
    SELECT
        visita.IdVisita,
        ((visita.IdVisita - 1) % 30) + 1
    FROM dbo.Visita AS visita;

    INSERT INTO dbo.VisitaCliente
    (
        IdVisita,
        IdCliente
    )
    SELECT
        visita.IdVisita,
        ((visita.IdVisita + 10) % 30) + 1
    FROM dbo.Visita AS visita
    WHERE visita.IdVisita % 3 = 0;

    /*
        7. Viáticos: 60 registros, ID 1 a 60.
        - Viajes Abiertos: 3 por Viaje.
        - En Rendición y Aprobados: 4 por Viaje.
        - 7 Viáticos quedan Excluidos.
    */

    DECLARE @NumerosViatico TABLE
    (
        Numero INT NOT NULL PRIMARY KEY
    );

    INSERT INTO @NumerosViatico
    (
        Numero
    )
    VALUES
        (1),
        (2),
        (3),
        (4);

    ;WITH PlanBase AS
    (
        SELECT
            viaje.IdViaje,
            viaje.FechaInicio,
            viaje.FechaFin,
            viaje.EstadoViaje,
            numero.Numero,
            ROW_NUMBER() OVER
            (
                ORDER BY
                    viaje.IdViaje,
                    numero.Numero
            ) AS IdViatico
        FROM dbo.Viaje AS viaje
        CROSS JOIN @NumerosViatico AS numero
        WHERE
            viaje.IdViaje <= 17
            AND numero.Numero <=
                CASE
                    WHEN viaje.IdViaje BETWEEN 1 AND 8
                        THEN 3
                    WHEN viaje.IdViaje BETWEEN 9 AND 17
                        THEN 4
                    ELSE 0
                END
    ),
    PlanConMetodo AS
    (
        SELECT
            viaticoPlan.*,
            CAST(
                ((viaticoPlan.IdViaje + viaticoPlan.Numero - 2) % 7) + 1
                AS TINYINT
            ) AS CategoriaGasto,
            CAST(
                ((viaticoPlan.IdViaje + viaticoPlan.Numero - 2) % 3) + 1
                AS TINYINT
            ) AS MetodoPago
        FROM PlanBase AS viaticoPlan
    )
    SELECT
        viaticoPlan.IdViatico,
        viaticoPlan.IdViaje,
        DATEADD(
            DAY,
            viaticoPlan.Numero - 1,
            viaticoPlan.FechaInicio
        ) AS Fecha,
        viaticoPlan.CategoriaGasto,
        viaticoPlan.MetodoPago,
        CASE
            WHEN viaticoPlan.MetodoPago = 3
                THEN NULL
            ELSE pagador.IdPersona
        END AS IdPersonaPagadora,
        CAST(
            15000
            + (viaticoPlan.IdViaje * 2500)
            + (viaticoPlan.Numero * 3000)
            AS DECIMAL(18,2)
        ) AS Monto,
        CONCAT(
            N'DEMO FINAL — Viático ',
            viaticoPlan.Numero,
            N' del viaje ',
            viaticoPlan.IdViaje,
            N' para análisis de gastos.'
        ) AS Descripcion,
        CAST(
            CASE
                WHEN
                    viaticoPlan.Numero = 4
                    AND
                    (
                        viaticoPlan.IdViaje BETWEEN 9 AND 13
                        OR viaticoPlan.IdViaje IN (14, 16)
                    )
                    THEN 2
                ELSE 1
            END
            AS TINYINT
        ) AS EstadoViatico,
        CASE
            WHEN
                viaticoPlan.Numero = 4
                AND
                (
                    viaticoPlan.IdViaje BETWEEN 9 AND 13
                    OR viaticoPlan.IdViaje IN (14, 16)
                )
                THEN N'Gasto excluido del cierre por falta de respaldo suficiente.'
            ELSE NULL
        END AS MotivoExclusion,
        CASE
            WHEN
                viaticoPlan.Numero = 4
                AND
                (
                    viaticoPlan.IdViaje BETWEEN 9 AND 13
                    OR viaticoPlan.IdViaje IN (14, 16)
                )
                THEN @IdUsuarioActor
            ELSE NULL
        END AS IdUsuarioExclusion,
        CASE
            WHEN
                viaticoPlan.Numero = 4
                AND
                (
                    viaticoPlan.IdViaje BETWEEN 9 AND 13
                    OR viaticoPlan.IdViaje IN (14, 16)
                )
                THEN DATEADD(
                    HOUR,
                    10,
                    CAST(
                        DATEADD(
                            DAY,
                            1,
                            viaticoPlan.FechaFin
                        )
                        AS DATETIME2(0)
                    )
                )
            ELSE NULL
        END AS FechaExclusion,
        CAST(NULL AS INT) AS IdUsuarioReactivacion,
        CAST(NULL AS DATETIME2(0)) AS FechaReactivacion
    INTO #PlanViaticos
    FROM PlanConMetodo AS viaticoPlan
    OUTER APPLY
    (
        SELECT TOP (1)
            participante.IdPersona
        FROM dbo.ViajeParticipante AS participante
        WHERE participante.IdViaje = viaticoPlan.IdViaje
        ORDER BY participante.IdPersona
    ) AS pagador;

    IF (SELECT COUNT(*) FROM #PlanViaticos) <> 60
    BEGIN
        THROW 52309,
            'El plan de Viáticos no contiene 60 registros.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM #PlanViaticos
        WHERE
            MetodoPago IN (1, 2)
            AND IdPersonaPagadora IS NULL
    )
    BEGIN
        THROW 52310,
            'Existe un Viático personal o corporativo sin pagador.',
            1;
    END;

    SET IDENTITY_INSERT dbo.Viatico ON;

    INSERT INTO dbo.Viatico
    (
        IdViatico,
        IdViaje,
        Fecha,
        CategoriaGasto,
        MetodoPago,
        IdPersonaPagadora,
        Monto,
        Descripcion,
        EstadoViatico,
        MotivoExclusion,
        IdUsuarioExclusion,
        FechaExclusion,
        IdUsuarioReactivacion,
        FechaReactivacion
    )
    SELECT
        IdViatico,
        IdViaje,
        Fecha,
        CategoriaGasto,
        MetodoPago,
        IdPersonaPagadora,
        Monto,
        Descripcion,
        EstadoViatico,
        MotivoExclusion,
        IdUsuarioExclusion,
        FechaExclusion,
        IdUsuarioReactivacion,
        FechaReactivacion
    FROM #PlanViaticos
    ORDER BY IdViatico;

    SET IDENTITY_INSERT dbo.Viatico OFF;

    DROP TABLE #PlanViaticos;

    /*
        8. Comprobantes: 45 registros.
        Se omite cada cuarto Viático para conservar casos sin Comprobante.
    */

    SET IDENTITY_INSERT dbo.Comprobante ON;

    INSERT INTO dbo.Comprobante
    (
        IdComprobante,
        IdViatico,
        TipoComprobante,
        CuitProveedor,
        RazonSocialProveedor,
        SituacionFiscal,
        Sucursal,
        Numero,
        MontoGravado,
        MontoImpuestos
    )
    SELECT
        ROW_NUMBER() OVER
        (
            ORDER BY viatico.IdViatico
        ) AS IdComprobante,
        viatico.IdViatico,
        CAST(
            ((viatico.IdViatico - 1) % 6) + 1
            AS TINYINT
        ) AS TipoComprobante,
        CONCAT(
            N'30-80',
            RIGHT(
                N'000000'
                + CONVERT(
                    NVARCHAR(6),
                    viatico.IdViatico
                ),
                6
            ),
            N'-9'
        ) AS CuitProveedor,
        CONCAT(
            N'Proveedor demostración ',
            viatico.IdViatico
        ) AS RazonSocialProveedor,
        CAST(
            ((viatico.IdViatico - 1) % 5) + 1
            AS TINYINT
        ) AS SituacionFiscal,
        RIGHT(
            '0000'
            + CONVERT(
                VARCHAR(4),
                ((viatico.IdViatico - 1) % 12) + 1
            ),
            4
        ) AS Sucursal,
        RIGHT(
            '00000000'
            + CONVERT(
                VARCHAR(8),
                viatico.IdViatico
            ),
            8
        ) AS Numero,
        CAST(
            viatico.Monto * 0.80
            AS DECIMAL(18,2)
        ) AS MontoGravado,
        CAST(
            viatico.Monto * 0.20
            AS DECIMAL(18,2)
        ) AS MontoImpuestos
    FROM dbo.Viatico AS viatico
    WHERE viatico.IdViatico % 4 <> 0
    ORDER BY viatico.IdViatico;

    SET IDENTITY_INSERT dbo.Comprobante OFF;

    /*
        9. Auditoría representativa.
    */

    INSERT INTO dbo.Auditoria
    (
        FechaHora,
        IdUsuario,
        NombreUsuario,
        Modulo,
        Accion,
        Entidad,
        IdEntidad,
        Descripcion
    )
    SELECT
        DATEADD(
            DAY,
            cliente.IdCliente,
            CAST('2026-01-01T09:00:00' AS DATETIME2(0))
        ),
        @IdUsuarioActor,
        @NombreUsuarioActor,
        N'Clientes',
        CASE
            WHEN cliente.IdCliente IN (3, 6, 9)
                THEN N'Modificacion'
            ELSE N'Alta'
        END,
        N'Cliente',
        cliente.IdCliente,
        CONCAT(
            N'DEMO FINAL — Operación registrada sobre ',
            cliente.RazonSocial,
            N'.'
        )
    FROM dbo.Cliente AS cliente
    WHERE cliente.IdCliente BETWEEN 1 AND 10
    ORDER BY cliente.IdCliente;

    INSERT INTO dbo.Auditoria
    (
        FechaHora,
        IdUsuario,
        NombreUsuario,
        Modulo,
        Accion,
        Entidad,
        IdEntidad,
        Descripcion
    )
    SELECT
        DATEADD(
            HOUR,
            12,
            CAST(viaje.FechaInicio AS DATETIME2(0))
        ),
        @IdUsuarioActor,
        @NombreUsuarioActor,
        N'Viajes',
        CASE viaje.EstadoViaje
            WHEN 1 THEN N'Alta'
            WHEN 2 THEN N'EnvioRendicion'
            WHEN 3 THEN N'Aprobacion'
            WHEN 4 THEN N'Cancelacion'
        END,
        N'Viaje',
        viaje.IdViaje,
        CONCAT(
            N'DEMO FINAL — ',
            viaje.Descripcion,
            N'.'
        )
    FROM dbo.Viaje AS viaje
    ORDER BY viaje.IdViaje;

    INSERT INTO dbo.Auditoria
    (
        FechaHora,
        IdUsuario,
        NombreUsuario,
        Modulo,
        Accion,
        Entidad,
        IdEntidad,
        Descripcion
    )
    SELECT
        DATEADD(
            HOUR,
            15,
            CAST(visita.Fecha AS DATETIME2(0))
        ),
        @IdUsuarioActor,
        @NombreUsuarioActor,
        N'Visitas',
        N'Alta',
        N'Visita',
        visita.IdVisita,
        CONCAT(
            N'DEMO FINAL — Registro de Visita ',
            visita.IdVisita,
            N'.'
        )
    FROM dbo.Visita AS visita
    WHERE visita.IdVisita BETWEEN 1 AND 5
    ORDER BY visita.IdVisita;

    INSERT INTO dbo.Auditoria
    (
        FechaHora,
        IdUsuario,
        NombreUsuario,
        Modulo,
        Accion,
        Entidad,
        IdEntidad,
        Descripcion
    )
    SELECT
        DATEADD(
            HOUR,
            18,
            CAST(viatico.Fecha AS DATETIME2(0))
        ),
        @IdUsuarioActor,
        @NombreUsuarioActor,
        N'Viaticos',
        N'Alta',
        N'Viatico',
        viatico.IdViatico,
        CONCAT(
            N'DEMO FINAL — Registro de Viático ',
            viatico.IdViatico,
            N'.'
        )
    FROM dbo.Viatico AS viatico
    WHERE viatico.IdViatico BETWEEN 1 AND 5
    ORDER BY viatico.IdViatico;

    INSERT INTO dbo.AuditoriaCambio
    (
        IdAuditoria,
        Campo,
        ValorAnterior,
        ValorNuevo
    )
    SELECT
        auditoria.IdAuditoria,
        cambio.Campo,
        cambio.ValorAnterior,
        cambio.ValorNuevo
    FROM dbo.Auditoria AS auditoria
    CROSS APPLY
    (
        VALUES
            (
                N'Email',
                N'contacto.anterior@demo.local',
                N'contacto.actual@demo.local'
            ),
            (
                N'Telefono',
                N'341-555-0000',
                N'341-555-9999'
            )
    ) AS cambio
    (
        Campo,
        ValorAnterior,
        ValorNuevo
    )
    WHERE
        auditoria.Modulo = N'Clientes'
        AND auditoria.Accion = N'Modificacion'
        AND auditoria.IdEntidad IN (3, 6, 9);

    /*
        10. Validaciones antes del COMMIT.
    */

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Cliente
    ) <> 30
       OR
    (
        SELECT MIN(IdCliente)
        FROM dbo.Cliente
    ) <> 1
       OR
    (
        SELECT MAX(IdCliente)
        FROM dbo.Cliente
    ) <> 30
    BEGIN
        THROW 52311,
            'La validación de Clientes no coincide con 1 a 30.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Persona
        WHERE Email LIKE N'demo.final.%@sigevip.local'
    ) <> 8
    BEGIN
        THROW 52312,
            'La validación de empleados no coincide con 8.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Viaje
    ) <> 20
       OR
    (
        SELECT MIN(IdViaje)
        FROM dbo.Viaje
    ) <> 1
       OR
    (
        SELECT MAX(IdViaje)
        FROM dbo.Viaje
    ) <> 20
    BEGIN
        THROW 52313,
            'La validación de Viajes no coincide con 1 a 20.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Viaje
        WHERE EstadoViaje = 1
    ) <> 8
       OR
    (
        SELECT COUNT(*)
        FROM dbo.Viaje
        WHERE EstadoViaje = 2
    ) <> 5
       OR
    (
        SELECT COUNT(*)
        FROM dbo.Viaje
        WHERE EstadoViaje = 3
    ) <> 4
       OR
    (
        SELECT COUNT(*)
        FROM dbo.Viaje
        WHERE EstadoViaje = 4
    ) <> 3
    BEGIN
        THROW 52314,
            'La distribución de estados de Viaje es incorrecta.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Visita
    ) <> 45
       OR
    (
        SELECT MIN(IdVisita)
        FROM dbo.Visita
    ) <> 1
       OR
    (
        SELECT MAX(IdVisita)
        FROM dbo.Visita
    ) <> 45
    BEGIN
        THROW 52315,
            'La validación de Visitas no coincide con 1 a 45.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Viatico
    ) <> 60
       OR
    (
        SELECT MIN(IdViatico)
        FROM dbo.Viatico
    ) <> 1
       OR
    (
        SELECT MAX(IdViatico)
        FROM dbo.Viatico
    ) <> 60
    BEGIN
        THROW 52316,
            'La validación de Viáticos no coincide con 1 a 60.',
            1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Comprobante
    ) <> 45
       OR
    (
        SELECT MIN(IdComprobante)
        FROM dbo.Comprobante
    ) <> 1
       OR
    (
        SELECT MAX(IdComprobante)
        FROM dbo.Comprobante
    ) <> 45
    BEGIN
        THROW 52317,
            'La validación de Comprobantes no coincide con 1 a 45.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Visita AS visita
        INNER JOIN dbo.Viaje AS viaje
            ON viaje.IdViaje = visita.IdViaje
        WHERE
            visita.Fecha < viaje.FechaInicio
            OR visita.Fecha > viaje.FechaFin
    )
    BEGIN
        THROW 52318,
            'Existe una Visita fuera del período de su Viaje.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Viatico AS viatico
        INNER JOIN dbo.Viaje AS viaje
            ON viaje.IdViaje = viatico.IdViaje
        WHERE
            viatico.Fecha < viaje.FechaInicio
            OR viatico.Fecha > viaje.FechaFin
    )
    BEGIN
        THROW 52319,
            'Existe un Viático fuera del período de su Viaje.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Viatico AS viatico
        WHERE
            viatico.IdPersonaPagadora IS NOT NULL
            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.ViajeParticipante AS participante
                WHERE
                    participante.IdViaje = viatico.IdViaje
                    AND participante.IdPersona =
                        viatico.IdPersonaPagadora
            )
    )
    BEGIN
        THROW 52320,
            'Existe un pagador que no participa del Viaje.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Comprobante AS comprobante
        INNER JOIN dbo.Viatico AS viatico
            ON viatico.IdViatico = comprobante.IdViatico
        WHERE
            comprobante.MontoGravado
            + comprobante.MontoImpuestos
            <> viatico.Monto
    )
    BEGIN
        THROW 52321,
            'Existe un Comprobante cuyo total no coincide con el Viático.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Viaje AS viaje
        WHERE
            viaje.EstadoViaje = 4
            AND
            (
                EXISTS
                (
                    SELECT 1
                    FROM dbo.Visita AS visita
                    WHERE visita.IdViaje = viaje.IdViaje
                )
                OR EXISTS
                (
                    SELECT 1
                    FROM dbo.Viatico AS viatico
                    WHERE viatico.IdViaje = viaje.IdViaje
                )
            )
    )
    BEGIN
        THROW 52322,
            'Un Viaje cancelado contiene Visitas o Viáticos.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Viaje AS viaje
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.ViajeParticipante AS participante
            WHERE participante.IdViaje = viaje.IdViaje
        )
    )
    BEGIN
        THROW 52323,
            'Existe un Viaje sin participantes.',
            1;
    END;

    DBCC CHECKIDENT (N'dbo.Cliente', RESEED, 30)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Viaje', RESEED, 20)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Visita', RESEED, 45)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Viatico', RESEED, 60)
        WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Comprobante', RESEED, 45)
        WITH NO_INFOMSGS;

    COMMIT TRANSACTION;

    PRINT N'Dataset final aplicado correctamente.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Cliente OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Viaje OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Visita OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Viatico OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Comprobante OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
GO

/*
    11. Resumen final verificable.
*/

SELECT
    DB_NAME() AS BaseActual,
    (SELECT COUNT(*) FROM dbo.Cliente) AS Clientes,
    (
        SELECT COUNT(*)
        FROM dbo.Persona
        WHERE Email LIKE N'demo.final.%@sigevip.local'
    ) AS EmpleadosDemo,
    (SELECT COUNT(*) FROM dbo.Viaje) AS Viajes,
    (SELECT COUNT(*) FROM dbo.Visita) AS Visitas,
    (SELECT COUNT(*) FROM dbo.Viatico) AS Viaticos,
    (SELECT COUNT(*) FROM dbo.Comprobante) AS Comprobantes,
    (SELECT COUNT(*) FROM dbo.Auditoria) AS EventosAuditoria,
    (SELECT COUNT(*) FROM dbo.AuditoriaCambio) AS CambiosAuditoria;
GO

SELECT
    EstadoViaje,
    CASE EstadoViaje
        WHEN 1 THEN N'Abierto'
        WHEN 2 THEN N'En rendición'
        WHEN 3 THEN N'Aprobado'
        WHEN 4 THEN N'Cancelado'
    END AS Estado,
    COUNT(*) AS Cantidad
FROM dbo.Viaje
GROUP BY EstadoViaje
ORDER BY EstadoViaje;
GO

SELECT
    N'Cliente' AS Entidad,
    MIN(IdCliente) AS IdMinimo,
    MAX(IdCliente) AS IdMaximo,
    COUNT(*) AS Cantidad
FROM dbo.Cliente
UNION ALL
SELECT
    N'Viaje',
    MIN(IdViaje),
    MAX(IdViaje),
    COUNT(*)
FROM dbo.Viaje
UNION ALL
SELECT
    N'Visita',
    MIN(IdVisita),
    MAX(IdVisita),
    COUNT(*)
FROM dbo.Visita
UNION ALL
SELECT
    N'Viatico',
    MIN(IdViatico),
    MAX(IdViatico),
    COUNT(*)
FROM dbo.Viatico
UNION ALL
SELECT
    N'Comprobante',
    MIN(IdComprobante),
    MAX(IdComprobante),
    COUNT(*)
FROM dbo.Comprobante;
GO

SELECT
    N'DATASET FINAL CORRECTO' AS Resultado;
GO
