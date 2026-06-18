USE Plataforma_Empleabilidad_BD;
GO

-- HU8 / HU19 - Catalogo de microcursos y recomendaciones por habilidades.

IF OBJECT_ID(N'dbo.MicroCursos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MicroCursos
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_MicroCursos_Id DEFAULT NEWID()
            CONSTRAINT PK_MicroCursos PRIMARY KEY,

        Titulo NVARCHAR(160) NOT NULL,
        Descripcion NVARCHAR(1000) NOT NULL,
        Area NVARCHAR(80) NOT NULL,
        DuracionHoras INT NOT NULL
            CONSTRAINT CK_MicroCursos_DuracionHoras CHECK (DuracionHoras > 0),
        EntidadProveedora NVARCHAR(200) NOT NULL,
        TipoProveedor NVARCHAR(30) NOT NULL
            CONSTRAINT CK_MicroCursos_TipoProveedor CHECK (TipoProveedor IN (N'Nacional', N'Internacional')),
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
            CONSTRAINT DF_MicroCursoHabilidades_Id DEFAULT NEWID()
            CONSTRAINT PK_MicroCursoHabilidades PRIMARY KEY,

        MicroCursoId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MicroCursoHabilidades_MicroCursos
                FOREIGN KEY REFERENCES dbo.MicroCursos (Id) ON DELETE CASCADE,

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
                FOREIGN KEY REFERENCES dbo.MicroCursos (Id) ON DELETE CASCADE,

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

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_MicroCursos_IsActive_Area'
        AND object_id = OBJECT_ID(N'dbo.MicroCursos')
)
    CREATE INDEX IX_MicroCursos_IsActive_Area ON dbo.MicroCursos (IsActive, Area);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_MicroCursoHabilidades_Nombre'
        AND object_id = OBJECT_ID(N'dbo.MicroCursoHabilidades')
)
    CREATE INDEX IX_MicroCursoHabilidades_Nombre ON dbo.MicroCursoHabilidades (Nombre);
GO
