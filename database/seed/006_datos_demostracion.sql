USE SIGEVIP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    Dataset idempotente de demostración para SIGEVIP.

    Finalidad:
    - mostrar Clientes activos e inactivos;
    - mostrar Viajes en los cuatro estados;
    - mostrar participantes;
    - mostrar Visitas relacionadas con uno o varios Clientes;
    - mostrar Viáticos vigentes y excluidos;
    - permitir filtros y consultas durante la exposición.

    Este archivo inserta datos técnicos de demostración directamente en SQL.
    No representa una operación normal realizada por un Usuario desde WinForms.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Persona', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Viaje', N'U') IS NULL
       OR OBJECT_ID(N'dbo.ViajeParticipante', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Visita', N'U') IS NULL
       OR OBJECT_ID(N'dbo.VisitaCliente', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Viatico', N'U') IS NULL
    BEGIN
        THROW 52001,
            'La base no contiene todas las tablas requeridas. Ejecute primero las migraciones 001 a 008.',
            1;
    END;

    DECLARE @IdUsuarioActor INT;

    SELECT TOP (1)
        @IdUsuarioActor = usuario.IdUsuario
    FROM dbo.Usuario AS usuario
    WHERE usuario.Activo = 1
    ORDER BY
        CASE
            WHEN usuario.NombreUsuario = N'gabriel'
                THEN 0
            ELSE 1
        END,
        usuario.IdUsuario;

    IF @IdUsuarioActor IS NULL
    BEGIN
        THROW 52002,
            'No existe un Usuario activo para asociar los estados de demostración.',
            1;
    END;

    /*
        1. Personas de demostración.
    */

    DECLARE @Personas TABLE
    (
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Email NVARCHAR(254) NOT NULL PRIMARY KEY,
        Activo BIT NOT NULL
    );

    INSERT INTO @Personas
    (
        Nombre,
        Apellido,
        Email,
        Activo
    )
    VALUES
        (N'Lucía', N'Fernández', N'demo.lucia@sigevip.local', 1),
        (N'Martín', N'Pereyra', N'demo.martin@sigevip.local', 1),
        (N'Sofía', N'Gómez', N'demo.sofia@sigevip.local', 1);

    MERGE dbo.Persona WITH (HOLDLOCK) AS destino
    USING @Personas AS origen
        ON destino.Email = origen.Email
    WHEN MATCHED THEN
        UPDATE SET
            Nombre = origen.Nombre,
            Apellido = origen.Apellido,
            Activo = origen.Activo
    WHEN NOT MATCHED THEN
        INSERT
        (
            Nombre,
            Apellido,
            Email,
            Activo
        )
        VALUES
        (
            origen.Nombre,
            origen.Apellido,
            origen.Email,
            origen.Activo
        );

    DECLARE @IdLucia INT =
    (
        SELECT IdPersona
        FROM dbo.Persona
        WHERE Email = N'demo.lucia@sigevip.local'
    );

    DECLARE @IdMartin INT =
    (
        SELECT IdPersona
        FROM dbo.Persona
        WHERE Email = N'demo.martin@sigevip.local'
    );

    DECLARE @IdSofia INT =
    (
        SELECT IdPersona
        FROM dbo.Persona
        WHERE Email = N'demo.sofia@sigevip.local'
    );

    /*
        2. Clientes de demostración.
    */

    DECLARE @Clientes TABLE
    (
        RazonSocial NVARCHAR(150) NOT NULL,
        Cuit NVARCHAR(20) NOT NULL PRIMARY KEY,
        Email NVARCHAR(150) NULL,
        Telefono NVARCHAR(50) NULL,
        Localidad NVARCHAR(100) NULL,
        Provincia NVARCHAR(100) NULL,
        Activo BIT NOT NULL
    );

    INSERT INTO @Clientes
    (
        RazonSocial,
        Cuit,
        Email,
        Telefono,
        Localidad,
        Provincia,
        Activo
    )
    VALUES
        (
            N'Agropecuaria Los Aromos SA',
            N'30-70000001-1',
            N'contacto@losaromos.demo',
            N'341-555-1001',
            N'Funes',
            N'Santa Fe',
            1
        ),
        (
            N'Campos del Sur SRL',
            N'30-70000002-2',
            N'administracion@camposdelsur.demo',
            N'341-555-1002',
            N'Roldán',
            N'Santa Fe',
            1
        ),
        (
            N'Cooperativa Horizonte',
            N'30-70000003-3',
            N'info@horizonte.demo',
            N'3464-555-103',
            N'Casilda',
            N'Santa Fe',
            1
        ),
        (
            N'Estancia La Esperanza',
            N'30-70000004-4',
            N'compras@laesperanza.demo',
            N'3476-555-104',
            N'Totoras',
            N'Santa Fe',
            1
        ),
        (
            N'Productores Unidos SA',
            N'30-70000005-5',
            N'contacto@productoresunidos.demo',
            N'3402-555-105',
            N'Las Rosas',
            N'Santa Fe',
            1
        ),
        (
            N'Agroinsumos del Litoral',
            N'30-70000006-6',
            N'ventas@agrolitoral.demo',
            N'341-555-1006',
            N'Rosario',
            N'Santa Fe',
            1
        ),
        (
            N'Semillería Regional',
            N'30-70000007-7',
            N'consultas@semilleriaregional.demo',
            N'341-555-1007',
            N'Pérez',
            N'Santa Fe',
            0
        ),
        (
            N'Ganadera El Ombú',
            N'30-70000008-8',
            N'administracion@elombu.demo',
            N'3462-555-108',
            N'Firmat',
            N'Santa Fe',
            0
        );

    MERGE dbo.Cliente WITH (HOLDLOCK) AS destino
    USING @Clientes AS origen
        ON destino.Cuit = origen.Cuit
    WHEN MATCHED THEN
        UPDATE SET
            RazonSocial = origen.RazonSocial,
            Email = origen.Email,
            Telefono = origen.Telefono,
            Localidad = origen.Localidad,
            Provincia = origen.Provincia,
            Activo = origen.Activo
    WHEN NOT MATCHED THEN
        INSERT
        (
            RazonSocial,
            Cuit,
            Email,
            Telefono,
            Localidad,
            Provincia,
            Activo
        )
        VALUES
        (
            origen.RazonSocial,
            origen.Cuit,
            origen.Email,
            origen.Telefono,
            origen.Localidad,
            origen.Provincia,
            origen.Activo
        );

    DECLARE @IdClienteAromos INT =
    (
        SELECT IdCliente
        FROM dbo.Cliente
        WHERE Cuit = N'30-70000001-1'
    );

    DECLARE @IdClienteCampos INT =
    (
        SELECT IdCliente
        FROM dbo.Cliente
        WHERE Cuit = N'30-70000002-2'
    );

    DECLARE @IdClienteHorizonte INT =
    (
        SELECT IdCliente
        FROM dbo.Cliente
        WHERE Cuit = N'30-70000003-3'
    );

    DECLARE @IdClienteEsperanza INT =
    (
        SELECT IdCliente
        FROM dbo.Cliente
        WHERE Cuit = N'30-70000004-4'
    );

    DECLARE @IdClienteProductores INT =
    (
        SELECT IdCliente
        FROM dbo.Cliente
        WHERE Cuit = N'30-70000005-5'
    );

    /*
        3. Viajes en los cuatro estados.

        TipoViaje:
        1 = Desplazamiento
        2 = EnOficina
        3 = EventoFeria

        EstadoViaje:
        1 = Abierto
        2 = EnRendicion
        3 = Aprobado
        4 = Cancelado
    */

    DECLARE @Viajes TABLE
    (
        Descripcion NVARCHAR(500) NOT NULL PRIMARY KEY,
        FechaInicio DATE NOT NULL,
        FechaFin DATE NOT NULL,
        TipoViaje TINYINT NOT NULL,
        MontoAnticipado DECIMAL(18,2) NOT NULL,
        EstadoViaje TINYINT NOT NULL,
        IdUsuarioEnvioRendicion INT NULL,
        FechaEnvioRendicion DATETIME2(0) NULL,
        IdUsuarioAprobador INT NULL,
        FechaAprobacion DATETIME2(0) NULL,
        MotivoCancelacion NVARCHAR(500) NULL,
        IdUsuarioCancelacion INT NULL,
        FechaCancelacion DATETIME2(0) NULL
    );

    INSERT INTO @Viajes
    (
        Descripcion,
        FechaInicio,
        FechaFin,
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
        (
            N'DEMO — Gira comercial Rosario y Funes — ABIERTO',
            '2026-07-01',
            '2026-07-03',
            1,
            120000.00,
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            N'DEMO — Visitas zona Casilda — EN RENDICIÓN',
            '2026-06-10',
            '2026-06-12',
            1,
            95000.00,
            2,
            @IdUsuarioActor,
            '2026-06-13T09:30:00',
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            N'DEMO — Feria regional de productores — APROBADO',
            '2026-05-05',
            '2026-05-06',
            3,
            150000.00,
            3,
            @IdUsuarioActor,
            '2026-05-07T10:00:00',
            @IdUsuarioActor,
            '2026-05-08T16:15:00',
            NULL,
            NULL,
            NULL
        ),
        (
            N'DEMO — Reunión comercial suspendida — CANCELADO',
            '2026-08-10',
            '2026-08-11',
            2,
            0.00,
            4,
            NULL,
            NULL,
            NULL,
            NULL,
            N'Actividad suspendida por reprogramación del cliente.',
            @IdUsuarioActor,
            '2026-07-28T18:00:00'
        );

    MERGE dbo.Viaje WITH (HOLDLOCK) AS destino
    USING @Viajes AS origen
        ON destino.Descripcion = origen.Descripcion
    WHEN MATCHED THEN
        UPDATE SET
            FechaInicio = origen.FechaInicio,
            FechaFin = origen.FechaFin,
            TipoViaje = origen.TipoViaje,
            MontoAnticipado = origen.MontoAnticipado,
            EstadoViaje = origen.EstadoViaje,
            IdUsuarioEnvioRendicion =
                origen.IdUsuarioEnvioRendicion,
            FechaEnvioRendicion =
                origen.FechaEnvioRendicion,
            IdUsuarioAprobador =
                origen.IdUsuarioAprobador,
            FechaAprobacion =
                origen.FechaAprobacion,
            MotivoCancelacion =
                origen.MotivoCancelacion,
            IdUsuarioCancelacion =
                origen.IdUsuarioCancelacion,
            FechaCancelacion =
                origen.FechaCancelacion
    WHEN NOT MATCHED THEN
        INSERT
        (
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
        (
            origen.FechaInicio,
            origen.FechaFin,
            origen.Descripcion,
            origen.TipoViaje,
            origen.MontoAnticipado,
            origen.EstadoViaje,
            origen.IdUsuarioEnvioRendicion,
            origen.FechaEnvioRendicion,
            origen.IdUsuarioAprobador,
            origen.FechaAprobacion,
            origen.MotivoCancelacion,
            origen.IdUsuarioCancelacion,
            origen.FechaCancelacion
        );

    DECLARE @IdViajeAbierto INT =
    (
        SELECT IdViaje
        FROM dbo.Viaje
        WHERE Descripcion =
            N'DEMO — Gira comercial Rosario y Funes — ABIERTO'
    );

    DECLARE @IdViajeRendicion INT =
    (
        SELECT IdViaje
        FROM dbo.Viaje
        WHERE Descripcion =
            N'DEMO — Visitas zona Casilda — EN RENDICIÓN'
    );

    DECLARE @IdViajeAprobado INT =
    (
        SELECT IdViaje
        FROM dbo.Viaje
        WHERE Descripcion =
            N'DEMO — Feria regional de productores — APROBADO'
    );

    DECLARE @IdViajeCancelado INT =
    (
        SELECT IdViaje
        FROM dbo.Viaje
        WHERE Descripcion =
            N'DEMO — Reunión comercial suspendida — CANCELADO'
    );

    /*
        4. Participantes.
    */

    DECLARE @Participaciones TABLE
    (
        IdViaje INT NOT NULL,
        IdPersona INT NOT NULL,
        PRIMARY KEY
        (
            IdViaje,
            IdPersona
        )
    );

    INSERT INTO @Participaciones
    (
        IdViaje,
        IdPersona
    )
    VALUES
        (@IdViajeAbierto, @IdLucia),
        (@IdViajeAbierto, @IdMartin),
        (@IdViajeRendicion, @IdMartin),
        (@IdViajeRendicion, @IdSofia),
        (@IdViajeAprobado, @IdLucia),
        (@IdViajeAprobado, @IdSofia),
        (@IdViajeCancelado, @IdMartin);

    INSERT INTO dbo.ViajeParticipante
    (
        IdViaje,
        IdPersona
    )
    SELECT
        origen.IdViaje,
        origen.IdPersona
    FROM @Participaciones AS origen
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ViajeParticipante AS destino
        WHERE destino.IdViaje = origen.IdViaje
          AND destino.IdPersona = origen.IdPersona
    );

    /*
        5. Visitas.
    */

    DECLARE @Visitas TABLE
    (
        IdViaje INT NOT NULL,
        Fecha DATE NOT NULL,
        Observacion NVARCHAR(1000) NOT NULL,
        LocalidadEncuentro NVARCHAR(150) NOT NULL,
        PRIMARY KEY
        (
            IdViaje,
            Observacion
        )
    );

    INSERT INTO @Visitas
    (
        IdViaje,
        Fecha,
        Observacion,
        LocalidadEncuentro
    )
    VALUES
        (
            @IdViajeAbierto,
            '2026-07-01',
            N'DEMO — Presentación de propuesta comercial y relevamiento de necesidades.',
            N'Rosario'
        ),
        (
            @IdViajeAbierto,
            '2026-07-02',
            N'DEMO — Seguimiento conjunto con dos clientes de la zona.',
            N'Funes'
        ),
        (
            @IdViajeRendicion,
            '2026-06-11',
            N'DEMO — Reunión de seguimiento de campaña y condiciones comerciales.',
            N'Casilda'
        ),
        (
            @IdViajeAprobado,
            '2026-05-05',
            N'DEMO — Participación en feria y entrevistas con productores.',
            N'Las Rosas'
        );

    MERGE dbo.Visita WITH (HOLDLOCK) AS destino
    USING @Visitas AS origen
        ON destino.IdViaje = origen.IdViaje
       AND destino.Observacion = origen.Observacion
    WHEN MATCHED THEN
        UPDATE SET
            Fecha = origen.Fecha,
            LocalidadEncuentro =
                origen.LocalidadEncuentro
    WHEN NOT MATCHED THEN
        INSERT
        (
            IdViaje,
            Fecha,
            Observacion,
            LocalidadEncuentro
        )
        VALUES
        (
            origen.IdViaje,
            origen.Fecha,
            origen.Observacion,
            origen.LocalidadEncuentro
        );

    DECLARE @IdVisitaAbierta1 INT =
    (
        SELECT IdVisita
        FROM dbo.Visita
        WHERE IdViaje = @IdViajeAbierto
          AND Observacion =
            N'DEMO — Presentación de propuesta comercial y relevamiento de necesidades.'
    );

    DECLARE @IdVisitaAbierta2 INT =
    (
        SELECT IdVisita
        FROM dbo.Visita
        WHERE IdViaje = @IdViajeAbierto
          AND Observacion =
            N'DEMO — Seguimiento conjunto con dos clientes de la zona.'
    );

    DECLARE @IdVisitaRendicion INT =
    (
        SELECT IdVisita
        FROM dbo.Visita
        WHERE IdViaje = @IdViajeRendicion
          AND Observacion =
            N'DEMO — Reunión de seguimiento de campaña y condiciones comerciales.'
    );

    DECLARE @IdVisitaAprobada INT =
    (
        SELECT IdVisita
        FROM dbo.Visita
        WHERE IdViaje = @IdViajeAprobado
          AND Observacion =
            N'DEMO — Participación en feria y entrevistas con productores.'
    );

    DECLARE @VisitaClientes TABLE
    (
        IdVisita INT NOT NULL,
        IdCliente INT NOT NULL,
        PRIMARY KEY
        (
            IdVisita,
            IdCliente
        )
    );

    INSERT INTO @VisitaClientes
    (
        IdVisita,
        IdCliente
    )
    VALUES
        (@IdVisitaAbierta1, @IdClienteAromos),
        (@IdVisitaAbierta2, @IdClienteCampos),
        (@IdVisitaAbierta2, @IdClienteEsperanza),
        (@IdVisitaRendicion, @IdClienteHorizonte),
        (@IdVisitaAprobada, @IdClienteProductores),
        (@IdVisitaAprobada, @IdClienteAromos);

    INSERT INTO dbo.VisitaCliente
    (
        IdVisita,
        IdCliente
    )
    SELECT
        origen.IdVisita,
        origen.IdCliente
    FROM @VisitaClientes AS origen
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.VisitaCliente AS destino
        WHERE destino.IdVisita = origen.IdVisita
          AND destino.IdCliente = origen.IdCliente
    );

    /*
        6. Viáticos.

        CategoriaGasto:
        1 Combustible
        2 Alojamiento
        3 Alimentación
        4 Peaje
        5 Estacionamiento
        6 Transporte
        7 Otros

        MetodoPago:
        1 PagoPersonal
        2 TarjetaCorporativa
        3 EfectivoEmpresa

        EstadoViatico:
        1 Vigente
        2 Excluido
    */

    DECLARE @Viaticos TABLE
    (
        IdViaje INT NOT NULL,
        Fecha DATE NOT NULL,
        CategoriaGasto TINYINT NOT NULL,
        MetodoPago TINYINT NOT NULL,
        IdPersonaPagadora INT NULL,
        Monto DECIMAL(18,2) NOT NULL,
        Descripcion NVARCHAR(1000) NOT NULL,
        EstadoViatico TINYINT NOT NULL,
        MotivoExclusion NVARCHAR(500) NULL,
        IdUsuarioExclusion INT NULL,
        FechaExclusion DATETIME2(0) NULL,
        IdUsuarioReactivacion INT NULL,
        FechaReactivacion DATETIME2(0) NULL,
        PRIMARY KEY
        (
            IdViaje,
            Descripcion
        )
    );

    INSERT INTO @Viaticos
    (
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
    VALUES
        (
            @IdViajeAbierto,
            '2026-07-01',
            1,
            2,
            @IdLucia,
            42000.00,
            N'DEMO — Combustible gira Rosario-Funes.',
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            @IdViajeAbierto,
            '2026-07-02',
            3,
            1,
            @IdMartin,
            18500.00,
            N'DEMO — Almuerzo durante visitas comerciales.',
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            @IdViajeRendicion,
            '2026-06-10',
            2,
            2,
            @IdSofia,
            68000.00,
            N'DEMO — Alojamiento zona Casilda.',
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            @IdViajeRendicion,
            '2026-06-11',
            7,
            1,
            @IdMartin,
            12500.00,
            N'DEMO — Gasto observado sin relación con el viaje.',
            2,
            N'El gasto no corresponde al objetivo comercial del viaje.',
            @IdUsuarioActor,
            '2026-06-13T11:00:00',
            NULL,
            NULL
        ),
        (
            @IdViajeAprobado,
            '2026-05-05',
            6,
            3,
            NULL,
            55000.00,
            N'DEMO — Transporte y montaje para feria regional.',
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        ),
        (
            @IdViajeAprobado,
            '2026-05-06',
            3,
            1,
            @IdLucia,
            28000.00,
            N'DEMO — Alimentación durante feria regional.',
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL
        );

    MERGE dbo.Viatico WITH (HOLDLOCK) AS destino
    USING @Viaticos AS origen
        ON destino.IdViaje = origen.IdViaje
       AND destino.Descripcion = origen.Descripcion
    WHEN MATCHED THEN
        UPDATE SET
            Fecha = origen.Fecha,
            CategoriaGasto = origen.CategoriaGasto,
            MetodoPago = origen.MetodoPago,
            IdPersonaPagadora =
                origen.IdPersonaPagadora,
            Monto = origen.Monto,
            EstadoViatico =
                origen.EstadoViatico,
            MotivoExclusion =
                origen.MotivoExclusion,
            IdUsuarioExclusion =
                origen.IdUsuarioExclusion,
            FechaExclusion =
                origen.FechaExclusion,
            IdUsuarioReactivacion =
                origen.IdUsuarioReactivacion,
            FechaReactivacion =
                origen.FechaReactivacion
    WHEN NOT MATCHED THEN
        INSERT
        (
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
        VALUES
        (
            origen.IdViaje,
            origen.Fecha,
            origen.CategoriaGasto,
            origen.MetodoPago,
            origen.IdPersonaPagadora,
            origen.Monto,
            origen.Descripcion,
            origen.EstadoViatico,
            origen.MotivoExclusion,
            origen.IdUsuarioExclusion,
            origen.FechaExclusion,
            origen.IdUsuarioReactivacion,
            origen.FechaReactivacion
        );

    COMMIT TRANSACTION;

    PRINT N'Datos de demostración aplicados correctamente.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO

/*
    7. Resumen verificable.
*/

SELECT
    IdCliente,
    RazonSocial,
    Cuit,
    Localidad,
    Activo
FROM dbo.Cliente
WHERE Cuit LIKE N'30-7000000%'
ORDER BY Activo DESC, RazonSocial;
GO

SELECT
    viaje.IdViaje,
    viaje.Descripcion,
    viaje.FechaInicio,
    viaje.FechaFin,
    viaje.MontoAnticipado,
    CASE viaje.EstadoViaje
        WHEN 1 THEN N'Abierto'
        WHEN 2 THEN N'EnRendicion'
        WHEN 3 THEN N'Aprobado'
        WHEN 4 THEN N'Cancelado'
    END AS Estado,
    COUNT(DISTINCT participante.IdPersona)
        AS Participantes,
    COUNT(DISTINCT visita.IdVisita)
        AS Visitas,
    COUNT(DISTINCT viatico.IdViatico)
        AS Viaticos
FROM dbo.Viaje AS viaje
LEFT JOIN dbo.ViajeParticipante AS participante
    ON participante.IdViaje = viaje.IdViaje
LEFT JOIN dbo.Visita AS visita
    ON visita.IdViaje = viaje.IdViaje
LEFT JOIN dbo.Viatico AS viatico
    ON viatico.IdViaje = viaje.IdViaje
WHERE viaje.Descripcion LIKE N'DEMO —%'
GROUP BY
    viaje.IdViaje,
    viaje.Descripcion,
    viaje.FechaInicio,
    viaje.FechaFin,
    viaje.MontoAnticipado,
    viaje.EstadoViaje
ORDER BY viaje.EstadoViaje;
GO

SELECT
    viaje.Descripcion AS Viaje,
    viatico.Descripcion AS Viatico,
    viatico.Monto,
    CASE viatico.EstadoViatico
        WHEN 1 THEN N'Vigente'
        WHEN 2 THEN N'Excluido'
    END AS EstadoViatico
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje = viatico.IdViaje
WHERE viaje.Descripcion LIKE N'DEMO —%'
ORDER BY viaje.EstadoViaje, viatico.Fecha, viatico.IdViatico;
GO
