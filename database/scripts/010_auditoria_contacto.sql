-- HU20 — Contacto entre Empleador y Candidato
-- Registra cada vez que un empleador accede al correo electrónico de un candidato (AC3).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContactoAccesos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ContactoAccesos (
        Id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ContactoAccesos_Id         DEFAULT NEWID(),
        EmployerProfileId UNIQUEIDENTIFIER NOT NULL,
        PostulacionId     UNIQUEIDENTIFIER NOT NULL,
        CandidateEmail    NVARCHAR(200)    NOT NULL,
        AccessedAtUtc     DATETIME2        NOT NULL CONSTRAINT DF_ContactoAccesos_AccessedAt DEFAULT GETUTCDATE(),
        CONSTRAINT PK_ContactoAccesos PRIMARY KEY (Id),
        CONSTRAINT FK_ContactoAccesos_EmployerProfiles FOREIGN KEY (EmployerProfileId)
            REFERENCES dbo.EmployerProfiles (Id),
        CONSTRAINT FK_ContactoAccesos_Postulaciones FOREIGN KEY (PostulacionId)
            REFERENCES dbo.Postulaciones (Id)
    );

    CREATE INDEX IX_ContactoAccesos_EmployerProfileId ON dbo.ContactoAccesos (EmployerProfileId);
    CREATE INDEX IX_ContactoAccesos_PostulacionId     ON dbo.ContactoAccesos (PostulacionId);
END;
