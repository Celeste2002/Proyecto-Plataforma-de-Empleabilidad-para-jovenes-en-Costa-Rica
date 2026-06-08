USE Plataforma_Empleabilidad_BD;
GO

/* =============================================================
   PATCH 003: usar fecha de nacimiento y actualizar perfil
   ============================================================= */

IF COL_LENGTH(N'dbo.CandidateProfiles', N'DateOfBirth') IS NULL
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ADD DateOfBirth DATE NULL;
END;
GO

UPDATE dbo.CandidateProfiles
SET DateOfBirth = DATEFROMPARTS(YEAR(SYSUTCDATETIME()) - Age, 1, 1)
WHERE DateOfBirth IS NULL
    AND COL_LENGTH(N'dbo.CandidateProfiles', N'Age') IS NOT NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CandidateProfiles')
        AND name = N'DateOfBirth'
        AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ALTER COLUMN DateOfBirth DATE NOT NULL;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CandidateProfiles')
        AND name = N'Age'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.CandidateProfiles
        ALTER COLUMN Age INT NULL;
END;
GO

CREATE OR ALTER VIEW dbo.PartnerEmployerVisibleCandidateProfiles
AS
SELECT
    cp.Id,
    cp.FullName,
    cp.DateOfBirth,
    DATEDIFF(YEAR, cp.DateOfBirth, CAST(SYSUTCDATETIME() AS DATE))
        - CASE
            WHEN DATEADD(YEAR, DATEDIFF(YEAR, cp.DateOfBirth, CAST(SYSUTCDATETIME() AS DATE)), cp.DateOfBirth)
                > CAST(SYSUTCDATETIME() AS DATE)
            THEN 1
            ELSE 0
        END AS Age,
    cp.Province,
    cp.EducationLevel,
    u.Email,
    u.EmailConfirmed,
    cp.CreatedAtUtc
FROM dbo.CandidateProfiles cp
INNER JOIN dbo.Users u
    ON cp.UserId = u.Id
WHERE cp.IsVisibleToPartnerEmployers = 1;
GO

