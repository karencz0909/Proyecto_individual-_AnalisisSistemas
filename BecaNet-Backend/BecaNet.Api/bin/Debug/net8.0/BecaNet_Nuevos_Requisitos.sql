/* Ejecutar una vez sobre la base existente antes de iniciar la API actualizada. */
DECLARE @TablaSolicitud SYSNAME;
IF OBJECT_ID('dbo.Solicitudes', 'U') IS NOT NULL SET @TablaSolicitud = 'Solicitudes';
ELSE IF OBJECT_ID('dbo.Solicitud', 'U') IS NOT NULL SET @TablaSolicitud = 'Solicitud';
ELSE
BEGIN
    ;THROW 51010, 'No se encontró la tabla Solicitud/Solicitudes.', 1;
END;

DECLARE @ColumnaMonto NVARCHAR(300) = N'dbo.' + QUOTENAME(@TablaSolicitud);
IF COL_LENGTH(@ColumnaMonto, 'MontoSolicitado') IS NULL
BEGIN
    DECLARE @sqlMonto NVARCHAR(MAX) = N'ALTER TABLE dbo.' + QUOTENAME(@TablaSolicitud) +
        N' ADD MontoSolicitado DECIMAL(12,2) NOT NULL CONSTRAINT DF_Solicitud_MontoSolicitado DEFAULT (0);';
    EXEC sys.sp_executesql @sqlMonto;
END;
GO

DECLARE @TablaSolicitud SYSNAME;
IF OBJECT_ID('dbo.Solicitudes', 'U') IS NOT NULL SET @TablaSolicitud = 'Solicitudes';
ELSE SET @TablaSolicitud = 'Solicitud';

IF OBJECT_ID('dbo.AprobacionSolicitud', 'U') IS NULL
BEGIN
    DECLARE @sqlAprobacion NVARCHAR(MAX) = N'
    CREATE TABLE dbo.AprobacionSolicitud (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AprobacionSolicitud PRIMARY KEY,
        IdSolicitud INT NOT NULL,
        Tipo VARCHAR(20) NOT NULL,
        IdUsuario INT NOT NULL,
        RolUsuario VARCHAR(30) NOT NULL,
        Aprobada BIT NOT NULL,
        Motivo NVARCHAR(1000) NULL,
        FechaHoraUtc DATETIME2 NOT NULL CONSTRAINT DF_AprobacionSolicitud_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AprobacionSolicitud_Solicitud FOREIGN KEY (IdSolicitud)
          REFERENCES dbo.' + QUOTENAME(@TablaSolicitud) + N'(Id),
        CONSTRAINT UQ_AprobacionSolicitud_Tipo UNIQUE (IdSolicitud, Tipo),
        CONSTRAINT CK_AprobacionSolicitud_Tipo CHECK (Tipo IN (''EVALUADOR'', ''COORDINADOR''))
    );';
    EXEC sys.sp_executesql @sqlAprobacion;
END;

IF OBJECT_ID('dbo.RegistroAuditoria', 'U') IS NULL
BEGIN
    DECLARE @sqlAuditoria NVARCHAR(MAX) = N'
    CREATE TABLE dbo.RegistroAuditoria (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RegistroAuditoria PRIMARY KEY,
        IdSolicitud INT NOT NULL,
        IdUsuario INT NOT NULL,
        RolUsuario VARCHAR(30) NOT NULL,
        Operacion VARCHAR(80) NOT NULL,
        Detalle NVARCHAR(2000) NULL,
        FechaHoraUtc DATETIME2 NOT NULL CONSTRAINT DF_RegistroAuditoria_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_RegistroAuditoria_Solicitud FOREIGN KEY (IdSolicitud)
          REFERENCES dbo.' + QUOTENAME(@TablaSolicitud) + N'(Id)
    );
    CREATE INDEX IX_RegistroAuditoria_Solicitud_Fecha ON dbo.RegistroAuditoria(IdSolicitud, FechaHoraUtc);';
    EXEC sys.sp_executesql @sqlAuditoria;
END;

IF OBJECT_ID('dbo.NotificacionSolicitud', 'U') IS NULL
BEGIN
    DECLARE @sqlNotificacion NVARCHAR(MAX) = N'
    CREATE TABLE dbo.NotificacionSolicitud (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificacionSolicitud PRIMARY KEY,
        IdSolicitud INT NOT NULL,
        IdEstudiante INT NOT NULL,
        EstadoAnterior VARCHAR(20) NOT NULL,
        EstadoNuevo VARCHAR(20) NOT NULL,
        Evento VARCHAR(60) NOT NULL CONSTRAINT DF_NotificacionSolicitud_Evento DEFAULT (''CAMBIO_ESTADO_SOLICITUD''),
        FechaHoraUtc DATETIME2 NOT NULL CONSTRAINT DF_NotificacionSolicitud_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_NotificacionSolicitud_Solicitud FOREIGN KEY (IdSolicitud)
          REFERENCES dbo.' + QUOTENAME(@TablaSolicitud) + N'(Id)
    );
    CREATE INDEX IX_NotificacionSolicitud_Estudiante_Fecha ON dbo.NotificacionSolicitud(IdEstudiante, FechaHoraUtc);';
    EXEC sys.sp_executesql @sqlNotificacion;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_RegistroAuditoria_Inmutable
ON dbo.RegistroAuditoria AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    ;THROW 51001, 'Los registros de auditoría no se pueden modificar ni eliminar.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_AprobacionSolicitud_Inmutable
ON dbo.AprobacionSolicitud AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    ;THROW 51002, 'Las decisiones de aprobación no se pueden modificar ni eliminar.', 1;
END;
GO
