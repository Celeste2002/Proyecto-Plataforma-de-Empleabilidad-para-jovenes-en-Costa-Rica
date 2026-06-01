IF DB_ID(N'Plataforma_Empleabilidad_BD') IS NULL
BEGIN
    CREATE DATABASE Plataforma_Empleabilidad_BD;
END;
GO

USE Plataforma_Empleabilidad_BD;
GO

/* =============================================================
   TABLA: Users  (autenticacion y roles)
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

/* =============================================================
   TABLA: CandidateProfiles  (perfil del candidato)
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

        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_CandidateProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

/* =============================================================
   VISTA: PartnerEmployerVisibleCandidateProfiles
   ============================================================= */

IF OBJECT_ID(N'dbo.PartnerEmployerVisibleCandidateProfiles', N'V') IS NULL
BEGIN
    EXEC(N'
        CREATE VIEW dbo.PartnerEmployerVisibleCandidateProfiles
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
            u.Email,
            u.EmailConfirmed,
            cp.CreatedAtUtc
        FROM dbo.CandidateProfiles cp
        INNER JOIN dbo.Users u
            ON cp.UserId = u.Id
        WHERE cp.IsVisibleToPartnerEmployers = 1;
    ');
END;
GO

/* =========================================================
   CREAR ADMIN Y CAMBIAR CONTRASENIA A 123456
========================================================= */

DECLARE @AdminEmail NVARCHAR(254) = N'admin@gmail.com';
DECLARE @Hash NVARCHAR(500) = N'123456';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO dbo.Users
        (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES
        (NEWID(), @AdminEmail, @Hash, N'ADMINISTRATOR', 1, 1, SYSUTCDATETIME());

    PRINT 'Administrador creado';
END;
GO

UPDATE dbo.Users
SET PasswordHash = '$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2'   -- pega el hash exacto aquí
WHERE Email = 'admin@gmail.com';

