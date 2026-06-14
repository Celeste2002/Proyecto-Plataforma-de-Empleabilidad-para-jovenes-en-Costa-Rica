USE Plataforma_Empleabilidad_BD;
GO

/*
    Datos de prueba para:
    - Login como admin, empleador y candidato
    - Crear vacantes activas / desactivadas
    - Probar postulaciones y la desactivacion sin borrado

    Contraseña de prueba para todas las cuentas: 123456
*/

SET NOCOUNT ON;

DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$HK.Fr.TCSkHMqrTbTi3TJ.pnqMc76AJgWed3mUWjYBmZNasY.g3wW';

DECLARE @AdminEmail NVARCHAR(254) = N'admin@gmail.com';
DECLARE @EmployerEmail NVARCHAR(254) = N'empleador.demo@sinergia.cr';
DECLARE @CandidateEmail NVARCHAR(254) = N'candidato.demo@sinergia.cr';

DECLARE @AdminUserId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @EmployerUserId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @CandidateUserId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

DECLARE @CandidateProfileId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @EmployerProfileId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

DECLARE @VacanteActivaId UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';
DECLARE @VacanteInactivaId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';
DECLARE @PostulacionId UNIQUEIDENTIFIER = '88888888-8888-8888-8888-888888888888';

DECLARE @ResolvedAdminUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedEmployerUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedCandidateUserId UNIQUEIDENTIFIER;
DECLARE @ResolvedEmployerProfileId UNIQUEIDENTIFIER;
DECLARE @ResolvedCandidateProfileId UNIQUEIDENTIFIER;

SELECT @ResolvedAdminUserId = Id FROM dbo.Users WHERE Email = @AdminEmail;
IF @ResolvedAdminUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@AdminUserId, @AdminEmail, @PasswordHash, N'ADMINISTRATOR', 1, 1, SYSUTCDATETIME());
    SET @ResolvedAdminUserId = @AdminUserId;
END
ELSE
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash,
        Role = N'ADMINISTRATOR',
        IsActive = 1,
        EmailConfirmed = 1
    WHERE Email = @AdminEmail;
END

SELECT @ResolvedEmployerUserId = Id FROM dbo.Users WHERE Email = @EmployerEmail;
IF @ResolvedEmployerUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@EmployerUserId, @EmployerEmail, @PasswordHash, N'EMPLOYER', 1, 1, SYSUTCDATETIME());
    SET @ResolvedEmployerUserId = @EmployerUserId;
END
ELSE
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash,
        Role = N'EMPLOYER',
        IsActive = 1,
        EmailConfirmed = 1
    WHERE Email = @EmployerEmail;
END

SELECT @ResolvedCandidateUserId = Id FROM dbo.Users WHERE Email = @CandidateEmail;
IF @ResolvedCandidateUserId IS NULL
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
    VALUES (@CandidateUserId, @CandidateEmail, @PasswordHash, N'CANDIDATE', 1, 1, SYSUTCDATETIME());
    SET @ResolvedCandidateUserId = @CandidateUserId;
END
ELSE
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash,
        Role = N'CANDIDATE',
        IsActive = 1,
        EmailConfirmed = 1
    WHERE Email = @CandidateEmail;
END

IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @ResolvedEmployerUserId)
BEGIN
    INSERT INTO dbo.EmployerProfiles
        (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
    VALUES
        (
            @EmployerProfileId,
            @ResolvedEmployerUserId,
            N'Comercial Demo Costa Rica S.A.',
            N'3-101-999999',
            N'Servicios',
            N'Ana Solano',
            N'+506 8888-1111',
            N'San Jose, Costa Rica',
            N'Active',
            SYSUTCDATETIME()
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @ResolvedCandidateUserId)
BEGIN
    INSERT INTO dbo.CandidateProfiles
        (Id, UserId, FullName, DateOfBirth, Province, EducationLevel, IsVisibleToPartnerEmployers, CreatedAtUtc)
    VALUES
        (
            @CandidateProfileId,
            @ResolvedCandidateUserId,
            N'Luis Enrique Mora',
            '2004-04-18',
            N'San Jose',
            N'Secundaria completa',
            1,
            SYSUTCDATETIME()
        );
END

SELECT @ResolvedEmployerProfileId = Id
FROM dbo.EmployerProfiles
WHERE UserId = @ResolvedEmployerUserId;

SELECT @ResolvedCandidateProfileId = Id
FROM dbo.CandidateProfiles
WHERE UserId = @ResolvedCandidateUserId;

IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteActivaId)
BEGIN
    INSERT INTO dbo.Vacantes
        (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, IsActive, PublishedAt, CreatedAtUtc)
    VALUES
        (
            @VacanteActivaId,
            @ResolvedEmployerProfileId,
            N'Asistente de servicio al cliente',
            N'San Jose',
            N'Servicios',
            N'Presencial',
            N'Sin experiencia',
            N'Vacante de prueba para validar postulación y desactivación sin eliminar el registro.',
            1,
            DATEADD(DAY, -1, SYSUTCDATETIME()),
            DATEADD(DAY, -1, SYSUTCDATETIME())
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteInactivaId)
BEGIN
    INSERT INTO dbo.Vacantes
        (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, IsActive, PublishedAt, CreatedAtUtc)
    VALUES
        (
            @VacanteInactivaId,
            @ResolvedEmployerProfileId,
            N'Auxiliar administrativo',
            N'Alajuela',
            N'Servicios',
            N'Híbrido',
            N'1-3 años',
            N'Esta vacante inicia desactivada para probar el cambio de estado en el panel.',
            0,
            DATEADD(DAY, -2, SYSUTCDATETIME()),
            DATEADD(DAY, -2, SYSUTCDATETIME())
        );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE Id = @PostulacionId)
BEGIN
    INSERT INTO dbo.Postulaciones
        (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
    VALUES
        (
            @PostulacionId,
            @VacanteActivaId,
            @ResolvedCandidateProfileId,
            N'Enviada',
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
END

PRINT N'Datos de prueba cargados.';

SELECT
    Email,
    Role,
    IsActive,
    EmailConfirmed,
    CreatedAtUtc
FROM dbo.Users
WHERE Email IN (@AdminEmail, @EmployerEmail, @CandidateEmail)
ORDER BY Email;

SELECT
    v.Id,
    v.JobTitle,
    ep.CompanyName,
    v.Province,
    v.Sector,
    v.Modality,
    v.ExperienceLevel,
    v.IsActive,
    v.PublishedAt
FROM dbo.Vacantes v
INNER JOIN dbo.EmployerProfiles ep ON ep.Id = v.EmployerProfileId
WHERE v.Id IN (@VacanteActivaId, @VacanteInactivaId)
ORDER BY v.PublishedAt DESC;

SELECT
    p.Id,
    p.Status,
    p.AppliedAt,
    v.JobTitle,
    u.Email AS CandidateEmail
FROM dbo.Postulaciones p
INNER JOIN dbo.Vacantes v ON v.Id = p.VacanteId
INNER JOIN dbo.CandidateProfiles cp ON cp.Id = p.CandidateProfileId
INNER JOIN dbo.Users u ON u.Id = cp.UserId
WHERE p.Id = @PostulacionId;
