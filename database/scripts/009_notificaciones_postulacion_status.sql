-- HU-Candidato/Empleador: Postulación y sistema de notificaciones
-- Crea la tabla Notificaciones para alertas de nuevas postulaciones al empleador.
-- No se necesita ALTER TABLE en Postulaciones porque la columna Status no tiene CHECK constraint.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notificaciones' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Notificaciones (
        Id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EmployerProfileId UNIQUEIDENTIFIER NOT NULL,
        PostulacionId     UNIQUEIDENTIFIER NOT NULL,
        VacanteId         UNIQUEIDENTIFIER NOT NULL,
        Message           NVARCHAR(300)    NOT NULL,
        IsRead            BIT              NOT NULL CONSTRAINT DF_Notificaciones_IsRead    DEFAULT 0,
        CreatedAtUtc      DATETIME2        NOT NULL CONSTRAINT DF_Notificaciones_CreatedAt DEFAULT GETUTCDATE(),
        CONSTRAINT PK_Notificaciones PRIMARY KEY (Id),
        CONSTRAINT FK_Notificaciones_EmployerProfiles FOREIGN KEY (EmployerProfileId)
            REFERENCES dbo.EmployerProfiles (Id),
        CONSTRAINT FK_Notificaciones_Postulaciones FOREIGN KEY (PostulacionId)
            REFERENCES dbo.Postulaciones (Id),
        CONSTRAINT FK_Notificaciones_Vacantes FOREIGN KEY (VacanteId)
            REFERENCES dbo.Vacantes (Id)
    );

    CREATE INDEX IX_Notificaciones_EmployerProfileId ON dbo.Notificaciones (EmployerProfileId);
    CREATE INDEX IX_Notificaciones_VacanteId         ON dbo.Notificaciones (VacanteId);
END;
