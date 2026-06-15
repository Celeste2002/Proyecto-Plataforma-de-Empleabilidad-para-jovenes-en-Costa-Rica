-- HU7 — Panel de Gestión de Candidatos para Empleador
-- Crea la tabla Mensajes para mensajería directa empleador → candidato en plataforma

CREATE TABLE dbo.Mensajes (
    Id                          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Mensajes_Id          DEFAULT NEWID(),
    PostulacionId               UNIQUEIDENTIFIER NOT NULL,
    SenderEmployerProfileId     UNIQUEIDENTIFIER NOT NULL,
    RecipientCandidateProfileId UNIQUEIDENTIFIER NOT NULL,
    Body                        NVARCHAR(2000)   NOT NULL,
    SentAtUtc                   DATETIME2        NOT NULL CONSTRAINT DF_Mensajes_SentAtUtc   DEFAULT GETUTCDATE(),
    IsReadByCandidate           BIT              NOT NULL CONSTRAINT DF_Mensajes_IsRead       DEFAULT 0,
    CONSTRAINT PK_Mensajes PRIMARY KEY (Id),
    CONSTRAINT FK_Mensajes_Postulaciones
        FOREIGN KEY (PostulacionId) REFERENCES dbo.Postulaciones (Id),
    CONSTRAINT FK_Mensajes_EmployerProfiles
        FOREIGN KEY (SenderEmployerProfileId) REFERENCES dbo.EmployerProfiles (Id),
    CONSTRAINT FK_Mensajes_CandidateProfiles
        FOREIGN KEY (RecipientCandidateProfileId) REFERENCES dbo.CandidateProfiles (Id)
);

CREATE INDEX IX_Mensajes_PostulacionId
    ON dbo.Mensajes (PostulacionId);

CREATE INDEX IX_Mensajes_RecipientCandidateProfileId
    ON dbo.Mensajes (RecipientCandidateProfileId);
