-- HU5 / HU16 — Vacantes y Postulaciones
-- Crea las tablas Vacantes (ofertas de empleo) y Postulaciones (solicitudes de candidatos)

CREATE TABLE dbo.Vacantes (
    Id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    EmployerProfileId UNIQUEIDENTIFIER NOT NULL,
    JobTitle          NVARCHAR(100)    NOT NULL,
    Province          NVARCHAR(50)     NOT NULL,
    Sector            NVARCHAR(50)     NOT NULL,
    Modality          NVARCHAR(30)     NOT NULL,
    ExperienceLevel   NVARCHAR(50)     NOT NULL,
    Description       NVARCHAR(MAX)    NULL,
    IsActive          BIT              NOT NULL CONSTRAINT DF_Vacantes_IsActive DEFAULT 1,
    PublishedAt       DATETIME2        NOT NULL CONSTRAINT DF_Vacantes_PublishedAt DEFAULT GETUTCDATE(),
    CreatedAtUtc      DATETIME2        NOT NULL CONSTRAINT DF_Vacantes_CreatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Vacantes PRIMARY KEY (Id),
    CONSTRAINT FK_Vacantes_EmployerProfiles FOREIGN KEY (EmployerProfileId)
        REFERENCES dbo.EmployerProfiles (Id)
);

CREATE TABLE dbo.Postulaciones (
    Id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    VacanteId          UNIQUEIDENTIFIER NOT NULL,
    CandidateProfileId UNIQUEIDENTIFIER NOT NULL,
    Status             NVARCHAR(30)     NOT NULL CONSTRAINT DF_Postulaciones_Status DEFAULT 'Enviada',
    AppliedAt          DATETIME2        NOT NULL CONSTRAINT DF_Postulaciones_AppliedAt DEFAULT GETUTCDATE(),
    UpdatedAtUtc       DATETIME2        NOT NULL CONSTRAINT DF_Postulaciones_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Postulaciones PRIMARY KEY (Id),
    CONSTRAINT FK_Postulaciones_Vacantes FOREIGN KEY (VacanteId)
        REFERENCES dbo.Vacantes (Id),
    CONSTRAINT FK_Postulaciones_CandidateProfiles FOREIGN KEY (CandidateProfileId)
        REFERENCES dbo.CandidateProfiles (Id),
    CONSTRAINT UQ_Postulaciones_Vacante_Candidate UNIQUE (VacanteId, CandidateProfileId)
);
