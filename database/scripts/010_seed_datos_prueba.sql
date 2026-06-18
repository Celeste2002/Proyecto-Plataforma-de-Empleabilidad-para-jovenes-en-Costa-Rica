USE Plataforma_Empleabilidad_BD;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2'; -- 123456
    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    DECLARE @AdminUserId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

    DECLARE @CandidateUserAna UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @CandidateUserJose UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
    DECLARE @CandidateUserValeria UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

    DECLARE @CandidateAna UNIQUEIDENTIFIER = '11111111-aaaa-1111-aaaa-111111111111';
    DECLARE @CandidateJose UNIQUEIDENTIFIER = '22222222-aaaa-2222-aaaa-222222222222';
    DECLARE @CandidateValeria UNIQUEIDENTIFIER = '33333333-aaaa-3333-aaaa-333333333333';

    DECLARE @EmployerUserServicios UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
    DECLARE @EmployerUserComercio UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
    DECLARE @EmployerUserPendiente UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';

    DECLARE @EmployerServicios UNIQUEIDENTIFIER = '44444444-bbbb-4444-bbbb-444444444444';
    DECLARE @EmployerComercio UNIQUEIDENTIFIER = '55555555-bbbb-5555-bbbb-555555555555';
    DECLARE @EmployerPendiente UNIQUEIDENTIFIER = '66666666-bbbb-6666-bbbb-666666666666';

    DECLARE @VacanteSoporte UNIQUEIDENTIFIER = '77777777-0001-0001-0001-777777777777';
    DECLARE @VacanteFrontend UNIQUEIDENTIFIER = '77777777-0002-0002-0002-777777777777';
    DECLARE @VacanteVentas UNIQUEIDENTIFIER = '88888888-0001-0001-0001-888888888888';
    DECLARE @VacanteBodega UNIQUEIDENTIFIER = '88888888-0002-0002-0002-888888888888';
    DECLARE @VacanteCerrada UNIQUEIDENTIFIER = '99999999-0001-0001-0001-999999999999';

    DECLARE @PostAnaSoporte UNIQUEIDENTIFIER = 'aaaaaaaa-0001-0001-0001-000000000001';
    DECLARE @PostAnaFrontend UNIQUEIDENTIFIER = 'aaaaaaaa-0002-0002-0002-000000000002';
    DECLARE @PostJoseSoporte UNIQUEIDENTIFIER = 'bbbbbbbb-0001-0001-0001-000000000001';
    DECLARE @PostJoseVentas UNIQUEIDENTIFIER = 'bbbbbbbb-0002-0002-0002-000000000002';

    -- Usuarios base para login.
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@gmail.com')
    BEGIN
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@AdminUserId, N'admin@gmail.com', @PasswordHash, N'ADMINISTRATOR', 1, 1, DATEADD(DAY, -30, @Now));
    END
    ELSE
    BEGIN
        UPDATE dbo.Users
        SET PasswordHash = @PasswordHash, Role = N'ADMINISTRATOR', IsActive = 1, EmailConfirmed = 1
        WHERE Email = N'admin@gmail.com';
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'ana.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserAna, N'ana.candidato@test.local', @PasswordHash, N'CANDIDATE', 1, 1, DATEADD(DAY, -20, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'jose.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserJose, N'jose.candidato@test.local', @PasswordHash, N'CANDIDATE', 1, 1, DATEADD(DAY, -18, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'valeria.candidato@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@CandidateUserValeria, N'valeria.candidato@test.local', @PasswordHash, N'CANDIDATE', 1, 0, DATEADD(DAY, -12, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.servicios@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserServicios, N'empleador.servicios@test.local', @PasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -25, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.comercio@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserComercio, N'empleador.comercio@test.local', @PasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -22, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.pendiente@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserPendiente, N'empleador.pendiente@test.local', @PasswordHash, N'EMPLOYER', 1, 0, DATEADD(DAY, -5, @Now));

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, Role = N'CANDIDATE', IsActive = 1, EmailConfirmed = 1
    WHERE Email IN (N'ana.candidato@test.local', N'jose.candidato@test.local');

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, Role = N'CANDIDATE', IsActive = 1, EmailConfirmed = 0
    WHERE Email = N'valeria.candidato@test.local';

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, Role = N'EMPLOYER', IsActive = 1, EmailConfirmed = 1
    WHERE Email IN (N'empleador.servicios@test.local', N'empleador.comercio@test.local');

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, Role = N'EMPLOYER', IsActive = 1, EmailConfirmed = 0
    WHERE Email = N'empleador.pendiente@test.local';

    SELECT @CandidateUserAna = Id FROM dbo.Users WHERE Email = N'ana.candidato@test.local';
    SELECT @CandidateUserJose = Id FROM dbo.Users WHERE Email = N'jose.candidato@test.local';
    SELECT @CandidateUserValeria = Id FROM dbo.Users WHERE Email = N'valeria.candidato@test.local';
    SELECT @EmployerUserServicios = Id FROM dbo.Users WHERE Email = N'empleador.servicios@test.local';
    SELECT @EmployerUserComercio = Id FROM dbo.Users WHERE Email = N'empleador.comercio@test.local';
    SELECT @EmployerUserPendiente = Id FROM dbo.Users WHERE Email = N'empleador.pendiente@test.local';

    -- Perfiles de candidatos visibles para empleadores.
    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserAna)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateAna, @CandidateUserAna, N'Ana Morales', '2004-05-15', 22, N'San Jose', N'Universidad incompleta', 1, 1, NULL, DATEADD(DAY, -20, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserJose)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateJose, @CandidateUserJose, N'Jose Vargas', '2001-11-20', 24, N'Cartago', N'Tecnico', 1, 1, NULL, DATEADD(DAY, -18, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserValeria)
        INSERT INTO dbo.CandidateProfiles
            (Id, UserId, FullName, DateOfBirth, Age, Province, EducationLevel, IsVisibleToPartnerEmployers, IsAvailableForContact, PhotoUrl, CreatedAtUtc)
        VALUES
            (@CandidateValeria, @CandidateUserValeria, N'Valeria Jimenez', '1998-08-12', 27, N'Alajuela', N'Universidad completa', 1, 0, NULL, DATEADD(DAY, -12, @Now));

    SELECT @CandidateAna = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserAna;
    SELECT @CandidateJose = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserJose;
    SELECT @CandidateValeria = Id FROM dbo.CandidateProfiles WHERE UserId = @CandidateUserValeria;

    -- Perfiles de empleadores.
    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerServicios, @EmployerUserServicios, N'Sinergia Servicios Digitales', N'3-101-000001', N'Servicios', N'Laura Rojas', N'8888-1001', N'San Jose', N'Active', DATEADD(DAY, -25, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerComercio, @EmployerUserComercio, N'Comercial La Sabana', N'3-101-000002', N'Comercio', N'Marco Solis', N'8888-1002', N'Alajuela', N'Active', DATEADD(DAY, -22, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserPendiente)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerPendiente, @EmployerUserPendiente, N'Proyecto Pyme Pendiente', N'3-101-000003', N'Otro', N'Paula Castro', N'8888-1003', N'Heredia', N'PendingVerification', DATEADD(DAY, -5, @Now));

    SELECT @EmployerServicios = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios;
    SELECT @EmployerComercio = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio;
    SELECT @EmployerPendiente = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserPendiente;

    -- Vacantes para listar, postular y ver metricas.
    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteSoporte)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteSoporte, @EmployerServicios, N'Soporte tecnico junior', N'San Jose', N'Servicios', N'Presencial', N'Sin experiencia',
             N'Atencion de tickets, soporte basico a usuarios y seguimiento de casos.',
             N'Conocimiento basico de computadoras, comunicacion clara y disponibilidad para horario diurno.',
             N'CRC 380000 - 450000', 1, DATEADD(DAY, -3, @Now), DATEADD(DAY, -3, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteFrontend)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteFrontend, @EmployerServicios, N'Desarrollador frontend junior', N'Heredia', N'Servicios', N'Remoto', N'1-3 aÃ±os',
             N'Construccion de interfaces React y consumo de APIs internas.',
             N'React, JavaScript, CSS y manejo basico de Git.',
             N'CRC 550000 - 750000', 1, DATEADD(DAY, -2, @Now), DATEADD(DAY, -2, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteVentas)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteVentas, @EmployerComercio, N'Asistente de ventas', N'Alajuela', N'Comercio', N'Presencial', N'Menos de 1 aÃ±o',
             N'Apoyo en piso de ventas, caja y servicio al cliente.',
             N'Excelente trato al cliente, orden y disponibilidad fines de semana.',
             N'CRC 360000 - 420000', 1, DATEADD(DAY, -1, @Now), DATEADD(DAY, -1, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteBodega)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteBodega, @EmployerComercio, N'Auxiliar de bodega', N'Cartago', N'Comercio', N'Presencial', N'Sin experiencia',
             N'Recepcion, acomodo y control de inventario.',
             N'Orden, puntualidad y capacidad para trabajo fisico moderado.',
             N'CRC 350000 - 410000', 1, DATEADD(HOUR, -8, @Now), DATEADD(HOUR, -8, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacantes WHERE Id = @VacanteCerrada)
        INSERT INTO dbo.Vacantes
            (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel, Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
        VALUES
            (@VacanteCerrada, @EmployerServicios, N'Asistente administrativo cerrado', N'San Jose', N'Servicios', N'Presencial', N'1-3 aÃ±os',
             N'Vacante cerrada para probar reportes y filtros.',
             N'Manejo de documentos y servicio al cliente.',
             N'CRC 400000 - 500000', 0, DATEADD(DAY, -15, @Now), DATEADD(DAY, -15, @Now));

    -- Postulaciones en distintos estados.
    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteSoporte AND CandidateProfileId = @CandidateAna)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostAnaSoporte, @VacanteSoporte, @CandidateAna, N'Enviada', DATEADD(DAY, -1, @Now), DATEADD(DAY, -1, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteFrontend AND CandidateProfileId = @CandidateAna)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostAnaFrontend, @VacanteFrontend, @CandidateAna, N'En revisiÃ³n', DATEADD(HOUR, -18, @Now), DATEADD(HOUR, -12, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteSoporte AND CandidateProfileId = @CandidateJose)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostJoseSoporte, @VacanteSoporte, @CandidateJose, N'Vista', DATEADD(DAY, -2, @Now), DATEADD(DAY, -1, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Postulaciones WHERE VacanteId = @VacanteVentas AND CandidateProfileId = @CandidateJose)
        INSERT INTO dbo.Postulaciones (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
        VALUES (@PostJoseVentas, @VacanteVentas, @CandidateJose, N'Entrevista solicitada', DATEADD(HOUR, -10, @Now), DATEADD(HOUR, -2, @Now));

    -- Notificaciones asociadas a postulaciones.
    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostAnaSoporte)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostAnaSoporte, @VacanteSoporte, N'Ana Morales se ha postulado a la vacante Soporte tecnico junior', 0, DATEADD(DAY, -1, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostAnaFrontend)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostAnaFrontend, @VacanteFrontend, N'Ana Morales se ha postulado a la vacante Desarrollador frontend junior', 1, DATEADD(HOUR, -18, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostJoseSoporte)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerServicios, @PostJoseSoporte, @VacanteSoporte, N'Jose Vargas se ha postulado a la vacante Soporte tecnico junior', 0, DATEADD(DAY, -2, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE PostulacionId = @PostJoseVentas)
        INSERT INTO dbo.Notificaciones (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
        VALUES (NEWID(), @EmployerComercio, @PostJoseVentas, @VacanteVentas, N'Jose Vargas se ha postulado a la vacante Asistente de ventas', 0, DATEADD(HOUR, -10, @Now));

    -- Perfil completo de candidatos.
    IF NOT EXISTS (SELECT 1 FROM dbo.ExperienciasLaborales WHERE Id = '11111111-eeee-1111-eeee-111111111111')
        INSERT INTO dbo.ExperienciasLaborales
            (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
        VALUES
            ('11111111-eeee-1111-eeee-111111111111', @CandidateAna, N'Call Center Central', N'Agente de soporte', '2024-02-01', '2025-01-31', 0, N'Atencion telefonica y registro de casos.');

    IF NOT EXISTS (SELECT 1 FROM dbo.ExperienciasLaborales WHERE Id = '22222222-eeee-2222-eeee-222222222222')
        INSERT INTO dbo.ExperienciasLaborales
            (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
        VALUES
            ('22222222-eeee-2222-eeee-222222222222', @CandidateJose, N'Taller Cartago Norte', N'Asistente tecnico', '2023-06-01', NULL, 1, N'Apoyo en inventario, mantenimiento basico y atencion al cliente.');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '11111111-f111-1111-f111-111111111111')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('11111111-f111-1111-f111-111111111111', @CandidateAna, N'React');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '11111111-f222-1111-f222-111111111111')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('11111111-f222-1111-f222-111111111111', @CandidateAna, N'Servicio al cliente');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '22222222-f111-2222-f111-222222222222')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('22222222-f111-2222-f111-222222222222', @CandidateJose, N'Excel basico');

    IF NOT EXISTS (SELECT 1 FROM dbo.Habilidades WHERE Id = '33333333-f111-3333-f111-333333333333')
        INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
        VALUES ('33333333-f111-3333-f111-333333333333', @CandidateValeria, N'Comunicacion');

    IF NOT EXISTS (SELECT 1 FROM dbo.CursosCompletados WHERE Id = '11111111-c111-1111-c111-111111111111')
        INSERT INTO dbo.CursosCompletados
            (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
        VALUES
            ('11111111-c111-1111-c111-111111111111', @CandidateAna, N'Introduccion a React', N'Sinergia', '2025-11-15', 1);

    IF NOT EXISTS (SELECT 1 FROM dbo.CursosCompletados WHERE Id = '22222222-c111-2222-c111-222222222222')
        INSERT INTO dbo.CursosCompletados
            (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
        VALUES
            ('22222222-c111-2222-c111-222222222222', @CandidateJose, N'Excel para empleo', N'INA', '2025-09-10', 0);

    COMMIT TRANSACTION;

    SELECT
        (SELECT COUNT(*) FROM dbo.Users) AS TotalUsers,
        (SELECT COUNT(*) FROM dbo.CandidateProfiles) AS TotalCandidates,
        (SELECT COUNT(*) FROM dbo.EmployerProfiles) AS TotalEmployers,
        (SELECT COUNT(*) FROM dbo.Vacantes) AS TotalVacantes,
        (SELECT COUNT(*) FROM dbo.Postulaciones) AS TotalPostulaciones,
        (SELECT COUNT(*) FROM dbo.Notificaciones WHERE IsRead = 0) AS NotificacionesSinLeer;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
