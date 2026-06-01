USE Plataforma_Empleabilidad_BD;
GO

/* =============================================================
   TABLA: EmployerProfiles  (perfil del empleador / PYME)
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
