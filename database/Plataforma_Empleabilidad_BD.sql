IF DB_ID(N'Plataforma_Empleabilidad_BD') IS NULL
BEGIN
    CREATE DATABASE Plataforma_Empleabilidad_BD;
END;
GO

USE Plataforma_Empleabilidad_BD;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

/* =============================================================
   TABLA: Users (autenticacion y roles)
   ============================================================= */

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Users PRIMARY KEY
            DEFAULT NEWID(),

        Email NVARCHAR(254) NOT NULL
            CONSTRAINT UX_Users_Email UNIQUE,

        PasswordHash NVARCHAR(500) NULL,

        PasswordResetToken NVARCHAR(500) NULL,
        PasswordResetTokenExpiresAtUtc DATETIME2(0) NULL,

        Role NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Users_Role DEFAULT N'CANDIDATE'
            CONSTRAINT CK_Users_Role CHECK (Role IN (N'CANDIDATE', N'EMPLOYER', N'ADMINISTRATOR')),

        IsActive BIT NOT NULL
            CONSTRAINT DF_Users_IsActive DEFAULT 1,

        EmailConfirmed BIT NOT NULL
            CONSTRAINT DF_Users_EmailConfirmed DEFAULT 0,

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordHash NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordResetToken') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordResetToken NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenExpiresAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordResetTokenExpiresAtUtc DATETIME2(0) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_PasswordResetToken'
        AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_PasswordResetToken
        ON dbo.Users (PasswordResetToken)
        WHERE PasswordResetToken IS NOT NULL;
END;
GO

/* =============================================================
   TABLA: CandidateProfiles (perfil del candidato)
   ============================================================= */

IF OBJECT_ID(N'dbo.CandidateProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CandidateProfiles
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_CandidateProfiles PRIMARY KEY,

        UserId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_CandidateProfiles_Users
                FOREIGN KEY REFERENCES dbo.Users (Id)
            CONSTRAINT UQ_CandidateProfiles_UserId UNIQUE,

        FullName NVARCHAR(160) NOT NULL,

        DateOfBirth DATE NOT NULL,

        Age INT NULL
            CONSTRAINT CK_CandidateProfiles_Age
                CHECK (Age BETWEEN 18 AND 30),

        Province NVARCHAR(40) NOT NULL
            CONSTRAINT CK_CandidateProfiles_Province
                CHECK (Province IN
                (
                    N'San Jose',
                    N'Alajuela',
                    N'Cartago',
                    N'Heredia',
                    N'Guanacaste',
                    N'Puntarenas',
                    N'Limon'
                )),

        EducationLevel NVARCHAR(80) NOT NULL
            CONSTRAINT CK_CandidateProfiles_EducationLevel
                CHECK (EducationLevel IN
                (
                    N'Secundaria incompleta',
                    N'Secundaria completa',
                    N'Tecnico',
                    N'Universidad incompleta',
                    N'Universidad completa'
                )),

        IsVisibleToPartnerEmployers BIT NOT NULL
            CONSTRAINT DF_CandidateProfiles_IsVisibleToPartnerEmployers DEFAULT 1,

        PhotoUrl NVARCHAR(500) NULL,

        IsAvailableForContact BIT NOT NULL
            CONSTRAINT DF_CandidateProfiles_IsAvailableForContact DEFAULT 1,

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_CandidateProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF COL_LENGTH(N'dbo.CandidateProfiles', N'Age') IS NULL
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ADD Age INT NULL;
END;
GO

IF COL_LENGTH(N'dbo.CandidateProfiles', N'DateOfBirth') IS NULL
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ADD DateOfBirth DATE NULL;
END;
GO

UPDATE dbo.CandidateProfiles
SET DateOfBirth = DATEFROMPARTS(
        YEAR(SYSUTCDATETIME()) - COALESCE(NULLIF(Age, 0), 18),
        1,
        1)
WHERE DateOfBirth IS NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CandidateProfiles')
        AND name = N'DateOfBirth'
        AND is_nullable = 1
)
AND NOT EXISTS (
    SELECT 1
    FROM dbo.CandidateProfiles
    WHERE DateOfBirth IS NULL
)
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ALTER COLUMN DateOfBirth DATE NOT NULL;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CandidateProfiles')
        AND name = N'Age'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ALTER COLUMN Age INT NULL;
END;
GO

IF COL_LENGTH(N'dbo.CandidateProfiles', N'PhotoUrl') IS NULL
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ADD PhotoUrl NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH(N'dbo.CandidateProfiles', N'IsAvailableForContact') IS NULL
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ADD IsAvailableForContact BIT NOT NULL
            CONSTRAINT DF_CandidateProfiles_IsAvailableForContact DEFAULT 1
            WITH VALUES;
END;
GO

CREATE OR ALTER VIEW dbo.PartnerEmployerVisibleCandidateProfiles
AS
SELECT
    cp.Id,
    cp.FullName,
    cp.DateOfBirth,
    DATEDIFF(YEAR, cp.DateOfBirth, CAST(SYSUTCDATETIME() AS DATE))
        - CASE
            WHEN DATEADD(YEAR, DATEDIFF(YEAR, cp.DateOfBirth, CAST(SYSUTCDATETIME() AS DATE)), cp.DateOfBirth)
                > CAST(SYSUTCDATETIME() AS DATE)
            THEN 1
            ELSE 0
        END AS Age,
    cp.Province,
    cp.EducationLevel,
    cp.IsAvailableForContact,
    cp.PhotoUrl,
    u.Email,
    u.EmailConfirmed,
    cp.CreatedAtUtc
FROM dbo.CandidateProfiles cp
INNER JOIN dbo.Users u
    ON cp.UserId = u.Id
WHERE cp.IsVisibleToPartnerEmployers = 1;
GO

/* =============================================================
   TABLA: EmployerProfiles (perfil del empleador / PYME)
   ============================================================= */

IF OBJECT_ID(N'dbo.EmployerProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployerProfiles
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_EmployerProfiles PRIMARY KEY,

        UserId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_EmployerProfiles_Users
                FOREIGN KEY REFERENCES dbo.Users (Id)
            CONSTRAINT UQ_EmployerProfiles_UserId UNIQUE,

        CompanyName NVARCHAR(200) NOT NULL,

        LegalId NVARCHAR(20) NOT NULL
            CONSTRAINT UQ_EmployerProfiles_LegalId UNIQUE,

        Sector NVARCHAR(80) NOT NULL
            CONSTRAINT CK_EmployerProfiles_Sector
                CHECK (Sector IN
                (
                    N'Tecnología',
                    N'Manufactura',
                    N'Comercio',
                    N'Servicios',
                    N'Educación',
                    N'Salud',
                    N'Construcción',
                    N'Turismo',
                    N'Agroindustria',
                    N'Transporte y logística',
                    N'Finanzas y banca',
                    N'Medios y comunicación',
                    N'Otro'
                )),

        ContactName NVARCHAR(160) NOT NULL,

        ContactPhone NVARCHAR(30) NOT NULL,

        Location NVARCHAR(200) NOT NULL,

        Status NVARCHAR(30) NOT NULL
            CONSTRAINT DF_EmployerProfiles_Status DEFAULT N'PendingVerification'
            CONSTRAINT CK_EmployerProfiles_Status
                CHECK (Status IN (N'PendingVerification', N'Active', N'Rejected')),

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_EmployerProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

/* =============================================================
   TABLA: Vacantes (ofertas de empleo)
   ============================================================= */

IF OBJECT_ID(N'dbo.Vacantes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vacantes
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Vacantes PRIMARY KEY
            DEFAULT NEWID(),

        EmployerProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Vacantes_EmployerProfiles
                FOREIGN KEY REFERENCES dbo.EmployerProfiles (Id),

        JobTitle NVARCHAR(100) NOT NULL,
        Province NVARCHAR(50) NOT NULL,
        Sector NVARCHAR(50) NOT NULL,
        Modality NVARCHAR(30) NOT NULL,
        ExperienceLevel NVARCHAR(50) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Requirements NVARCHAR(MAX) NULL,
        SalaryRange NVARCHAR(100) NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_Vacantes_IsActive DEFAULT 1,

        PublishedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Vacantes_PublishedAt DEFAULT SYSUTCDATETIME(),

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_Vacantes_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF COL_LENGTH(N'dbo.Vacantes', N'Requirements') IS NULL
BEGIN
    ALTER TABLE dbo.Vacantes
        ADD Requirements NVARCHAR(MAX) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Vacantes', N'SalaryRange') IS NULL
BEGIN
    ALTER TABLE dbo.Vacantes
        ADD SalaryRange NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Vacantes_EmployerProfileId'
        AND object_id = OBJECT_ID(N'dbo.Vacantes')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vacantes_EmployerProfileId
        ON dbo.Vacantes (EmployerProfileId, PublishedAt DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Vacantes_IsActive_PublishedAt'
        AND object_id = OBJECT_ID(N'dbo.Vacantes')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vacantes_IsActive_PublishedAt
        ON dbo.Vacantes (IsActive, PublishedAt DESC);
END;
GO

/* =============================================================
   TABLA: Postulaciones (solicitudes de candidatos)
   ============================================================= */

IF OBJECT_ID(N'dbo.Postulaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Postulaciones
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Postulaciones PRIMARY KEY
            DEFAULT NEWID(),

        VacanteId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Postulaciones_Vacantes
                FOREIGN KEY REFERENCES dbo.Vacantes (Id),

        CandidateProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Postulaciones_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id),

        Status NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Postulaciones_Status DEFAULT N'Enviada',

        AppliedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Postulaciones_AppliedAt DEFAULT SYSUTCDATETIME(),

        UpdatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_Postulaciones_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UQ_Postulaciones_Vacante_Candidate UNIQUE (VacanteId, CandidateProfileId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Postulaciones_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.Postulaciones')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Postulaciones_CandidateProfileId
        ON dbo.Postulaciones (CandidateProfileId, AppliedAt DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Postulaciones_VacanteId'
        AND object_id = OBJECT_ID(N'dbo.Postulaciones')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Postulaciones_VacanteId
        ON dbo.Postulaciones (VacanteId, AppliedAt DESC);
END;
GO

/* =============================================================
   TABLAS: Perfil publico del candidato
   ============================================================= */

IF OBJECT_ID(N'dbo.ExperienciasLaborales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExperienciasLaborales
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_ExperienciasLaborales PRIMARY KEY
            DEFAULT NEWID(),

        CandidateProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_ExperienciasLaborales_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id)
                ON DELETE CASCADE,

        Empresa NVARCHAR(200) NOT NULL,
        Cargo NVARCHAR(200) NOT NULL,
        FechaInicio DATE NOT NULL,
        FechaFin DATE NULL,
        EsTrabajoActual BIT NOT NULL
            CONSTRAINT DF_ExperienciasLaborales_EsTrabajoActual DEFAULT 0,
        Descripcion NVARCHAR(1000) NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ExperienciasLaborales_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.ExperienciasLaborales')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExperienciasLaborales_CandidateProfileId
        ON dbo.ExperienciasLaborales (CandidateProfileId, FechaInicio DESC);
END;
GO

IF OBJECT_ID(N'dbo.Habilidades', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Habilidades
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Habilidades PRIMARY KEY
            DEFAULT NEWID(),

        CandidateProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Habilidades_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id)
                ON DELETE CASCADE,

        Nombre NVARCHAR(100) NOT NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Habilidades_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.Habilidades')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Habilidades_CandidateProfileId
        ON dbo.Habilidades (CandidateProfileId, Nombre);
END;
GO

IF OBJECT_ID(N'dbo.CursosCompletados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CursosCompletados
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_CursosCompletados PRIMARY KEY
            DEFAULT NEWID(),

        CandidateProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_CursosCompletados_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id)
                ON DELETE CASCADE,

        NombreCurso NVARCHAR(200) NOT NULL,
        Institucion NVARCHAR(200) NOT NULL,
        FechaCompletado DATE NOT NULL,
        EsDePlataforma BIT NOT NULL
            CONSTRAINT DF_CursosCompletados_EsDePlataforma DEFAULT 0
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_CursosCompletados_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.CursosCompletados')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_CursosCompletados_CandidateProfileId
        ON dbo.CursosCompletados (CandidateProfileId, FechaCompletado DESC);
END;
GO

/* =============================================================
   TABLAS: MicroCursos (catalogo validado por empleadores)
   ============================================================= */

IF OBJECT_ID(N'dbo.MicroCursos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MicroCursos
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_MicroCursos PRIMARY KEY
            DEFAULT NEWID(),

        Titulo NVARCHAR(160) NOT NULL,
        Descripcion NVARCHAR(1000) NOT NULL,
        Area NVARCHAR(80) NOT NULL,

        DuracionHoras INT NOT NULL
            CONSTRAINT CK_MicroCursos_DuracionHoras CHECK (DuracionHoras > 0),

        EntidadProveedora NVARCHAR(200) NOT NULL,

        TipoProveedor NVARCHAR(30) NOT NULL
            CONSTRAINT CK_MicroCursos_TipoProveedor
                CHECK (TipoProveedor IN (N'Nacional', N'Internacional')),

        OtorgaCertificacion BIT NOT NULL
            CONSTRAINT DF_MicroCursos_OtorgaCertificacion DEFAULT 0,

        IsActive BIT NOT NULL
            CONSTRAINT DF_MicroCursos_IsActive DEFAULT 1,

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_MicroCursos_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

-- Enlace a la pagina real del curso (proveedor externo), para que el
-- candidato pueda ir directamente a inscribirse/verlo desde el detalle.
IF COL_LENGTH(N'dbo.MicroCursos', N'EnlaceUrl') IS NULL
BEGIN
    ALTER TABLE dbo.MicroCursos
        ADD EnlaceUrl NVARCHAR(500) NULL;
END;
GO

IF OBJECT_ID(N'dbo.MicroCursoHabilidades', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MicroCursoHabilidades
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_MicroCursoHabilidades PRIMARY KEY
            DEFAULT NEWID(),

        MicroCursoId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MicroCursoHabilidades_MicroCursos
                FOREIGN KEY REFERENCES dbo.MicroCursos (Id)
                ON DELETE CASCADE,

        Nombre NVARCHAR(100) NOT NULL,

        CONSTRAINT UQ_MicroCursoHabilidades_Curso_Nombre
            UNIQUE (MicroCursoId, Nombre)
    );
END;
GO

IF OBJECT_ID(N'dbo.MicroCursoValidacionesEmpleador', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MicroCursoValidacionesEmpleador
    (
        MicroCursoId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MicroCursoValidaciones_MicroCursos
                FOREIGN KEY REFERENCES dbo.MicroCursos (Id)
                ON DELETE CASCADE,

        EmployerProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MicroCursoValidaciones_EmployerProfiles
                FOREIGN KEY REFERENCES dbo.EmployerProfiles (Id),

        ValidatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_MicroCursoValidaciones_ValidatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_MicroCursoValidacionesEmpleador
            PRIMARY KEY (MicroCursoId, EmployerProfileId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_MicroCursos_IsActive_Area'
        AND object_id = OBJECT_ID(N'dbo.MicroCursos')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MicroCursos_IsActive_Area
        ON dbo.MicroCursos (IsActive, Area);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_MicroCursoHabilidades_Nombre'
        AND object_id = OBJECT_ID(N'dbo.MicroCursoHabilidades')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MicroCursoHabilidades_Nombre
        ON dbo.MicroCursoHabilidades (Nombre);
END;
GO

/* =============================================================
   TABLA: Notificaciones (alertas para empleadores)
   ============================================================= */

IF OBJECT_ID(N'dbo.Notificaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificaciones
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Notificaciones PRIMARY KEY
            DEFAULT NEWID(),

        EmployerProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Notificaciones_EmployerProfiles
                FOREIGN KEY REFERENCES dbo.EmployerProfiles (Id),

        CandidateProfileId UNIQUEIDENTIFIER NULL
            CONSTRAINT FK_Notificaciones_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id),

        PostulacionId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Notificaciones_Postulaciones
                FOREIGN KEY REFERENCES dbo.Postulaciones (Id),

        VacanteId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Notificaciones_Vacantes
                FOREIGN KEY REFERENCES dbo.Vacantes (Id),

        Message NVARCHAR(300) NOT NULL,

        IsRead BIT NOT NULL
            CONSTRAINT DF_Notificaciones_IsRead DEFAULT 0,

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_Notificaciones_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF COL_LENGTH(N'dbo.Notificaciones', N'CandidateProfileId') IS NULL
BEGIN
    ALTER TABLE dbo.Notificaciones
        ADD CandidateProfileId UNIQUEIDENTIFIER NULL;
END;
GO

-- Las notificaciones dirigidas a un candidato (ej. entrevista solicitada,
-- postulacion declinada, vacante llenada) no tienen un empleador que las
-- "posea" desde la perspectiva del candidato, asi que EmployerProfileId
-- debe admitir NULL para esos casos.
IF EXISTS
(
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'Notificaciones'
        AND c.name = N'EmployerProfileId'
        AND c.is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.Notificaciones
        ALTER COLUMN EmployerProfileId UNIQUEIDENTIFIER NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Notificaciones_CandidateProfiles'
)
BEGIN
    ALTER TABLE dbo.Notificaciones
        ADD CONSTRAINT FK_Notificaciones_CandidateProfiles
            FOREIGN KEY (CandidateProfileId) REFERENCES dbo.CandidateProfiles (Id);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Notificaciones_EmployerProfileId'
        AND object_id = OBJECT_ID(N'dbo.Notificaciones')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notificaciones_EmployerProfileId
        ON dbo.Notificaciones (EmployerProfileId, IsRead, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Notificaciones_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.Notificaciones')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notificaciones_CandidateProfileId
        ON dbo.Notificaciones (CandidateProfileId, IsRead, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Notificaciones_VacanteId'
        AND object_id = OBJECT_ID(N'dbo.Notificaciones')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notificaciones_VacanteId
        ON dbo.Notificaciones (VacanteId, CreatedAtUtc DESC);
END;
GO

/* =============================================================
   PROCEDIMIENTOS ALMACENADOS
   ============================================================= */

CREATE OR ALTER PROCEDURE dbo.usp_User_FindByEmail
    @Email NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        Id, Email, PasswordHash, PasswordResetToken,
        PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
    FROM dbo.Users
    WHERE Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_FindById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        Id, Email, PasswordHash, PasswordResetToken,
        PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
    FROM dbo.Users
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_FindByPasswordResetToken
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        Id, Email, PasswordHash, PasswordResetToken,
        PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
    FROM dbo.Users
    WHERE PasswordResetToken = @Token;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id, Email, PasswordHash, PasswordResetToken,
        PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
    FROM dbo.Users
    ORDER BY CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Save
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(254),
    @PasswordHash NVARCHAR(500) = NULL,
    @Role NVARCHAR(20),
    @IsActive BIT,
    @EmailConfirmed BIT,
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users
        (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES
        (@Id, @Email, @PasswordHash, @Role, @IsActive, @EmailConfirmed, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdatePassword
    @Id UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdateRole
    @Id UNIQUEIDENTIFIER,
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET Role = @Role
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_SavePasswordResetToken
    @Id UNIQUEIDENTIFIER,
    @Token NVARCHAR(500),
    @ExpiresAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET PasswordResetToken = @Token,
        PasswordResetTokenExpiresAtUtc = @ExpiresAtUtc
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_ClearPasswordResetToken
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET PasswordResetToken = NULL,
        PasswordResetTokenExpiresAtUtc = NULL
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_SetActive
    @Id UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET IsActive = @IsActive
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_FindByEmail
    @Email NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        cp.Id,
        cp.UserId,
        cp.FullName,
        cp.DateOfBirth,
        cp.Province,
        cp.EducationLevel,
        cp.IsVisibleToPartnerEmployers,
        cp.IsAvailableForContact,
        cp.PhotoUrl,
        cp.CreatedAtUtc,
        u.Email,
        u.EmailConfirmed
    FROM dbo.CandidateProfiles cp
    INNER JOIN dbo.Users u ON cp.UserId = u.Id
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_FindByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        cp.Id,
        cp.UserId,
        cp.FullName,
        cp.DateOfBirth,
        cp.Province,
        cp.EducationLevel,
        cp.IsVisibleToPartnerEmployers,
        cp.IsAvailableForContact,
        cp.PhotoUrl,
        cp.CreatedAtUtc,
        u.Email,
        u.EmailConfirmed
    FROM dbo.CandidateProfiles cp
    INNER JOIN dbo.Users u ON cp.UserId = u.Id
    WHERE cp.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetVisibleToPartnerEmployers
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        FullName,
        DateOfBirth,
        Age,
        Province,
        EducationLevel,
        IsAvailableForContact,
        PhotoUrl,
        Email,
        EmailConfirmed,
        CreatedAtUtc
    FROM dbo.PartnerEmployerVisibleCandidateProfiles
    ORDER BY CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_Save
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(160),
    @DateOfBirth DATE,
    @Province NVARCHAR(40),
    @EducationLevel NVARCHAR(80),
    @IsVisibleToPartnerEmployers BIT,
    @IsAvailableForContact BIT,
    @PhotoUrl NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @Age INT =
        DATEDIFF(YEAR, @DateOfBirth, @Today)
        - CASE
            WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DateOfBirth, @Today), @DateOfBirth) > @Today THEN 1
            ELSE 0
          END;

    INSERT INTO dbo.CandidateProfiles
    (
        Id,
        UserId,
        FullName,
        DateOfBirth,
        Age,
        Province,
        EducationLevel,
        IsVisibleToPartnerEmployers,
        IsAvailableForContact,
        PhotoUrl,
        CreatedAtUtc
    )
    VALUES
    (
        @Id,
        @UserId,
        @FullName,
        @DateOfBirth,
        @Age,
        @Province,
        @EducationLevel,
        @IsVisibleToPartnerEmployers,
        @IsAvailableForContact,
        @PhotoUrl,
        @CreatedAtUtc
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_Update
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(160),
    @DateOfBirth DATE,
    @Province NVARCHAR(40),
    @EducationLevel NVARCHAR(80),
    @PhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @Age INT =
        DATEDIFF(YEAR, @DateOfBirth, @Today)
        - CASE
            WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DateOfBirth, @Today), @DateOfBirth) > @Today THEN 1
            ELSE 0
          END;

    UPDATE dbo.CandidateProfiles
    SET FullName = @FullName,
        DateOfBirth = @DateOfBirth,
        Age = @Age,
        Province = @Province,
        EducationLevel = @EducationLevel,
        PhotoUrl = @PhotoUrl
    WHERE Id = @Id
        AND UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_UpdateAvailability
    @Id UNIQUEIDENTIFIER,
    @IsAvailableForContact BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.CandidateProfiles
    SET IsAvailableForContact = @IsAvailableForContact
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_MarkEmailConfirmationSent
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE u
    SET u.EmailConfirmed = 1
    FROM dbo.Users u
    INNER JOIN dbo.CandidateProfiles cp ON u.Id = cp.UserId
    WHERE cp.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetExperiencias
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion
    FROM dbo.ExperienciasLaborales
    WHERE CandidateProfileId = @CandidateProfileId
    ORDER BY FechaInicio DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_SaveExperiencia
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER,
    @Empresa NVARCHAR(200),
    @Cargo NVARCHAR(200),
    @FechaInicio DATE,
    @FechaFin DATE = NULL,
    @EsTrabajoActual BIT,
    @Descripcion NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ExperienciasLaborales
        (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
    VALUES
        (@Id, @CandidateProfileId, @Empresa, @Cargo, @FechaInicio, @FechaFin, @EsTrabajoActual, @Descripcion);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_DeleteExperiencia
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.ExperienciasLaborales
    WHERE Id = @Id
        AND CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetHabilidades
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, CandidateProfileId, Nombre
    FROM dbo.Habilidades
    WHERE CandidateProfileId = @CandidateProfileId
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_SaveHabilidad
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
    VALUES (@Id, @CandidateProfileId, @Nombre);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_DeleteHabilidad
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Habilidades
    WHERE Id = @Id
        AND CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetCursos
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma
    FROM dbo.CursosCompletados
    WHERE CandidateProfileId = @CandidateProfileId
    ORDER BY FechaCompletado DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_SaveCurso
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER,
    @NombreCurso NVARCHAR(200),
    @Institucion NVARCHAR(200),
    @FechaCompletado DATE,
    @EsDePlataforma BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.CursosCompletados
        (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
    VALUES
        (@Id, @CandidateProfileId, @NombreCurso, @Institucion, @FechaCompletado, @EsDePlataforma);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_DeleteCurso
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.CursosCompletados
    WHERE Id = @Id
        AND CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_FindByEmail
    @Email NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
        ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
        u.Email, u.EmailConfirmed
    FROM dbo.EmployerProfiles ep
    INNER JOIN dbo.Users u ON ep.UserId = u.Id
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_FindByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
        ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
        u.Email, u.EmailConfirmed
    FROM dbo.EmployerProfiles ep
    INNER JOIN dbo.Users u ON ep.UserId = u.Id
    WHERE ep.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_FindById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
        ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
        u.Email, u.EmailConfirmed
    FROM dbo.EmployerProfiles ep
    INNER JOIN dbo.Users u ON ep.UserId = u.Id
    WHERE ep.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_Save
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @CompanyName NVARCHAR(200),
    @LegalId NVARCHAR(20),
    @Sector NVARCHAR(80),
    @ContactName NVARCHAR(160),
    @ContactPhone NVARCHAR(30),
    @Location NVARCHAR(200),
    @Status NVARCHAR(30),
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.EmployerProfiles
        (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
    VALUES
        (@Id, @UserId, @CompanyName, @LegalId, @Sector, @ContactName, @ContactPhone, @Location, @Status, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_UpdateStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.EmployerProfiles
    SET Status = @Status
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employer_MarkActivationEmailSent
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE u
    SET u.EmailConfirmed = 1
    FROM dbo.Users u
    INNER JOIN dbo.EmployerProfiles ep ON u.Id = ep.UserId
    WHERE ep.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_GetActive
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.Id,
        v.EmployerProfileId,
        v.JobTitle,
        v.Province,
        v.Sector,
        v.Modality,
        v.ExperienceLevel,
        v.Description,
        v.Requirements,
        v.SalaryRange,
        v.IsActive,
        v.PublishedAt,
        v.CreatedAtUtc,
        ep.CompanyName
    FROM dbo.Vacantes v
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    WHERE v.IsActive = 1
    ORDER BY v.PublishedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.Id,
        v.EmployerProfileId,
        v.JobTitle,
        v.Province,
        v.Sector,
        v.Modality,
        v.ExperienceLevel,
        v.Description,
        v.Requirements,
        v.SalaryRange,
        v.IsActive,
        v.PublishedAt,
        v.CreatedAtUtc,
        ep.CompanyName
    FROM dbo.Vacantes v
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    ORDER BY v.PublishedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_GetByEmployerProfileId
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.Id,
        v.EmployerProfileId,
        v.JobTitle,
        v.Province,
        v.Sector,
        v.Modality,
        v.ExperienceLevel,
        v.Description,
        v.Requirements,
        v.SalaryRange,
        v.IsActive,
        v.PublishedAt,
        v.CreatedAtUtc,
        ep.CompanyName
    FROM dbo.Vacantes v
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    WHERE v.EmployerProfileId = @EmployerProfileId
    ORDER BY v.PublishedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_FindById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        v.Id,
        v.EmployerProfileId,
        v.JobTitle,
        v.Province,
        v.Sector,
        v.Modality,
        v.ExperienceLevel,
        v.Description,
        v.Requirements,
        v.SalaryRange,
        v.IsActive,
        v.PublishedAt,
        v.CreatedAtUtc,
        ep.CompanyName
    FROM dbo.Vacantes v
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    WHERE v.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_Save
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER,
    @JobTitle NVARCHAR(100),
    @Province NVARCHAR(50),
    @Sector NVARCHAR(50),
    @Modality NVARCHAR(30),
    @ExperienceLevel NVARCHAR(50),
    @Description NVARCHAR(MAX) = NULL,
    @Requirements NVARCHAR(MAX) = NULL,
    @SalaryRange NVARCHAR(100) = NULL,
    @IsActive BIT,
    @PublishedAt DATETIME2(0),
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Vacantes
        (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel,
         Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
    VALUES
        (@Id, @EmployerProfileId, @JobTitle, @Province, @Sector, @Modality, @ExperienceLevel,
         @Description, @Requirements, @SalaryRange, @IsActive, @PublishedAt, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_UpdateStatus
    @Id UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vacantes
    SET IsActive = @IsActive
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vacante_UpdateEditableFields
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER,
    @Description NVARCHAR(MAX) = NULL,
    @Requirements NVARCHAR(MAX) = NULL,
    @SalaryRange NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vacantes
    SET Description = @Description,
        Requirements = @Requirements,
        SalaryRange = @SalaryRange
    WHERE Id = @Id
        AND EmployerProfileId = @EmployerProfileId
        AND IsActive = 1;

    SELECT CAST(@@ROWCOUNT AS INT) AS AffectedRows;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MicroCurso_GetValidated
    @Area NVARCHAR(80) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        mc.Id,
        mc.Titulo,
        mc.Descripcion,
        mc.Area,
        mc.DuracionHoras,
        mc.EntidadProveedora,
        mc.TipoProveedor,
        mc.OtorgaCertificacion,
        mc.EnlaceUrl,
        COUNT(DISTINCT ep.Id) AS CantidadValidaciones,
        mc.IsActive,
        mc.CreatedAtUtc
    FROM dbo.MicroCursos mc
    LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
        ON mc.Id = mcv.MicroCursoId
    LEFT JOIN dbo.EmployerProfiles ep
        ON mcv.EmployerProfileId = ep.Id
        AND ep.Status = N'Active'
    WHERE mc.IsActive = 1
        AND (@Area IS NULL OR mc.Area = @Area)
    GROUP BY
        mc.Id,
        mc.Titulo,
        mc.Descripcion,
        mc.Area,
        mc.DuracionHoras,
        mc.EntidadProveedora,
        mc.TipoProveedor,
        mc.OtorgaCertificacion,
        mc.EnlaceUrl,
        mc.IsActive,
        mc.CreatedAtUtc
    HAVING COUNT(DISTINCT ep.Id) >= 3
    ORDER BY mc.Area, mc.Titulo;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MicroCurso_FindValidatedById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        mc.Id,
        mc.Titulo,
        mc.Descripcion,
        mc.Area,
        mc.DuracionHoras,
        mc.EntidadProveedora,
        mc.TipoProveedor,
        mc.OtorgaCertificacion,
        mc.EnlaceUrl,
        COUNT(DISTINCT ep.Id) AS CantidadValidaciones,
        mc.IsActive,
        mc.CreatedAtUtc
    FROM dbo.MicroCursos mc
    LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
        ON mc.Id = mcv.MicroCursoId
    LEFT JOIN dbo.EmployerProfiles ep
        ON mcv.EmployerProfileId = ep.Id
        AND ep.Status = N'Active'
    WHERE mc.Id = @Id
        AND mc.IsActive = 1
    GROUP BY
        mc.Id,
        mc.Titulo,
        mc.Descripcion,
        mc.Area,
        mc.DuracionHoras,
        mc.EntidadProveedora,
        mc.TipoProveedor,
        mc.OtorgaCertificacion,
        mc.EnlaceUrl,
        mc.IsActive,
        mc.CreatedAtUtc
    HAVING COUNT(DISTINCT ep.Id) >= 3;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MicroCurso_GetHabilidades
    @MicroCursoId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Nombre
    FROM dbo.MicroCursoHabilidades
    WHERE MicroCursoId = @MicroCursoId
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_Save
    @Id UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER,
    @Status NVARCHAR(30),
    @AppliedAt DATETIME2(0),
    @UpdatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Postulaciones
        (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
    VALUES
        (@Id, @VacanteId, @CandidateProfileId, @Status, @AppliedAt, @UpdatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_ExistsByVacanteAndCandidate
    @VacanteId UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Postulaciones
    WHERE VacanteId = @VacanteId
        AND CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_GetByCandidateProfileId
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.VacanteId,
        p.CandidateProfileId,
        p.Status,
        p.AppliedAt,
        p.UpdatedAtUtc,
        v.JobTitle,
        v.Province,
        ep.CompanyName
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    WHERE p.CandidateProfileId = @CandidateProfileId
    ORDER BY p.AppliedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_GetByVacanteForEmployer
    @VacanteId UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.VacanteId,
        p.CandidateProfileId,
        p.Status,
        p.AppliedAt,
        p.UpdatedAtUtc,
        v.JobTitle,
        v.Province,
        ep.CompanyName,
        cp.FullName AS CandidateFullName,
        cp.Province AS CandidateProvince,
        cp.EducationLevel AS CandidateEducationLevel,
        cp.DateOfBirth AS CandidateDateOfBirth,
        u.Email AS CandidateEmail
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    INNER JOIN dbo.CandidateProfiles cp ON p.CandidateProfileId = cp.Id
    INNER JOIN dbo.Users u ON cp.UserId = u.Id
    WHERE p.VacanteId = @VacanteId
        AND v.EmployerProfileId = @EmployerProfileId
    ORDER BY p.AppliedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_GetByVacanteForClosure
    @VacanteId UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.VacanteId,
        p.CandidateProfileId,
        p.Status,
        p.AppliedAt,
        p.UpdatedAtUtc,
        v.JobTitle,
        v.Province,
        ep.CompanyName,
        cp.FullName AS CandidateFullName,
        cp.Province AS CandidateProvince,
        cp.EducationLevel AS CandidateEducationLevel,
        cp.DateOfBirth AS CandidateDateOfBirth,
        u.Email AS CandidateEmail
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    INNER JOIN dbo.CandidateProfiles cp ON p.CandidateProfileId = cp.Id
    INNER JOIN dbo.Users u ON cp.UserId = u.Id
    WHERE p.VacanteId = @VacanteId
        AND v.EmployerProfileId = @EmployerProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_FindByIdForEmployer
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        p.Id,
        p.VacanteId,
        p.CandidateProfileId,
        p.Status,
        p.AppliedAt,
        p.UpdatedAtUtc,
        v.JobTitle,
        v.Province,
        ep.CompanyName,
        cp.FullName AS CandidateFullName,
        cp.Province AS CandidateProvince,
        cp.EducationLevel AS CandidateEducationLevel,
        cp.DateOfBirth AS CandidateDateOfBirth,
        u.Email AS CandidateEmail
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    INNER JOIN dbo.CandidateProfiles cp ON p.CandidateProfileId = cp.Id
    INNER JOIN dbo.Users u ON cp.UserId = u.Id
    WHERE p.Id = @Id
        AND v.EmployerProfileId = @EmployerProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_UpdateStatusForEmployer
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER,
    @Status NVARCHAR(30),
    @UpdatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Status = @Status,
        p.UpdatedAtUtc = @UpdatedAtUtc
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    WHERE p.Id = @Id
        AND v.EmployerProfileId = @EmployerProfileId;

    SELECT CAST(@@ROWCOUNT AS INT) AS AffectedRows;
END;
GO

-- Solo se permite eliminar mientras la postulacion sigue en estado 'Enviada'
-- (antes de que el empleador la haya revisado/actuado). Evita que el
-- candidato borre el rastro de notificaciones de una postulacion que el
-- empleador ya esta gestionando activamente (entrevista, etc.).
CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_DeleteForCandidate
    @Id UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Notificaciones
    WHERE PostulacionId = @Id
        AND EXISTS
        (
            SELECT 1
            FROM dbo.Postulaciones p
            WHERE p.Id = @Id
                AND p.CandidateProfileId = @CandidateProfileId
                AND p.Status = N'Enviada'
        );

    DELETE FROM dbo.Postulaciones
    WHERE Id = @Id
        AND CandidateProfileId = @CandidateProfileId
        AND Status = N'Enviada';

    SELECT CAST(@@ROWCOUNT AS INT) AS AffectedRows;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_Save
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER = NULL,
    @PostulacionId UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER,
    @Message NVARCHAR(300),
    @IsRead BIT,
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Notificaciones
        (Id, EmployerProfileId, CandidateProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
    VALUES
        (@Id, @EmployerProfileId, @CandidateProfileId, @PostulacionId, @VacanteId, @Message, @IsRead, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_GetByEmployerProfileId
    @EmployerProfileId UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.Id,
        n.EmployerProfileId,
        n.CandidateProfileId,
        n.PostulacionId,
        n.VacanteId,
        v.JobTitle,
        n.Message,
        n.IsRead,
        n.CreatedAtUtc
    FROM dbo.Notificaciones n
    INNER JOIN dbo.Vacantes v ON n.VacanteId = v.Id
    WHERE n.EmployerProfileId = @EmployerProfileId
        AND (@VacanteId IS NULL OR n.VacanteId = @VacanteId)
    ORDER BY n.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_GetByCandidateProfileId
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.Id,
        n.EmployerProfileId,
        n.CandidateProfileId,
        n.PostulacionId,
        n.VacanteId,
        v.JobTitle,
        n.Message,
        n.IsRead,
        n.CreatedAtUtc
    FROM dbo.Notificaciones n
    INNER JOIN dbo.Vacantes v ON n.VacanteId = v.Id
    WHERE n.CandidateProfileId = @CandidateProfileId
    ORDER BY n.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_MarkAsRead
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notificaciones
    SET IsRead = 1
    WHERE Id = @Id
        AND EmployerProfileId = @EmployerProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_MarkEmployerVacanteAsRead
    @EmployerProfileId UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notificaciones
    SET IsRead = 1
    WHERE EmployerProfileId = @EmployerProfileId
        AND (@VacanteId IS NULL OR VacanteId = @VacanteId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_MarkCandidateAsRead
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notificaciones
    SET IsRead = 1
    WHERE CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_GetUnreadCount
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Notificaciones
    WHERE EmployerProfileId = @EmployerProfileId
        AND IsRead = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_GetCandidateUnreadCount
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Notificaciones
    WHERE CandidateProfileId = @CandidateProfileId
        AND IsRead = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_AdminReport_GetReportData
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalUsers,
        COALESCE(SUM(CASE WHEN Role = 'CANDIDATE' THEN 1 ELSE 0 END), 0) AS TotalCandidates,
        COALESCE(SUM(CASE WHEN Role = 'EMPLOYER' THEN 1 ELSE 0 END), 0) AS TotalEmployers,
        COALESCE(SUM(CASE WHEN Role = 'ADMINISTRATOR' THEN 1 ELSE 0 END), 0) AS TotalAdministrators
    FROM dbo.Users;

    SELECT
        COUNT(*) AS TotalVacantes,
        COALESCE(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END), 0) AS ActiveVacantes,
        COALESCE(SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END), 0) AS ClosedVacantes
    FROM dbo.Vacantes;

    SELECT
        COUNT(*) AS TotalPostulaciones,
        COUNT(DISTINCT CandidateProfileId) AS CandidatesWithPostulaciones,
        COUNT(DISTINCT VacanteId) AS VacantesWithPostulaciones
    FROM dbo.Postulaciones;

    SELECT Status, COUNT(*) AS Count
    FROM dbo.Postulaciones
    GROUP BY Status
    ORDER BY COUNT(*) DESC;

    SELECT Province, COUNT(*) AS Count
    FROM dbo.CandidateProfiles
    GROUP BY Province
    ORDER BY COUNT(*) DESC;

    SELECT Province, COUNT(*) AS Count
    FROM dbo.Vacantes
    GROUP BY Province
    ORDER BY COUNT(*) DESC;

    SELECT COUNT(*) AS TotalMicrocursos
    FROM
    (
        SELECT mc.Id
        FROM dbo.MicroCursos mc
        LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
            ON mc.Id = mcv.MicroCursoId
        LEFT JOIN dbo.EmployerProfiles ep
            ON mcv.EmployerProfileId = ep.Id
            AND ep.Status = N'Active'
        WHERE mc.IsActive = 1
        GROUP BY mc.Id
        HAVING COUNT(DISTINCT ep.Id) >= 3
    ) AS ValidatedMicroCursos;
END;
GO

/* =============================================================
   Datos iniciales
   ============================================================= */

DECLARE @AdminEmail NVARCHAR(254) = N'admin@gmail.com';
DECLARE @AdminPasswordHash NVARCHAR(500) = N'$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2';

-- Solo se crea si no existe. NUNCA se sobrescribe la contrasena/rol en corridas
-- subsecuentes: este script es idempotente y puede re-ejecutarse en produccion;
-- resetear credenciales aqui pisaria un cambio de contrasena legitimo del admin.
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO dbo.Users
        (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES
        (NEWID(), @AdminEmail, @AdminPasswordHash, N'ADMINISTRATOR', 1, 1, SYSUTCDATETIME());
END;
GO

/* =============================================================
   Datos iniciales: microcursos validados para HU8 / HU19
   ============================================================= */

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SeedPasswordHash NVARCHAR(500) = N'$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2';
    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    DECLARE @EmployerUserServicios UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
    DECLARE @EmployerUserComercio UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
    DECLARE @EmployerUserEducacion UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777771';

    DECLARE @EmployerServicios UNIQUEIDENTIFIER = '44444444-bbbb-4444-bbbb-444444444444';
    DECLARE @EmployerComercio UNIQUEIDENTIFIER = '55555555-bbbb-5555-bbbb-555555555555';
    DECLARE @EmployerEducacion UNIQUEIDENTIFIER = '77777777-bbbb-7777-bbbb-777777777771';

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.servicios@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserServicios, N'empleador.servicios@test.local', @SeedPasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -25, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.comercio@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserComercio, N'empleador.comercio@test.local', @SeedPasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -22, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.educacion@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserEducacion, N'empleador.educacion@test.local', @SeedPasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -10, @Now));

    SELECT @EmployerUserServicios = Id FROM dbo.Users WHERE Email = N'empleador.servicios@test.local';
    SELECT @EmployerUserComercio = Id FROM dbo.Users WHERE Email = N'empleador.comercio@test.local';
    SELECT @EmployerUserEducacion = Id FROM dbo.Users WHERE Email = N'empleador.educacion@test.local';

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerServicios, @EmployerUserServicios, N'Sinergia Servicios Digitales', N'3-101-000001', N'Servicios',
             N'Laura Rojas', N'8888-1001', N'San Jose', N'Active', DATEADD(DAY, -25, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerComercio, @EmployerUserComercio, N'Comercial La Sabana', N'3-101-000002', N'Comercio',
             N'Marco Solis', N'8888-1002', N'Alajuela', N'Active', DATEADD(DAY, -22, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserEducacion)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerEducacion, @EmployerUserEducacion, N'Academia Aliada CR', N'3-101-000004', N'Educación',
             N'Natalia Mena', N'8888-1004', N'San Jose', N'Active', DATEADD(DAY, -10, @Now));

    SELECT @EmployerServicios = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios;
    SELECT @EmployerComercio = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio;
    SELECT @EmployerEducacion = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserEducacion;

    DECLARE @React UNIQUEIDENTIFIER = '12121212-0001-0001-0001-121212121212';
    DECLARE @Excel UNIQUEIDENTIFIER = '12121212-0002-0002-0002-121212121212';
    DECLARE @ServicioCliente UNIQUEIDENTIFIER = '12121212-0003-0003-0003-121212121212';
    DECLARE @InglesEntrevistas UNIQUEIDENTIFIER = '12121212-0004-0004-0004-121212121212';
    DECLARE @Comunicacion UNIQUEIDENTIFIER = '12121212-0005-0005-0005-121212121212';

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @React)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, EnlaceUrl, IsActive, CreatedAtUtc)
        VALUES
            (@React, N'Introducción a React',
             N'Bases de componentes, estado, props y consumo simple de APIs para crear interfaces web.',
             N'Tecnología', 8, N'INA Virtual', N'Nacional', 1, N'https://react.dev/learn', 1, DATEADD(DAY, -8, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Excel)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, EnlaceUrl, IsActive, CreatedAtUtc)
        VALUES
            (@Excel, N'Excel para empleo',
             N'Funciones básicas, filtros, tablas y buenas prácticas para tareas administrativas iniciales.',
             N'Herramientas digitales', 6, N'INA', N'Nacional', 1,
             N'https://support.microsoft.com/en-us/office/excel-video-training-9bc05390-e94c-46af-a5b3-d7c22f6990bb',
             1, DATEADD(DAY, -7, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @ServicioCliente)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, EnlaceUrl, IsActive, CreatedAtUtc)
        VALUES
            (@ServicioCliente, N'Servicio al cliente digital',
             N'Técnicas de atención, seguimiento de casos y comunicación clara en canales digitales.',
             N'Habilidades blandas', 5, N'Fundación Aliada CR', N'Nacional', 1,
             N'https://www.coursera.org/learn/customer-service-fundamentals', 1, DATEADD(DAY, -6, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @InglesEntrevistas)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, EnlaceUrl, IsActive, CreatedAtUtc)
        VALUES
            (@InglesEntrevistas, N'Inglés para entrevistas laborales',
             N'Frases y respuestas frecuentes para entrevistas de primer empleo en inglés.',
             N'Idiomas', 10, N'Coursera', N'Internacional', 1,
             N'https://www.coursera.org/learn/careerdevelopment', 1, DATEADD(DAY, -5, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Comunicacion)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, EnlaceUrl, IsActive, CreatedAtUtc)
        VALUES
            (@Comunicacion, N'Comunicación efectiva en equipos',
             N'Prácticas para escuchar, sintetizar información y coordinar tareas en equipos de trabajo.',
             N'Habilidades blandas', 4, N'edX', N'Internacional', 1,
             N'https://www.edx.org/learn/communication-skills', 1, DATEADD(DAY, -4, @Now));

    -- Respaldo: si estos microcursos ya existian de una corrida anterior del
    -- seed (antes de que existiera EnlaceUrl), completa el enlace sin tocar
    -- nada mas.
    UPDATE dbo.MicroCursos SET EnlaceUrl = N'https://react.dev/learn'
        WHERE Id = @React AND EnlaceUrl IS NULL;
    UPDATE dbo.MicroCursos SET EnlaceUrl = N'https://support.microsoft.com/en-us/office/excel-video-training-9bc05390-e94c-46af-a5b3-d7c22f6990bb'
        WHERE Id = @Excel AND EnlaceUrl IS NULL;
    UPDATE dbo.MicroCursos SET EnlaceUrl = N'https://www.coursera.org/learn/customer-service-fundamentals'
        WHERE Id = @ServicioCliente AND EnlaceUrl IS NULL;
    UPDATE dbo.MicroCursos SET EnlaceUrl = N'https://www.coursera.org/learn/careerdevelopment'
        WHERE Id = @InglesEntrevistas AND EnlaceUrl IS NULL;
    UPDATE dbo.MicroCursos SET EnlaceUrl = N'https://www.edx.org/learn/communication-skills'
        WHERE Id = @Comunicacion AND EnlaceUrl IS NULL;

    INSERT INTO dbo.MicroCursoHabilidades (MicroCursoId, Nombre)
    SELECT Seed.MicroCursoId, Seed.Nombre
    FROM (VALUES
        (@React, N'React'),
        (@React, N'JavaScript'),
        (@React, N'CSS'),
        (@Excel, N'Excel básico'),
        (@Excel, N'Análisis de datos'),
        (@ServicioCliente, N'Servicio al cliente'),
        (@ServicioCliente, N'Comunicación'),
        (@InglesEntrevistas, N'Inglés'),
        (@InglesEntrevistas, N'Comunicación'),
        (@Comunicacion, N'Comunicación'),
        (@Comunicacion, N'Trabajo en equipo')
    ) AS Seed(MicroCursoId, Nombre)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.MicroCursoHabilidades Existing
        WHERE Existing.MicroCursoId = Seed.MicroCursoId
            AND Existing.Nombre = Seed.Nombre
    );

    INSERT INTO dbo.MicroCursoValidacionesEmpleador (MicroCursoId, EmployerProfileId, ValidatedAtUtc)
    SELECT Seed.MicroCursoId, Seed.EmployerProfileId, DATEADD(DAY, -2, @Now)
    FROM (VALUES
        (@React, @EmployerServicios), (@React, @EmployerComercio), (@React, @EmployerEducacion),
        (@Excel, @EmployerServicios), (@Excel, @EmployerComercio), (@Excel, @EmployerEducacion),
        (@ServicioCliente, @EmployerServicios), (@ServicioCliente, @EmployerComercio), (@ServicioCliente, @EmployerEducacion),
        (@InglesEntrevistas, @EmployerServicios), (@InglesEntrevistas, @EmployerComercio), (@InglesEntrevistas, @EmployerEducacion),
        (@Comunicacion, @EmployerServicios), (@Comunicacion, @EmployerComercio), (@Comunicacion, @EmployerEducacion)
    ) AS Seed(MicroCursoId, EmployerProfileId)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.MicroCursoValidacionesEmpleador Existing
        WHERE Existing.MicroCursoId = Seed.MicroCursoId
            AND Existing.EmployerProfileId = Seed.EmployerProfileId
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* =============================================================
   TABLA: HabilidadesBlandasSugeridas (catalogo de habilidades
   blandas sugeridas al candidato al agregar habilidades)
   ============================================================= */

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.HabilidadesBlandasSugeridas', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.HabilidadesBlandasSugeridas
        (
            Id UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_Id DEFAULT NEWID()
                CONSTRAINT PK_HabilidadesBlandasSugeridas PRIMARY KEY,

            Nombre NVARCHAR(100) NOT NULL,

            DisplayOrder INT NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_DisplayOrder DEFAULT 0,

            IsActive BIT NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_IsActive DEFAULT 1,

            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

            CONSTRAINT UQ_HabilidadesBlandasSugeridas_Nombre UNIQUE (Nombre)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_HabilidadesBlandasSugeridas_Active_Order'
            AND object_id = OBJECT_ID(N'dbo.HabilidadesBlandasSugeridas')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_HabilidadesBlandasSugeridas_Active_Order
            ON dbo.HabilidadesBlandasSugeridas (IsActive, DisplayOrder, Nombre);
    END;

    DECLARE @HabilidadSeed TABLE
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        DisplayOrder INT NOT NULL
    );

    INSERT INTO @HabilidadSeed (Id, Nombre, DisplayOrder)
    VALUES
        ('13131313-0001-0001-0001-131313131313', N'Comunicaci' + NCHAR(243) + N'n efectiva', 10),
        ('13131313-0002-0002-0002-131313131313', N'Trabajo en equipo', 20),
        ('13131313-0003-0003-0003-131313131313', N'Servicio al cliente', 30),
        ('13131313-0004-0004-0004-131313131313', N'Resoluci' + NCHAR(243) + N'n de problemas', 40),
        ('13131313-0005-0005-0005-131313131313', N'Adaptabilidad', 50),
        ('13131313-0006-0006-0006-131313131313', N'Responsabilidad', 60),
        ('13131313-0007-0007-0007-131313131313', N'Puntualidad', 70),
        ('13131313-0008-0008-0008-131313131313', N'Proactividad', 80),
        ('13131313-0009-0009-0009-131313131313', N'Organizaci' + NCHAR(243) + N'n', 90),
        ('13131313-0010-0010-0010-131313131313', N'Gesti' + NCHAR(243) + N'n del tiempo', 100),
        ('13131313-0011-0011-0011-131313131313', N'Atenci' + NCHAR(243) + N'n al detalle', 110),
        ('13131313-0012-0012-0012-131313131313', N'Empat' + NCHAR(237) + N'a', 120),
        ('13131313-0013-0013-0013-131313131313', N'Liderazgo', 130),
        ('13131313-0014-0014-0014-131313131313', N'Pensamiento cr' + NCHAR(237) + N'tico', 140),
        ('13131313-0015-0015-0015-131313131313', N'Aprendizaje r' + NCHAR(225) + N'pido', 150);

    UPDATE Existing
    SET Nombre = HabilidadSeed.Nombre,
        DisplayOrder = HabilidadSeed.DisplayOrder,
        IsActive = 1
    FROM dbo.HabilidadesBlandasSugeridas Existing
    INNER JOIN @HabilidadSeed HabilidadSeed
        ON Existing.Id = HabilidadSeed.Id;

    UPDATE Existing
    SET DisplayOrder = HabilidadSeed.DisplayOrder,
        IsActive = 1
    FROM dbo.HabilidadesBlandasSugeridas Existing
    INNER JOIN @HabilidadSeed HabilidadSeed
        ON Existing.Nombre = HabilidadSeed.Nombre;

    INSERT INTO dbo.HabilidadesBlandasSugeridas (Id, Nombre, DisplayOrder, IsActive)
    SELECT HabilidadSeed.Id, HabilidadSeed.Nombre, HabilidadSeed.DisplayOrder, 1
    FROM @HabilidadSeed HabilidadSeed
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.HabilidadesBlandasSugeridas Existing
        WHERE Existing.Id = HabilidadSeed.Id
    )
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.HabilidadesBlandasSugeridas Existing
            WHERE Existing.Nombre = HabilidadSeed.Nombre
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetHabilidadesBlandasSugeridas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Nombre
    FROM dbo.HabilidadesBlandasSugeridas
    WHERE IsActive = 1
    ORDER BY DisplayOrder, Nombre;
END;
GO

/* =============================================================
   TABLA: SugerenciasPostulacion (sugerencias de postulacion
   enviadas por empleadores a candidatos)
   ============================================================= */

IF OBJECT_ID(N'dbo.SugerenciasPostulacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SugerenciasPostulacion
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_SugerenciasPostulacion PRIMARY KEY
            DEFAULT NEWID(),

        VacanteId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_SugerenciasPostulacion_Vacantes
                FOREIGN KEY REFERENCES dbo.Vacantes (Id),

        CandidateProfileId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_SugerenciasPostulacion_CandidateProfiles
                FOREIGN KEY REFERENCES dbo.CandidateProfiles (Id),

        Message NVARCHAR(500) NULL,

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_SugerenciasPostulacion_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UQ_SugerenciasPostulacion_Vacante_Candidate UNIQUE (VacanteId, CandidateProfileId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_SugerenciasPostulacion_CandidateProfileId'
        AND object_id = OBJECT_ID(N'dbo.SugerenciasPostulacion')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SugerenciasPostulacion_CandidateProfileId
        ON dbo.SugerenciasPostulacion (CandidateProfileId, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_SugerenciasPostulacion_VacanteId'
        AND object_id = OBJECT_ID(N'dbo.SugerenciasPostulacion')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SugerenciasPostulacion_VacanteId
        ON dbo.SugerenciasPostulacion (VacanteId, CreatedAtUtc DESC);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SugerenciaPostulacion_Save
    @Id UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER,
    @Message NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SugerenciasPostulacion
        (Id, VacanteId, CandidateProfileId, Message, CreatedAtUtc)
    VALUES
        (@Id, @VacanteId, @CandidateProfileId, @Message, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SugerenciaPostulacion_ExistsByVacanteAndCandidate
    @VacanteId UNIQUEIDENTIFIER,
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.SugerenciasPostulacion
    WHERE VacanteId = @VacanteId
        AND CandidateProfileId = @CandidateProfileId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SugerenciaPostulacion_GetByCandidateProfileId
    @CandidateProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sp.Id,
        sp.VacanteId,
        sp.CandidateProfileId,
        sp.Message,
        sp.CreatedAtUtc,
        v.JobTitle,
        v.Province,
        v.IsActive AS VacanteIsActive,
        ep.CompanyName,
        ep.ContactName AS EmployerContactName,
        CAST(CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS AlreadyApplied
    FROM dbo.SugerenciasPostulacion sp
    INNER JOIN dbo.Vacantes v ON sp.VacanteId = v.Id
    INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
    LEFT JOIN dbo.Postulaciones p
        ON p.VacanteId = sp.VacanteId AND p.CandidateProfileId = sp.CandidateProfileId
    WHERE sp.CandidateProfileId = @CandidateProfileId
    ORDER BY sp.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_FindVisibleById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1) *
    FROM dbo.PartnerEmployerVisibleCandidateProfiles
    WHERE Id = @Id;
END;
GO

-- usp_Candidate_SearchForEmployer: definida directamente en su forma final
-- (incluye @EmployerProfileId y HasAppliedToYourVacantes desde el inicio;
-- las versiones intermedias sin estos campos, usadas durante el desarrollo
-- de esta feature, quedaron superadas y no aportan nada al unificar).
CREATE OR ALTER PROCEDURE dbo.usp_Candidate_SearchForEmployer
    @EmployerProfileId UNIQUEIDENTIFIER,
    @SkillKeyword NVARCHAR(100) = NULL,
    @Province NVARCHAR(40) = NULL,
    @EducationLevel NVARCHAR(80) = NULL,
    @MinExperienceYears DECIMAL(5,1) = NULL,
    @IsAvailableForContact BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ExperienceYears AS
    (
        SELECT
            el.CandidateProfileId,
            SUM(DATEDIFF(MONTH, el.FechaInicio, ISNULL(el.FechaFin, CAST(SYSUTCDATETIME() AS DATE)))) / 12.0 AS TotalYears
        FROM dbo.ExperienciasLaborales el
        GROUP BY el.CandidateProfileId
    )
    SELECT DISTINCT
        v.Id, v.FullName, v.DateOfBirth, v.Age, v.Province, v.EducationLevel,
        v.IsAvailableForContact, v.PhotoUrl, v.Email, v.EmailConfirmed, v.CreatedAtUtc,
        CAST(ISNULL(ey.TotalYears, 0) AS DECIMAL(5,1)) AS ExperienceYears,
        CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.Postulaciones p
            INNER JOIN dbo.Vacantes pv ON p.VacanteId = pv.Id
            WHERE p.CandidateProfileId = v.Id
                AND pv.EmployerProfileId = @EmployerProfileId
        ) THEN 1 ELSE 0 END AS BIT) AS HasAppliedToYourVacantes
    FROM dbo.PartnerEmployerVisibleCandidateProfiles v
    LEFT JOIN ExperienceYears ey ON ey.CandidateProfileId = v.Id
    LEFT JOIN dbo.Habilidades h
        ON h.CandidateProfileId = v.Id
        AND (@SkillKeyword IS NULL OR h.Nombre LIKE '%' + @SkillKeyword + '%')
    WHERE (@Province IS NULL OR v.Province = @Province)
        AND (@EducationLevel IS NULL OR v.EducationLevel = @EducationLevel)
        AND (@IsAvailableForContact IS NULL OR v.IsAvailableForContact = @IsAvailableForContact)
        AND (@SkillKeyword IS NULL OR h.Id IS NOT NULL)
        AND (@MinExperienceYears IS NULL OR ISNULL(ey.TotalYears, 0) >= @MinExperienceYears)
    ORDER BY v.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Postulacion_GetAppliedVacanteIdsForEmployer
    @CandidateProfileId UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.VacanteId
    FROM dbo.Postulaciones p
    INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
    WHERE p.CandidateProfileId = @CandidateProfileId
        AND v.EmployerProfileId = @EmployerProfileId;
END;
GO

/* =============================================================
   Datos de prueba: login de demostracion (admin, empleador, candidato)
   con una vacante activa, una inactiva y una postulacion de ejemplo.
   Contrasena de prueba para estas 3 cuentas: 123456
   ============================================================= */

SET NOCOUNT ON;
GO

DECLARE @DemoPasswordHash NVARCHAR(500) = N'$2a$11$HK.Fr.TCSkHMqrTbTi3TJ.pnqMc76AJgWed3mUWjYBmZNasY.g3wW';

DECLARE @DemoAdminEmail NVARCHAR(254) = N'admin@gmail.com';
DECLARE @DemoEmployerEmail NVARCHAR(254) = N'empleador.demo@sinergia.cr';
DECLARE @DemoCandidateEmail NVARCHAR(254) = N'candidato.demo@sinergia.cr';

DECLARE @DemoAdminUserId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @DemoEmployerUserId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @DemoCandidateUserId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

DECLARE @DemoCandidateProfileId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @DemoEmployerProfileId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

DECLARE @DemoVacanteActivaId UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';
DECLARE @DemoVacanteInactivaId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';
DECLARE @DemoPostulacionId UNIQUEIDENTIFIER = '88888888-8888-8888-8888-888888888888';

DECLARE @ResolvedDemoAdminUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedDemoEmployerUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedDemoCandidateUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedDemoEmployerProfileId UNIQUEIDENTIFIER;
DECLARE @ResolvedDemoCandidateProfileId UNIQUEIDENTIFIER;

-- Solo se crean si no existen: no se resetea contrasena/rol en corridas
-- subsecuentes para no pisar cambios ya hechos sobre cuentas existentes.
SELECT @ResolvedDemoAdminUserId = Id FROM dbo.Users WHERE Email = @DemoAdminEmail;
IF @ResolvedDemoAdminUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@DemoAdminUserId, @DemoAdminEmail, @DemoPasswordHash, N'ADMINISTRATOR', 1, 1, SYSUTCDATETIME());
    SET @ResolvedDemoAdminUserId = @DemoAdminUserId;
END

SELECT @ResolvedDemoEmployerUserId = Id FROM dbo.Users WHERE Email = @DemoEmployerEmail;
IF @ResolvedDemoEmployerUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@DemoEmployerUserId, @DemoEmployerEmail, @DemoPasswordHash, N'EMPLOYER', 1, 1, SYSUTCDATETIME());
    SET @ResolvedDemoEmployerUserId = @DemoEmployerUserId;
END

SELECT @ResolvedDemoCandidateUserId = Id FROM dbo.Users WHERE Email = @DemoCandidateEmail;
IF @ResolvedDemoCandidateUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@DemoCandidateUserId, @DemoCandidateEmail, @DemoPasswordHash, N'CANDIDATE', 1, 1, SYSUTCDATETIME());
    SET @ResolvedDemoCandidateUserId = @DemoCandidateUserId;
END

IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @ResolvedDemoEmployerUserId)
BEGIN
    INSERT INTO dbo.EmployerProfiles
        (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
    VALUES
        (
            @DemoEmployerProfileId,
            @ResolvedDemoEmployerUserId,
            N'Comercial Demo Costa Rica S.A.',
            N'3-101-999999',
            N'Servicios',
            N'Ana Solano',
            N'+506 8888-1111',
            N'San Jose, Costa Rica',
            N'Active',
            SYSUTCDATETIME()
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @ResolvedDemoCandidateUserId)
BEGIN
    INSERT INTO dbo.CandidateProfiles
        (Id, UserId, FullName, DateOfBirth, Province, EducationLevel, IsVisibleToPartnerEmployers, CreatedAtUtc)
    VALUES
        (
            @DemoCandidateProfileId,
            @ResolvedDemoCandidateUserId,
            N'Luis Enrique Mora',
            '2004-04-18',
            N'San Jose',
            N'Secundaria completa',
            1,
            SYSUTCDATETIME()
        );
END

SELECT @ResolvedDemoEmployerProfileId = Id
FROM dbo.EmployerProfiles
WHERE UserId = @ResolvedDemoEmployerUserId;

SELECT @ResolvedDemoCandidateProfileId = Id
FROM dbo.CandidateProfiles
WHERE UserId = @ResolvedDemoCandidateUserId;

IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @DemoVacanteActivaId)
BEGIN
    INSERT INTO dbo.Vacantes
        (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, IsActive, PublishedAt, CreatedAtUtc)
    VALUES
        (
            @DemoVacanteActivaId,
            @ResolvedDemoEmployerProfileId,
            N'Asistente de servicio al cliente',
            N'San Jose',
            N'Servicios',
            N'Presencial',
            N'Sin experiencia',
            N'Vacante de prueba para validar postulación y desactivación sin eliminar el registro.',
            1,
            DATEADD(DAY, -1, SYSUTCDATETIME()),
            DATEADD(DAY, -1, SYSUTCDATETIME())
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @DemoVacanteInactivaId)
BEGIN
    INSERT INTO dbo.Vacantes
        (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, IsActive, PublishedAt, CreatedAtUtc)
    VALUES
        (
            @DemoVacanteInactivaId,
            @ResolvedDemoEmployerProfileId,
            N'Auxiliar administrativo',
            N'Alajuela',
            N'Servicios',
            N'Híbrido',
            N'1-3 años',
            N'Esta vacante inicia desactivada para probar el cambio de estado en el panel.',
            0,
            DATEADD(DAY, -2, SYSUTCDATETIME()),
            DATEADD(DAY, -2, SYSUTCDATETIME())
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE Id = @DemoPostulacionId)
BEGIN
    INSERT INTO dbo.Postulaciones
        (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
    VALUES
        (
            @DemoPostulacionId,
            @DemoVacanteActivaId,
            @ResolvedDemoCandidateProfileId,
            N'Enviada',
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
END
GO

/* =============================================================
   Datos de prueba extendidos: 3 candidatos, 3 empleadores (uno
   pendiente de verificacion), 5 vacantes, postulaciones en
   distintos estados, notificaciones, experiencia/habilidades/cursos.
   Contrasena de prueba para estas cuentas: 123456
   ============================================================= */

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PruebaPasswordHash NVARCHAR(500) = N'$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2'; -- 123456
    DECLARE @PruebaNow DATETIME2(0) = SYSUTCDATETIME();

    DECLARE @PruebaAdminUserId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

    DECLARE @CandidateUserAna UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @CandidateUserJose UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
    DECLARE @CandidateUserValeria UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

    DECLARE @CandidateAna UNIQUEIDENTIFIER = '11111111-aaaa-1111-aaaa-111111111111';
    DECLARE @CandidateJose UNIQUEIDENTIFIER = '22222222-aaaa-2222-aaaa-222222222222';
    DECLARE @CandidateValeria UNIQUEIDENTIFIER = '33333333-aaaa-3333-aaaa-333333333333';

    DECLARE @EmployerUserServicios UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
    DECLARE @EmployerUserComercio UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
    DECLARE @EmployerUserPendiente UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';

    DECLARE @EmployerServicios UNIQUEIDENTIFIER = '44444444-bbbb-4444-bbbb-444444444444';
    DECLARE @EmployerComercio UNIQUEIDENTIFIER = '55555555-bbbb-5555-bbbb-555555555555';
    DECLARE @EmployerPendiente UNIQUEIDENTIFIER = '66666666-bbbb-6666-bbbb-666666666666';

    DECLARE @VacanteSoporte UNIQUEIDENTIFIER = '77777777-0001-0001-0001-777777777777';
    DECLARE @VacanteFrontend UNIQUEIDENTIFIER = '77777777-0002-0002-0002-777777777777';
    DECLARE @VacanteVentas UNIQUEIDENTIFIER = '88888888-0001-0001-0001-888888888888';
    DECLARE @VacanteBodega UNIQUEIDENTIFIER = '88888888-0002-0002-0002-888888888888';
    DECLARE @VacanteCerrada UNIQUEIDENTIFIER = '99999999-0001-0001-0001-999999999999';

    DECLARE @PostAnaSoporte UNIQUEIDENTIFIER = 'aaaaaaaa-0001-0001-0001-000000000001';
    DECLARE @PostAnaFrontend UNIQUEIDENTIFIER = 'aaaaaaaa-0002-0002-0002-000000000002';
    DECLARE @PostJoseSoporte UNIQUEIDENTIFIER = 'bbbbbbbb-0001-0001-0001-000000000001';
    DECLARE @PostJoseVentas UNIQUEIDENTIFIER = 'bbbbbbbb-0002-0002-0002-000000000002';

    -- Usuarios base para login. Solo se crean si no existen: no se resetea su
    -- contrasena/rol en corridas subsecuentes para no pisar cambios ya hechos
    -- sobre datos de prueba existentes (ej. contrasena cambiada manualmente).
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@gmail.com')
    BEGIN
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@PruebaAdminUserId, N'admin@gmail.com', @PruebaPasswordHash, N'ADMINISTRATOR', 1, 1, DATEADD(DAY, -30, @PruebaNow));
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'ana.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserAna, N'ana.candidato@test.local', @PruebaPasswordHash, N'CANDIDATE', 1, 1, DATEADD(DAY, -20, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'jose.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserJose, N'jose.candidato@test.local', @PruebaPasswordHash, N'CANDIDATE', 1, 1, DATEADD(DAY, -18, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'valeria.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserValeria, N'valeria.candidato@test.local', @PruebaPasswordHash, N'CANDIDATE', 1, 0, DATEADD(DAY, -12, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.servicios@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserServicios, N'empleador.servicios@test.local', @PruebaPasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -25, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.comercio@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserComercio, N'empleador.comercio@test.local', @PruebaPasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -22, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.pendiente@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserPendiente, N'empleador.pendiente@test.local', @PruebaPasswordHash, N'EMPLOYER', 1, 0, DATEADD(DAY, -5, @PruebaNow));

    SELECT @CandidateUserAna = Id FROM dbo.Users WHERE Email = N'ana.candidato@test.local';
    SELECT @CandidateUserJose = Id FROM dbo.Users WHERE Email = N'jose.candidato@test.local';
    SELECT @CandidateUserValeria = Id FROM dbo.Users WHERE Email = N'valeria.candidato@test.local';
    SELECT @EmployerUserServicios = Id FROM dbo.Users WHERE Email = N'empleador.servicios@test.local';
    SELECT @EmployerUserComercio = Id FROM dbo.Users WHERE Email = N'empleador.comercio@test.local';
    SELECT @EmployerUserPendiente = Id FROM dbo.Users WHERE Email = N'empleador.pendiente@test.local';

    -- Perfiles de candidatos visibles para empleadores.
    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserAna)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateAna, @CandidateUserAna, N'Ana Morales', '2004-05-15', 22, N'San Jose', N'Universidad incompleta', 1, 1, NULL, DATEADD(DAY, -20, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserJose)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateJose, @CandidateUserJose, N'Jose Vargas', '2001-11-20', 24, N'Cartago', N'Tecnico', 1, 1, NULL, DATEADD(DAY, -18, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserValeria)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateValeria, @CandidateUserValeria, N'Valeria Jimenez', '1998-08-12', 27, N'Alajuela', N'Universidad completa', 1, 0, NULL, DATEADD(DAY, -12, @PruebaNow));

    SELECT @CandidateAna = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserAna;
    SELECT @CandidateJose = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserJose;
    SELECT @CandidateValeria = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserValeria;

    -- Perfiles de empleadores.
    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerServicios, @EmployerUserServicios, N'Sinergia Servicios Digitales', N'3-101-000001', N'Servicios', N'Laura Rojas', N'8888-1001', N'San Jose', N'Active', DATEADD(DAY, -25, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerComercio, @EmployerUserComercio, N'Comercial La Sabana', N'3-101-000002', N'Comercio', N'Marco Solis', N'8888-1002', N'Alajuela', N'Active', DATEADD(DAY, -22, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserPendiente)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerPendiente, @EmployerUserPendiente, N'Proyecto Pyme Pendiente', N'3-101-000003', N'Otro', N'Paula Castro', N'8888-1003', N'Heredia', N'PendingVerification', DATEADD(DAY, -5, @PruebaNow));

    SELECT @EmployerServicios = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios;
    SELECT @EmployerComercio = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio;
    SELECT @EmployerPendiente = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserPendiente;

    -- Vacantes para listar, postular y ver metricas.
    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteSoporte)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteSoporte, @EmployerServicios, N'Soporte tecnico junior', N'San Jose', N'Servicios', N'Presencial', N'Sin experiencia',
             N'Atencion de tickets, soporte basico a usuarios y seguimiento de casos.',
             N'Conocimiento basico de computadoras, comunicacion clara y disponibilidad para horario diurno.',
             N'CRC 380000 - 450000', 1, DATEADD(DAY, -3, @PruebaNow), DATEADD(DAY, -3, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteFrontend)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteFrontend, @EmployerServicios, N'Desarrollador frontend junior', N'Heredia', N'Servicios', N'Remoto', N'1-3 años',
             N'Construccion de interfaces React y consumo de APIs internas.',
             N'React, JavaScript, CSS y manejo basico de Git.',
             N'CRC 550000 - 750000', 1, DATEADD(DAY, -2, @PruebaNow), DATEADD(DAY, -2, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteVentas)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteVentas, @EmployerComercio, N'Asistente de ventas', N'Alajuela', N'Comercio', N'Presencial', N'Menos de 1 año',
             N'Apoyo en piso de ventas, caja y servicio al cliente.',
             N'Excelente trato al cliente, orden y disponibilidad fines de semana.',
             N'CRC 360000 - 420000', 1, DATEADD(DAY, -1, @PruebaNow), DATEADD(DAY, -1, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteBodega)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteBodega, @EmployerComercio, N'Auxiliar de bodega', N'Cartago', N'Comercio', N'Presencial', N'Sin experiencia',
             N'Recepcion, acomodo y control de inventario.',
             N'Orden, puntualidad y capacidad para trabajo fisico moderado.',
             N'CRC 350000 - 410000', 1, DATEADD(HOUR, -8, @PruebaNow), DATEADD(HOUR, -8, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteCerrada)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteCerrada, @EmployerServicios, N'Asistente administrativo cerrado', N'San Jose', N'Servicios', N'Presencial', N'1-3 años',
             N'Vacante cerrada para probar reportes y filtros.',
             N'Manejo de documentos y servicio al cliente.',
             N'CRC 400000 - 500000', 0, DATEADD(DAY, -15, @PruebaNow), DATEADD(DAY, -15, @PruebaNow));

    -- Postulaciones en distintos estados.
    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteSoporte AND CandidateProfileId = @CandidateAna)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostAnaSoporte, @VacanteSoporte, @CandidateAna, N'Enviada', DATEADD(DAY, -1, @PruebaNow), DATEADD(DAY, -1, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteFrontend AND CandidateProfileId = @CandidateAna)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostAnaFrontend, @VacanteFrontend, @CandidateAna, N'En revisión', DATEADD(HOUR, -18, @PruebaNow), DATEADD(HOUR, -12, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteSoporte AND CandidateProfileId = @CandidateJose)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostJoseSoporte, @VacanteSoporte, @CandidateJose, N'Vista', DATEADD(DAY, -2, @PruebaNow), DATEADD(DAY, -1, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteVentas AND CandidateProfileId = @CandidateJose)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostJoseVentas, @VacanteVentas, @CandidateJose, N'Entrevista solicitada', DATEADD(HOUR, -10, @PruebaNow), DATEADD(HOUR, -2, @PruebaNow));

    -- Notificaciones asociadas a postulaciones.
    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostAnaSoporte)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostAnaSoporte, @VacanteSoporte, N'Ana Morales se ha postulado a la vacante Soporte tecnico junior', 0, DATEADD(DAY, -1, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostAnaFrontend)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostAnaFrontend, @VacanteFrontend, N'Ana Morales se ha postulado a la vacante Desarrollador frontend junior', 1, DATEADD(HOUR, -18, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostJoseSoporte)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostJoseSoporte, @VacanteSoporte, N'Jose Vargas se ha postulado a la vacante Soporte tecnico junior', 0, DATEADD(DAY, -2, @PruebaNow));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostJoseVentas)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerComercio, @PostJoseVentas, @VacanteVentas, N'Jose Vargas se ha postulado a la vacante Asistente de ventas', 0, DATEADD(HOUR, -10, @PruebaNow));

    -- Perfil completo de candidatos.
    IF NOT EXISTS (SELECT 1 FROM dbo.ExperienciasLaborales WHERE Id = '11111111-eeee-1111-eeee-111111111111')
        INSERT INTO dbo.ExperienciasLaborales
            (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
        VALUES
            ('11111111-eeee-1111-eeee-111111111111', @CandidateAna, N'Call Center Central', N'Agente de soporte', '2024-02-01', '2025-01-31', 0, N'Atencion telefonica y registro de casos.');

    IF NOT EXISTS (SELECT 1 FROM dbo.ExperienciasLaborales WHERE Id = '22222222-eeee-2222-eeee-222222222222')
        INSERT INTO dbo.ExperienciasLaborales
            (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
        VALUES
            ('22222222-eeee-2222-eeee-222222222222', @CandidateJose, N'Taller Cartago Norte', N'Asistente tecnico', '2023-06-01', NULL, 1, N'Apoyo en inventario, mantenimiento basico y atencion al cliente.');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '11111111-f111-1111-f111-111111111111')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('11111111-f111-1111-f111-111111111111', @CandidateAna, N'React');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '11111111-f222-1111-f222-111111111111')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('11111111-f222-1111-f222-111111111111', @CandidateAna, N'Servicio al cliente');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '22222222-f111-2222-f111-222222222222')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('22222222-f111-2222-f111-222222222222', @CandidateJose, N'Excel basico');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '33333333-f111-3333-f111-333333333333')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('33333333-f111-3333-f111-333333333333', @CandidateValeria, N'Comunicacion');

    IF NOT EXISTS (SELECT 1 FROM dbo.CursosCompletados WHERE Id = '11111111-c111-1111-c111-111111111111')
        INSERT INTO dbo.CursosCompletados
            (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
        VALUES
            ('11111111-c111-1111-c111-111111111111', @CandidateAna, N'Introduccion a React', N'Sinergia', '2025-11-15', 1);

    IF NOT EXISTS (SELECT 1 FROM dbo.CursosCompletados WHERE Id = '22222222-c111-2222-c111-222222222222')
        INSERT INTO dbo.CursosCompletados
            (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
        VALUES
            ('22222222-c111-2222-c111-222222222222', @CandidateJose, N'Excel para empleo', N'INA', '2025-09-10', 0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
