USE Plataforma_Empleabilidad_BD;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.HabilidadesBlandasSugeridas', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.HabilidadesBlandasSugeridas
        (
            Id UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_Id DEFAULT NEWID()
                CONSTRAINT PK_HabilidadesBlandasSugeridas PRIMARY KEY,

            Nombre NVARCHAR(100) NOT NULL,

            DisplayOrder INT NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_DisplayOrder DEFAULT 0,

            IsActive BIT NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_IsActive DEFAULT 1,

            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_HabilidadesBlandasSugeridas_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

            CONSTRAINT UQ_HabilidadesBlandasSugeridas_Nombre UNIQUE (Nombre)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_HabilidadesBlandasSugeridas_Active_Order'
            AND object_id = OBJECT_ID(N'dbo.HabilidadesBlandasSugeridas')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_HabilidadesBlandasSugeridas_Active_Order
            ON dbo.HabilidadesBlandasSugeridas (IsActive, DisplayOrder, Nombre);
    END;

    DECLARE @Seed TABLE
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        DisplayOrder INT NOT NULL
    );

    INSERT INTO @Seed (Id, Nombre, DisplayOrder)
    VALUES
        ('13131313-0001-0001-0001-131313131313', N'Comunicaci' + NCHAR(243) + N'n efectiva', 10),
        ('13131313-0002-0002-0002-131313131313', N'Trabajo en equipo', 20),
        ('13131313-0003-0003-0003-131313131313', N'Servicio al cliente', 30),
        ('13131313-0004-0004-0004-131313131313', N'Resoluci' + NCHAR(243) + N'n de problemas', 40),
        ('13131313-0005-0005-0005-131313131313', N'Adaptabilidad', 50),
        ('13131313-0006-0006-0006-131313131313', N'Responsabilidad', 60),
        ('13131313-0007-0007-0007-131313131313', N'Puntualidad', 70),
        ('13131313-0008-0008-0008-131313131313', N'Proactividad', 80),
        ('13131313-0009-0009-0009-131313131313', N'Organizaci' + NCHAR(243) + N'n', 90),
        ('13131313-0010-0010-0010-131313131313', N'Gesti' + NCHAR(243) + N'n del tiempo', 100),
        ('13131313-0011-0011-0011-131313131313', N'Atenci' + NCHAR(243) + N'n al detalle', 110),
        ('13131313-0012-0012-0012-131313131313', N'Empat' + NCHAR(237) + N'a', 120),
        ('13131313-0013-0013-0013-131313131313', N'Liderazgo', 130),
        ('13131313-0014-0014-0014-131313131313', N'Pensamiento cr' + NCHAR(237) + N'tico', 140),
        ('13131313-0015-0015-0015-131313131313', N'Aprendizaje r' + NCHAR(225) + N'pido', 150);

    UPDATE Existing
    SET Nombre = Seed.Nombre,
        DisplayOrder = Seed.DisplayOrder,
        IsActive = 1
    FROM dbo.HabilidadesBlandasSugeridas Existing
    INNER JOIN @Seed Seed
        ON Existing.Id = Seed.Id;

    UPDATE Existing
    SET DisplayOrder = Seed.DisplayOrder,
        IsActive = 1
    FROM dbo.HabilidadesBlandasSugeridas Existing
    INNER JOIN @Seed Seed
        ON Existing.Nombre = Seed.Nombre;

    INSERT INTO dbo.HabilidadesBlandasSugeridas (Id, Nombre, DisplayOrder, IsActive)
    SELECT Seed.Id, Seed.Nombre, Seed.DisplayOrder, 1
    FROM @Seed Seed
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.HabilidadesBlandasSugeridas Existing
        WHERE Existing.Id = Seed.Id
    )
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.HabilidadesBlandasSugeridas Existing
            WHERE Existing.Nombre = Seed.Nombre
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Candidate_GetHabilidadesBlandasSugeridas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Nombre
    FROM dbo.HabilidadesBlandasSugeridas
    WHERE IsActive = 1
    ORDER BY DisplayOrder, Nombre;
END;
GO
