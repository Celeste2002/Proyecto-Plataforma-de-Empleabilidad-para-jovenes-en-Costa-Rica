-- HU-Empleador: Publicar oferta de trabajo con requisitos y salario
-- Agrega columnas Requirements y SalaryRange a la tabla Vacantes

ALTER TABLE dbo.Vacantes
    ADD Requirements NVARCHAR(MAX) NULL,
        SalaryRange   NVARCHAR(100) NULL;
