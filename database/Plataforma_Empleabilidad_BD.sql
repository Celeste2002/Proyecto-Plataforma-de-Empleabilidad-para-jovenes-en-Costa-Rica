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

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_Save
    @Id UNIQUEIDENTIFIER,
    @EmployerProfileId UNIQUEIDENTIFIER,
    @PostulacionId UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER,
    @Message NVARCHAR(300),
    @IsRead BIT,
    @CreatedAtUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Notificaciones
        (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
    VALUES
        (@Id, @EmployerProfileId, @PostulacionId, @VacanteId, @Message, @IsRead, @CreatedAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notificacion_GetByEmployerProfileId
    @EmployerProfileId UNIQUEIDENTIFIER,
    @VacanteId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc
    FROM dbo.Notificaciones
    WHERE EmployerProfileId = @EmployerProfileId
        AND (@VacanteId IS NULL OR VacanteId = @VacanteId)
    ORDER BY CreatedAtUtc DESC;
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

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO dbo.Users
        (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES
        (NEWID(), @AdminEmail, @AdminPasswordHash, N'ADMINISTRATOR', 1, 1, SYSUTCDATETIME());
END;
ELSE
BEGIN
    UPDATE dbo.Users
    SET
        PasswordHash = @AdminPasswordHash,
        Role = N'ADMINISTRATOR',
        IsActive = 1,
        EmailConfirmed = 1
    WHERE Email = @AdminEmail;
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

    UPDATE dbo.Users
    SET PasswordHash = @SeedPasswordHash,
        Role = N'EMPLOYER',
        IsActive = 1,
        EmailConfirmed = 1
    WHERE Email IN
    (
        N'empleador.servicios@test.local',
        N'empleador.comercio@test.local',
        N'empleador.educacion@test.local'
    );

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

    UPDATE dbo.EmployerProfiles
    SET Status = N'Active'
    WHERE UserId IN (@EmployerUserServicios, @EmployerUserComercio, @EmployerUserEducacion);

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
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@React, N'Introducción a React',
             N'Bases de componentes, estado, props y consumo simple de APIs para crear interfaces web.',
             N'Tecnología', 8, N'INA Virtual', N'Nacional', 1, 1, DATEADD(DAY, -8, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Excel)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@Excel, N'Excel para empleo',
             N'Funciones básicas, filtros, tablas y buenas prácticas para tareas administrativas iniciales.',
             N'Herramientas digitales', 6, N'INA', N'Nacional', 1, 1, DATEADD(DAY, -7, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @ServicioCliente)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@ServicioCliente, N'Servicio al cliente digital',
             N'Técnicas de atención, seguimiento de casos y comunicación clara en canales digitales.',
             N'Habilidades blandas', 5, N'Fundación Aliada CR', N'Nacional', 1, 1, DATEADD(DAY, -6, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @InglesEntrevistas)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@InglesEntrevistas, N'Inglés para entrevistas laborales',
             N'Frases y respuestas frecuentes para entrevistas de primer empleo en inglés.',
             N'Idiomas', 10, N'Coursera', N'Internacional', 1, 1, DATEADD(DAY, -5, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Comunicacion)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@Comunicacion, N'Comunicación efectiva en equipos',
             N'Prácticas para escuchar, sintetizar información y coordinar tareas en equipos de trabajo.',
             N'Habilidades blandas', 4, N'edX', N'Internacional', 1, 1, DATEADD(DAY, -4, @Now));

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
