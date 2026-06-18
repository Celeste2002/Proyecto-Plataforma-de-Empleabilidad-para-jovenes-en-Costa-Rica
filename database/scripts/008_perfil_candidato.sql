-- HU9: Perfil público del candidato
-- Agrega foto, disponibilidad para contacto y secciones de experiencia, habilidades y cursos

ALTER TABLE dbo.CandidateProfiles
    ADD PhotoUrl             NVARCHAR(500) NULL,
        IsAvailableForContact BIT           NOT NULL DEFAULT 1;

CREATE TABLE dbo.ExperienciasLaborales
(
    Id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    CandidateProfileId UNIQUEIDENTIFIER NOT NULL
        REFERENCES dbo.CandidateProfiles (Id) ON DELETE CASCADE,
    Empresa            NVARCHAR(200)    NOT NULL,
    Cargo              NVARCHAR(200)    NOT NULL,
    FechaInicio        DATE             NOT NULL,
    FechaFin           DATE             NULL,
    EsTrabajoActual    BIT              NOT NULL DEFAULT 0,
    Descripcion        NVARCHAR(1000)   NULL
);

CREATE TABLE dbo.Habilidades
(
    Id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    CandidateProfileId UNIQUEIDENTIFIER NOT NULL
        REFERENCES dbo.CandidateProfiles (Id) ON DELETE CASCADE,
    Nombre             NVARCHAR(100)    NOT NULL
);

CREATE TABLE dbo.CursosCompletados
(
    Id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    CandidateProfileId UNIQUEIDENTIFIER NOT NULL
        REFERENCES dbo.CandidateProfiles (Id) ON DELETE CASCADE,
    NombreCurso        NVARCHAR(200)    NOT NULL,
    Institucion        NVARCHAR(200)    NOT NULL,
    FechaCompletado    DATE             NOT NULL,
    EsDePlataforma     BIT              NOT NULL DEFAULT 0
);
