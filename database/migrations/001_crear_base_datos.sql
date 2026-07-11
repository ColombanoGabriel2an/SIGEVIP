USE master;
GO

IF DB_ID(N'SIGEVIP') IS NULL
BEGIN
    CREATE DATABASE SIGEVIP;
END;
GO

USE SIGEVIP;
GO

IF OBJECT_ID(N'dbo.VersionBaseDatos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VersionBaseDatos
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_VersionBaseDatos PRIMARY KEY,
        NumeroVersion NVARCHAR(20) NOT NULL,
        Descripcion NVARCHAR(200) NOT NULL,
        FechaAplicacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_VersionBaseDatos_FechaAplicacion
            DEFAULT SYSDATETIME()
    );

    INSERT INTO dbo.VersionBaseDatos
        (NumeroVersion, Descripcion)
    VALUES
        (N'001', N'Creación inicial de la base de datos SIGEVIP');
END;
GO

SELECT
    DB_NAME() AS BaseActual,
    NumeroVersion,
    Descripcion,
    FechaAplicacion
FROM dbo.VersionBaseDatos;
GO
