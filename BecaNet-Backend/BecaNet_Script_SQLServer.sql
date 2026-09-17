/* ============================================================================
   BecaNet - Plataforma Nacional para la Gestión Integral de Becas
   Script de creación de Base de Datos - Modelo Relacional
   Motor: Microsoft SQL Server (Express Edition / LocalDB)
   Universidad Mariano Gálvez - Análisis de Sistemas II
   ============================================================================ */

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BecaNetDB')
BEGIN
    CREATE DATABASE BecaNetDB;
END
GO

USE BecaNetDB;
GO

-- ============================================================================
-- 1. USUARIO (Tabla base)
-- ============================================================================
CREATE TABLE Usuario (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          VARCHAR(150)  NOT NULL,
    Correo          VARCHAR(150)  NOT NULL UNIQUE,
    Contrasena      VARCHAR(255)  NOT NULL,
    Telefono        VARCHAR(20)   NULL,
    FechaRegistro   DATETIME      NOT NULL DEFAULT GETDATE(),
    TipoUsuario     VARCHAR(20)   NOT NULL
        CHECK (TipoUsuario IN ('ESTUDIANTE', 'COORDINADOR', 'EVALUADOR'))
);
GO

-- ============================================================================
-- 2. TABLAS FÍSICAS DE ROLES (Con prefijo tbl_ para convivir con Vistas)
-- ============================================================================
CREATE TABLE tbl_Estudiante (
    IdUsuario               INT PRIMARY KEY,
    NivelAcademico          VARCHAR(50)   NOT NULL,
    InstitucionProcedencia  VARCHAR(150)  NULL,
    CONSTRAINT FK_Estudiante_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
GO

CREATE TABLE tbl_CoordinadorBecas (
    IdUsuario   INT PRIMARY KEY,
    Cargo       VARCHAR(100)  NULL,
    CONSTRAINT FK_Coordinador_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
GO

CREATE TABLE tbl_EvaluadorComite (
    IdUsuario     INT PRIMARY KEY,
    Especialidad  VARCHAR(100)  NULL,
    CONSTRAINT FK_Evaluador_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 3. VISTAS UNIFICADAS PARA ENTITY FRAMEWORK CORE
-- ============================================================================

-- Vistas Estudiante
CREATE OR ALTER VIEW Estudiante AS 
SELECT 
    u.Id AS Id,
    u.Nombre,
    u.Correo,
    u.Contrasena,
    u.Telefono,
    u.FechaRegistro,
    u.TipoUsuario,
    est.NivelAcademico,
    est.InstitucionProcedencia
FROM Usuario u
INNER JOIN tbl_Estudiante est ON u.Id = est.IdUsuario;
GO

CREATE OR ALTER VIEW Estudiantes AS SELECT * FROM Estudiante;
GO

-- Vistas Coordinador
CREATE OR ALTER VIEW CoordinadorBecas AS 
SELECT 
    u.Id AS Id,
    u.Nombre,
    u.Correo,
    u.Contrasena,
    u.Telefono,
    u.FechaRegistro,
    u.TipoUsuario,
    cb.Cargo
FROM Usuario u
INNER JOIN tbl_CoordinadorBecas cb ON u.Id = cb.IdUsuario;
GO

CREATE OR ALTER VIEW CoordinadoresBecas AS SELECT * FROM CoordinadorBecas;
GO

-- Vistas Evaluador
CREATE OR ALTER VIEW EvaluadorComite AS 
SELECT 
    u.Id AS Id,
    u.Nombre,
    u.Correo,
    u.Contrasena,
    u.Telefono,
    u.FechaRegistro,
    u.TipoUsuario,
    ec.Especialidad
FROM Usuario u
INNER JOIN tbl_EvaluadorComite ec ON u.Id = ec.IdUsuario;
GO

CREATE OR ALTER VIEW EvaluadorComites AS SELECT * FROM EvaluadorComite;
GO

CREATE OR ALTER VIEW EvaluadoresComite AS SELECT * FROM EvaluadorComite;
GO

-- ============================================================================
-- 4. CONVOCATORIA Y COMITÉ EVALUADOR
-- ============================================================================
CREATE TABLE Convocatoria (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Titulo          VARCHAR(200)  NOT NULL,
    Descripcion     VARCHAR(MAX)  NULL,
    Requisitos      VARCHAR(MAX)  NULL,
    FechaApertura   DATE          NOT NULL,
    FechaCierre     DATE          NOT NULL,
    Estado          VARCHAR(20)   NOT NULL DEFAULT 'BORRADOR'
        CHECK (Estado IN ('BORRADOR', 'ABIERTA', 'CERRADA')),
    IdCoordinador   INT           NOT NULL,
    CONSTRAINT FK_Convocatoria_Coordinador
        FOREIGN KEY (IdCoordinador) REFERENCES tbl_CoordinadorBecas(IdUsuario),
    CONSTRAINT CK_Convocatoria_Fechas
        CHECK (FechaCierre > FechaApertura)
);
GO

CREATE TABLE ComiteEvaluador (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    Nombre         VARCHAR(150)  NOT NULL,
    FechaCreacion  DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================================
-- 5. SOLICITUD
-- ============================================================================
CREATE TABLE Solicitud (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FechaCreacion   DATETIME      NOT NULL DEFAULT GETDATE(),
    Estado          VARCHAR(20)   NOT NULL DEFAULT 'EN_PROCESO'
        CHECK (Estado IN ('EN_PROCESO', 'EVALUADA', 'APROBADA', 'RECHAZADA', 'CANCELADA')),
    Observaciones   VARCHAR(MAX)  NULL,
    IdEstudiante    INT           NOT NULL,
    IdConvocatoria  INT           NOT NULL,
    IdComite        INT           NULL,
    CONSTRAINT FK_Solicitud_Estudiante
        FOREIGN KEY (IdEstudiante) REFERENCES tbl_Estudiante(IdUsuario),
    CONSTRAINT FK_Solicitud_Convocatoria
        FOREIGN KEY (IdConvocatoria) REFERENCES Convocatoria(Id),
    CONSTRAINT FK_Solicitud_Comite
        FOREIGN KEY (IdComite) REFERENCES ComiteEvaluador(Id),
    CONSTRAINT UQ_Solicitud_Estudiante_Convocatoria
        UNIQUE (IdEstudiante, IdConvocatoria)
);
GO

-- ============================================================================
-- 6. TABLAS RELACIONALES Y AUXILIARES
-- ============================================================================
CREATE TABLE ComiteMiembro (
    IdComite     INT NOT NULL,
    IdEvaluador  INT NOT NULL,
    CONSTRAINT PK_ComiteMiembro PRIMARY KEY (IdComite, IdEvaluador),
    CONSTRAINT FK_ComiteMiembro_Comite
        FOREIGN KEY (IdComite) REFERENCES ComiteEvaluador(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ComiteMiembro_Evaluador
        FOREIGN KEY (IdEvaluador) REFERENCES tbl_EvaluadorComite(IdUsuario) ON DELETE CASCADE
);
GO

CREATE TABLE Documento (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    NombreArchivo  VARCHAR(255)  NOT NULL,
    TipoArchivo    VARCHAR(10)   NOT NULL
        CHECK (TipoArchivo IN ('PDF', 'JPG', 'PNG')),
    UrlArchivo     VARCHAR(500)  NOT NULL,
    FechaCarga     DATETIME      NOT NULL DEFAULT GETDATE(),
    IdSolicitud    INT           NOT NULL,
    CONSTRAINT FK_Documento_Solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES Solicitud(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Evaluacion (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Puntaje          DECIMAL(5,2)  NOT NULL
        CHECK (Puntaje BETWEEN 0 AND 100),
    Observaciones    VARCHAR(MAX)  NULL,
    FechaEvaluacion  DATETIME      NOT NULL DEFAULT GETDATE(),
    IdSolicitud      INT           NOT NULL,
    IdEvaluador      INT           NOT NULL,
    CONSTRAINT FK_Evaluacion_Solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES Solicitud(Id),
    CONSTRAINT FK_Evaluacion_Evaluador
        FOREIGN KEY (IdEvaluador) REFERENCES tbl_EvaluadorComite(IdUsuario),
    CONSTRAINT UQ_Evaluacion_Solicitud_Evaluador
        UNIQUE (IdSolicitud, IdEvaluador)
);
GO

CREATE TABLE Reporte (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    TipoReporte       VARCHAR(50)   NOT NULL,
    FechaGeneracion   DATETIME      NOT NULL DEFAULT GETDATE(),
    Parametros        VARCHAR(MAX)  NULL,
    IdCoordinador     INT           NOT NULL,
    CONSTRAINT FK_Reporte_Coordinador
        FOREIGN KEY (IdCoordinador) REFERENCES tbl_CoordinadorBecas(IdUsuario)
);
GO

-- ============================================================================
-- 7. ÍNDICES DE RENDIMIENTO
-- ============================================================================
CREATE INDEX IX_Solicitud_Estado ON Solicitud(Estado);
CREATE INDEX IX_Solicitud_Convocatoria ON Solicitud(IdConvocatoria);
CREATE INDEX IX_Convocatoria_Estado ON Convocatoria(Estado);
GO

-- ============================================================================
-- 8. DATOS DE PRUEBA INICIALES
-- ============================================================================
INSERT INTO Usuario (Nombre, Correo, Contrasena, Telefono, TipoUsuario) VALUES
('Maria Lopez', 'maria.lopez@miumg.edu.gt', 'HASH_PENDIENTE_1', '55551111', 'ESTUDIANTE'),
('Ana Ramirez', 'ana.ramirez@mineduc.gob.gt', 'HASH_PENDIENTE_2', '55552222', 'COORDINADOR'),
('Carlos Perez', 'carlos.perez@mineduc.gob.gt', 'HASH_PENDIENTE_3', '55553333', 'EVALUADOR');
GO

INSERT INTO tbl_Estudiante (IdUsuario, NivelAcademico, InstitucionProcedencia)
VALUES (1, 'Universitario', 'Universidad Mariano Galvez');

INSERT INTO tbl_CoordinadorBecas (IdUsuario, Cargo)
VALUES (2, 'Coordinadora de Becas Ministerio de Educacion');

INSERT INTO tbl_EvaluadorComite (IdUsuario, Especialidad)
VALUES (3, 'Evaluacion Academica');
GO

INSERT INTO Convocatoria (Titulo, Descripcion, Requisitos, FechaApertura, FechaCierre, Estado, IdCoordinador)
VALUES ('Beca Universitaria 2026', 'Beca para estudiantes de escasos recursos',
        'Promedio minimo 80, carta de ingresos', '2026-09-01', '2026-09-30', 'ABIERTA', 2);
GO

INSERT INTO ComiteEvaluador (Nombre, FechaCreacion)
VALUES ('Comité Institucional de Becas 2026', GETDATE());
GO

/* ============================================================================
   FIN DEL SCRIPT CONSOLIDADO
   ============================================================================ */
