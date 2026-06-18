-- HU-Empleador: Publicar oferta de trabajo con requisitos y salario
-- Agrega columnas Requirements y SalaryRange a la tabla Vacantes

IF COL_LENGTH('dbo.Vacantes', 'Requirements') IS NULL
BEGIN
    ALTER TABLE dbo.Vacantes
        ADD Requirements NVARCHAR(MAX) NULL;
END;
GO

IF COL_LENGTH('dbo.Vacantes', 'SalaryRange') IS NULL
BEGIN
    ALTER TABLE dbo.Vacantes
        ADD SalaryRange NVARCHAR(100) NULL;
END;
GO
