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
        CONSTRAINT CK_AprobacionSolicitud_Tipo CHECK (Tipo IN (''EVALUADOR'', ''COORDINADOR'', ''COORD_PRE'', ''COORD_FINAL''))
    );';
    EXEC sys.sp_executesql @sqlAprobacion;
END;

-- Retira cualquier versión anterior de la restricción; en algunas bases SQL Server
-- le asignó un nombre automático y por eso no bastaba con buscar el nombre conocido.
DECLARE @sqlEliminarChecks NVARCHAR(MAX) = N'';
SELECT @sqlEliminarChecks = @sqlEliminarChecks
    + N'ALTER TABLE dbo.AprobacionSolicitud DROP CONSTRAINT ' + QUOTENAME(cc.name) + N';'
FROM sys.check_constraints AS cc
WHERE cc.parent_object_id = OBJECT_ID(N'dbo.AprobacionSolicitud')
  AND cc.definition LIKE N'%Tipo%';
IF LEN(@sqlEliminarChecks) > 0
    EXEC sys.sp_executesql @sqlEliminarChecks;

-- Permite distinguir la preaprobación de la autorización final del segundo coordinador.
ALTER TABLE dbo.AprobacionSolicitud WITH CHECK ADD CONSTRAINT CK_AprobacionSolicitud_Tipo
    CHECK (Tipo IN ('EVALUADOR', 'COORDINADOR', 'COORD_PRE', 'COORD_FINAL'));

IF OBJECT_ID('dbo.RegistroAuditoria', 'U') IS NULL
BEGIN
    DECLARE @sqlAuditoria NVARCHAR(MAX) = N'
    CREATE TABLE dbo.RegistroAuditoria (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RegistroAuditoria PRIMARY KEY,
        IdSolicitud INT NULL,
        Entidad NVARCHAR(80) NOT NULL CONSTRAINT DF_RegistroAuditoria_Entidad DEFAULT (''SOLICITUD''),
        IdEntidad NVARCHAR(100) NULL,
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

-- La auditoría también cubre cambios relevantes fuera de una solicitud.
IF COL_LENGTH('dbo.RegistroAuditoria', 'Entidad') IS NULL
    ALTER TABLE dbo.RegistroAuditoria ADD Entidad NVARCHAR(80) NOT NULL
        CONSTRAINT DF_RegistroAuditoria_Entidad DEFAULT ('SOLICITUD');
IF COL_LENGTH('dbo.RegistroAuditoria', 'IdEntidad') IS NULL
    ALTER TABLE dbo.RegistroAuditoria ADD IdEntidad NVARCHAR(100) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RegistroAuditoria') AND name = 'IdSolicitud' AND is_nullable = 0)
BEGIN
    DECLARE @FkAuditoria SYSNAME;
    SELECT @FkAuditoria = fk.name FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.RegistroAuditoria') AND c.name = 'IdSolicitud';
    IF @FkAuditoria IS NOT NULL
    BEGIN
        DECLARE @SqlEliminarFk NVARCHAR(MAX) = N'ALTER TABLE dbo.RegistroAuditoria DROP CONSTRAINT ' + QUOTENAME(@FkAuditoria);
        EXEC sys.sp_executesql @SqlEliminarFk;
    END;
    ALTER TABLE dbo.RegistroAuditoria ALTER COLUMN IdSolicitud INT NULL;
    DECLARE @FkSql NVARCHAR(MAX) = N'ALTER TABLE dbo.RegistroAuditoria WITH CHECK ADD CONSTRAINT FK_RegistroAuditoria_Solicitud FOREIGN KEY (IdSolicitud) REFERENCES dbo.' + QUOTENAME(@TablaSolicitud) + N'(Id);';
    EXEC sys.sp_executesql @FkSql;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.RegistroAuditoria') AND name = 'IX_RegistroAuditoria_Entidad_Fecha')
    CREATE INDEX IX_RegistroAuditoria_Entidad_Fecha ON dbo.RegistroAuditoria(Entidad, IdEntidad, FechaHoraUtc);
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
