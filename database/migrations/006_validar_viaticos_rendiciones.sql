USE SIGEVIP;
GO

SET NOCOUNT ON;
GO

SELECT
    @@SERVERNAME AS Servidor,
    DB_NAME() AS BaseActual;
GO

IF OBJECT_ID(N'dbo.Viatico', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Comprobante', N'U') IS NULL
BEGIN
    THROW 50100,
        'La migración 006 no se encuentra aplicada completamente.',
        1;
END;
GO

/*
    1. Migración registrada.
*/

SELECT
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos
WHERE NumeroVersion = N'006';
GO

/*
    2. Existencia de tablas.
*/

SELECT
    CASE
        WHEN OBJECT_ID(
            N'dbo.Viatico',
            N'U'
        ) IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaViatico,

    CASE
        WHEN OBJECT_ID(
            N'dbo.Comprobante',
            N'U'
        ) IS NOT NULL
            THEN N'SÍ'
        ELSE N'NO'
    END AS ExisteTablaComprobante;
GO

/*
    3. Columnas nuevas de Viaje.
*/

SELECT
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,
    columna.max_length AS LongitudBytes,
    columna.precision AS Precision,
    columna.scale AS Escala,
    columna.is_nullable AS PermiteNulos
FROM sys.columns AS columna
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id =
        columna.user_type_id
WHERE
    columna.object_id =
        OBJECT_ID(N'dbo.Viaje')
    AND columna.name IN
    (
        N'IdUsuarioEnvioRendicion',
        N'FechaEnvioRendicion',
        N'IdUsuarioAprobador',
        N'FechaAprobacion',
        N'MotivoCancelacion',
        N'IdUsuarioCancelacion',
        N'FechaCancelacion'
    )
ORDER BY columna.column_id;
GO

/*
    4. Columnas de Viatico y Comprobante.
*/

SELECT
    tabla.name AS Tabla,
    columna.column_id AS Orden,
    columna.name AS Columna,
    tipo.name AS Tipo,

    CASE
        WHEN tipo.name IN
        (
            N'nvarchar',
            N'nchar',
            N'varchar',
            N'char'
        )
        THEN
            CASE
                WHEN tipo.name IN
                (
                    N'nvarchar',
                    N'nchar'
                )
                    THEN columna.max_length / 2
                ELSE columna.max_length
            END
        ELSE columna.max_length
    END AS Longitud,

    columna.precision AS Precision,
    columna.scale AS Escala,
    columna.is_nullable AS PermiteNulos,
    columna.is_identity AS EsIdentity
FROM sys.columns AS columna
INNER JOIN sys.tables AS tabla
    ON tabla.object_id =
        columna.object_id
INNER JOIN sys.types AS tipo
    ON tipo.user_type_id =
        columna.user_type_id
WHERE tabla.object_id IN
(
    OBJECT_ID(N'dbo.Viatico'),
    OBJECT_ID(N'dbo.Comprobante')
)
ORDER BY
    tabla.name,
    columna.column_id;
GO

/*
    5. Claves primarias e índices.
*/

SELECT
    tabla.name AS Tabla,
    indice.name AS Indice,
    indice.is_primary_key AS EsClavePrimaria,
    indice.is_unique AS EsUnico,
    indice.has_filter AS TieneFiltro,
    indice.filter_definition AS DefinicionFiltro,
    columnaIndice.key_ordinal AS OrdenColumna,
    columna.name AS Columna
FROM sys.indexes AS indice
INNER JOIN sys.tables AS tabla
    ON tabla.object_id =
        indice.object_id
INNER JOIN sys.index_columns AS columnaIndice
    ON columnaIndice.object_id =
        indice.object_id
    AND columnaIndice.index_id =
        indice.index_id
INNER JOIN sys.columns AS columna
    ON columna.object_id =
        columnaIndice.object_id
    AND columna.column_id =
        columnaIndice.column_id
WHERE
    indice.object_id IN
    (
        OBJECT_ID(N'dbo.Viatico'),
        OBJECT_ID(N'dbo.Comprobante')
    )
    AND
    (
        indice.is_primary_key = 1
        OR indice.name IN
        (
            N'IX_Viatico_IdViaje_Fecha',
            N'IX_Viatico_IdViaje_Estado',
            N'IX_Viatico_CategoriaGasto',
            N'IX_Viatico_IdPersonaPagadora',
            N'UX_Comprobante_IdViatico'
        )
    )
ORDER BY
    tabla.name,
    indice.name,
    columnaIndice.key_ordinal;
GO

/*
    6. Claves foráneas.
*/

SELECT
    clave.name AS ClaveForanea,

    OBJECT_SCHEMA_NAME(
        clave.parent_object_id
    ) +
    N'.' +
    OBJECT_NAME(
        clave.parent_object_id
    ) AS TablaOrigen,

    columnaOrigen.name AS ColumnaOrigen,

    OBJECT_SCHEMA_NAME(
        clave.referenced_object_id
    ) +
    N'.' +
    OBJECT_NAME(
        clave.referenced_object_id
    ) AS TablaDestino,

    columnaDestino.name AS ColumnaDestino,

    clave.delete_referential_action_desc
        AS AccionEliminacion,

    clave.is_disabled AS Deshabilitada,
    clave.is_not_trusted AS NoConfiable
FROM sys.foreign_keys AS clave
INNER JOIN sys.foreign_key_columns AS relacion
    ON relacion.constraint_object_id =
        clave.object_id
INNER JOIN sys.columns AS columnaOrigen
    ON columnaOrigen.object_id =
        relacion.parent_object_id
    AND columnaOrigen.column_id =
        relacion.parent_column_id
INNER JOIN sys.columns AS columnaDestino
    ON columnaDestino.object_id =
        relacion.referenced_object_id
    AND columnaDestino.column_id =
        relacion.referenced_column_id
WHERE clave.parent_object_id IN
(
    OBJECT_ID(N'dbo.Viaje'),
    OBJECT_ID(N'dbo.Viatico'),
    OBJECT_ID(N'dbo.Comprobante')
)
AND clave.name IN
(
    N'FK_Viaje_UsuarioEnvioRendicion',
    N'FK_Viaje_UsuarioAprobador',
    N'FK_Viaje_UsuarioCancelacion',
    N'FK_Viatico_Viaje',
    N'FK_Viatico_PersonaPagadora',
    N'FK_Viatico_UsuarioExclusion',
    N'FK_Viatico_UsuarioReactivacion',
    N'FK_Comprobante_Viatico'
)
ORDER BY clave.name;
GO

/*
    7. Restricciones CHECK.
*/

SELECT
    OBJECT_NAME(
        restriccion.parent_object_id
    ) AS Tabla,
    restriccion.name AS Restriccion,
    restriccion.definition AS Definicion,
    restriccion.is_disabled AS Deshabilitada,
    restriccion.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS restriccion
WHERE restriccion.parent_object_id IN
(
    OBJECT_ID(N'dbo.Viaje'),
    OBJECT_ID(N'dbo.Viatico'),
    OBJECT_ID(N'dbo.Comprobante')
)
AND
(
    restriccion.name LIKE
        N'CK_Viaje_Auditoria%'
    OR restriccion.name LIKE
        N'CK_Viatico_%'
    OR restriccion.name LIKE
        N'CK_Comprobante_%'
)
ORDER BY
    Tabla,
    Restriccion;
GO

/*
    8. Restricciones DEFAULT.
*/

SELECT
    OBJECT_NAME(
        restriccion.parent_object_id
    ) AS Tabla,
    restriccion.name AS Restriccion,
    columna.name AS Columna,
    restriccion.definition AS Definicion
FROM sys.default_constraints AS restriccion
INNER JOIN sys.columns AS columna
    ON columna.object_id =
        restriccion.parent_object_id
    AND columna.column_id =
        restriccion.parent_column_id
WHERE restriccion.parent_object_id =
    OBJECT_ID(N'dbo.Viatico')
ORDER BY restriccion.name;
GO

/*
    9. Correspondencias de enums.
*/

SELECT
    valor.CategoriaGasto,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'Combustible'),
        (CAST(2 AS TINYINT), N'Alojamiento'),
        (CAST(3 AS TINYINT), N'Alimentacion'),
        (CAST(4 AS TINYINT), N'Peaje'),
        (CAST(5 AS TINYINT), N'Estacionamiento'),
        (CAST(6 AS TINYINT), N'Transporte'),
        (CAST(7 AS TINYINT), N'Otros')
) AS valor
(
    CategoriaGasto,
    Nombre
);
GO

SELECT
    valor.MetodoPago,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'PagoPersonal'),
        (CAST(2 AS TINYINT), N'TarjetaCorporativa'),
        (CAST(3 AS TINYINT), N'EfectivoEmpresa')
) AS valor
(
    MetodoPago,
    Nombre
);
GO

SELECT
    valor.EstadoViatico,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'Vigente'),
        (CAST(2 AS TINYINT), N'Excluido')
) AS valor
(
    EstadoViatico,
    Nombre
);
GO

SELECT
    valor.TipoComprobante,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'FacturaA'),
        (CAST(2 AS TINYINT), N'FacturaB'),
        (CAST(3 AS TINYINT), N'FacturaC'),
        (CAST(4 AS TINYINT), N'Ticket'),
        (CAST(5 AS TINYINT), N'Recibo'),
        (CAST(6 AS TINYINT), N'Otro')
) AS valor
(
    TipoComprobante,
    Nombre
);
GO

SELECT
    valor.SituacionFiscal,
    valor.Nombre
FROM
(
    VALUES
        (CAST(1 AS TINYINT), N'ResponsableInscripto'),
        (CAST(2 AS TINYINT), N'Monotributista'),
        (CAST(3 AS TINYINT), N'Exento'),
        (CAST(4 AS TINYINT), N'ConsumidorFinal'),
        (CAST(5 AS TINYINT), N'NoInformada')
) AS valor
(
    SituacionFiscal,
    Nombre
);
GO

/*
    10. Cantidades generales.
*/

SELECT
    COUNT(*) AS CantidadViaticos
FROM dbo.Viatico;
GO

SELECT
    COUNT(*) AS CantidadComprobantes
FROM dbo.Comprobante;
GO

/*
    11. Viáticos sin Viaje.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadViaticosSinViaje
FROM dbo.Viatico AS viatico
LEFT JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
WHERE viaje.IdViaje IS NULL;
GO

/*
    12. Comprobantes sin Viático.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadComprobantesSinViatico
FROM dbo.Comprobante AS comprobante
LEFT JOIN dbo.Viatico AS viatico
    ON viatico.IdViatico =
        comprobante.IdViatico
WHERE viatico.IdViatico IS NULL;
GO

/*
    13. Más de un Comprobante por Viático.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadViaticosConComprobantesDuplicados
FROM
(
    SELECT
        IdViatico
    FROM dbo.Comprobante
    GROUP BY IdViatico
    HAVING COUNT(*) > 1
) AS duplicados;
GO

/*
    14. Métodos de pago incompatibles con pagador.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadViaticosConPagadorInconsistente
FROM dbo.Viatico
WHERE
    (
        MetodoPago IN
        (
            1,
            2
        )
        AND IdPersonaPagadora IS NULL
    )
    OR
    (
        MetodoPago = 3
        AND IdPersonaPagadora IS NOT NULL
    );
GO

/*
    15. Viáticos fuera del período del Viaje.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadViaticosFueraPeriodo
FROM dbo.Viatico AS viatico
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje =
        viatico.IdViaje
WHERE
    viatico.Fecha <
        viaje.FechaInicio
    OR viatico.Fecha >
        viaje.FechaFin;
GO

/*
    16. Viáticos excluidos sin auditoría completa.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadExcluidosSinAuditoria
FROM dbo.Viatico
WHERE
    EstadoViatico = 2
    AND
    (
        MotivoExclusion IS NULL
        OR LEN(
            LTRIM(
                RTRIM(
                    MotivoExclusion
                )
            )
        ) = 0
        OR IdUsuarioExclusion IS NULL
        OR FechaExclusion IS NULL
    );
GO

/*
    17. Auditorías incompletas en Viaje.
    Resultado esperado: cero.
*/

SELECT
    COUNT(*) AS CantidadViajesConAuditoriaIncompleta
FROM dbo.Viaje
WHERE
    (
        (
            IdUsuarioEnvioRendicion IS NULL
            AND FechaEnvioRendicion IS NOT NULL
        )
        OR
        (
            IdUsuarioEnvioRendicion IS NOT NULL
            AND FechaEnvioRendicion IS NULL
        )
    )
    OR
    (
        (
            IdUsuarioAprobador IS NULL
            AND FechaAprobacion IS NOT NULL
        )
        OR
        (
            IdUsuarioAprobador IS NOT NULL
            AND FechaAprobacion IS NULL
        )
    )
    OR
    (
        (
            MotivoCancelacion IS NULL
            AND
            (
                IdUsuarioCancelacion IS NOT NULL
                OR FechaCancelacion IS NOT NULL
            )
        )
        OR
        (
            MotivoCancelacion IS NOT NULL
            AND
            (
                LEN(
                    LTRIM(
                        RTRIM(
                            MotivoCancelacion
                        )
                    )
                ) = 0
                OR IdUsuarioCancelacion IS NULL
                OR FechaCancelacion IS NULL
            )
        )
    );
GO
