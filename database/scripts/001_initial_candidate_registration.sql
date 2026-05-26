IF DB_ID(N'Plataforma_Empleabilidad_BD') IS NULL
BEGIN
    CREATE DATABASE Plataforma_Empleabilidad_BD;
END;
GO

USE Plataforma_Empleabilidad_BD;
GO

IF OBJECT_ID(N'dbo.CandidateProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CandidateProfiles
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_CandidateProfiles PRIMARY KEY,

        FullName NVARCHAR(160) NOT NULL,
        Age INT NOT NULL,
        Province NVARCHAR(40) NOT NULL,
        EducationLevel NVARCHAR(80) NOT NULL,
        Email NVARCHAR(254) NOT NULL,
        IsVisibleToPartnerEmployers BIT NOT NULL
            CONSTRAINT DF_CandidateProfiles_IsVisibleToPartnerEmployers DEFAULT 1,
        EmailConfirmationSent BIT NOT NULL
            CONSTRAINT DF_CandidateProfiles_EmailConfirmationSent DEFAULT 0,
        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_CandidateProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT CK_CandidateProfiles_Age
            CHECK (Age BETWEEN 18 AND 30),

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

        CONSTRAINT CK_CandidateProfiles_EducationLevel
            CHECK (EducationLevel IN
            (
                N'Secundaria incompleta',
                N'Secundaria completa',
                N'Tecnico',
                N'Universidad incompleta',
                N'Universidad completa'
            ))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_CandidateProfiles_Email'
      AND object_id = OBJECT_ID(N'dbo.CandidateProfiles')
)
BEGIN
    CREATE UNIQUE INDEX UX_CandidateProfiles_Email
        ON dbo.CandidateProfiles (Email);
END;
GO

IF OBJECT_ID(N'dbo.PartnerEmployerVisibleCandidateProfiles', N'V') IS NULL
BEGIN
    EXEC(N'
        CREATE VIEW dbo.PartnerEmployerVisibleCandidateProfiles
        AS
        SELECT
            Id,
            FullName,
            Age,
            Province,
            EducationLevel,
            Email,
            EmailConfirmationSent,
            CreatedAtUtc
        FROM dbo.CandidateProfiles
        WHERE IsVisibleToPartnerEmployers = 1;
    ');
END;
GO
