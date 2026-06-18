USE Plataforma_Empleabilidad_BD;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$MVE8phT12S0aoXgsySzqueuUwl0pLiWd2dun5QcSGh0DV9aPll4M2'; -- 123456
    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    DECLARE @EmployerUserServicios UNIQUEIDENTIFIER;
    DECLARE @EmployerUserComercio UNIQUEIDENTIFIER;
    DECLARE @EmployerUserEducacion UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777771';

    DECLARE @EmployerServicios UNIQUEIDENTIFIER;
    DECLARE @EmployerComercio UNIQUEIDENTIFIER;
    DECLARE @EmployerEducacion UNIQUEIDENTIFIER = '77777777-bbbb-7777-bbbb-777777777771';

    SELECT @EmployerUserServicios = Id FROM dbo.Users WHERE Email = N'empleador.servicios@test.local';
    SELECT @EmployerUserComercio = Id FROM dbo.Users WHERE Email = N'empleador.comercio@test.local';

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'empleador.educacion@test.local')
        INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
        VALUES (@EmployerUserEducacion, N'empleador.educacion@test.local', @PasswordHash, N'EMPLOYER', 1, 1, DATEADD(DAY, -10, @Now));

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, Role = N'EMPLOYER', IsActive = 1, EmailConfirmed = 1
    WHERE Email = N'empleador.educacion@test.local';

    SELECT @EmployerUserEducacion = Id FROM dbo.Users WHERE Email = N'empleador.educacion@test.local';

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserEducacion)
        INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
        VALUES
            (@EmployerEducacion, @EmployerUserEducacion, N'Academia Aliada CR', N'3-101-000004', N'Servicios',
             N'Natalia Mena', N'8888-1004', N'San Jose', N'Active', DATEADD(DAY, -10, @Now));

    SELECT @EmployerServicios = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserServicios;
    SELECT @EmployerComercio = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserComercio;
    SELECT @EmployerEducacion = Id FROM dbo.EmployerProfiles WHERE UserId = @EmployerUserEducacion;

    DECLARE @React UNIQUEIDENTIFIER = '12121212-0001-0001-0001-121212121212';
    DECLARE @Excel UNIQUEIDENTIFIER = '12121212-0002-0002-0002-121212121212';
    DECLARE @ServicioCliente UNIQUEIDENTIFIER = '12121212-0003-0003-0003-121212121212';
    DECLARE @InglesEntrevistas UNIQUEIDENTIFIER = '12121212-0004-0004-0004-121212121212';
    DECLARE @Comunicacion UNIQUEIDENTIFIER = '12121212-0005-0005-0005-121212121212';

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @React)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@React, N'Introduccion a React',
             N'Bases de componentes, estado, props y consumo simple de APIs para crear interfaces web.',
             N'Tecnologia', 8, N'INA Virtual', N'Nacional', 1, 1, DATEADD(DAY, -8, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Excel)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@Excel, N'Excel para empleo',
             N'Funciones basicas, filtros, tablas y buenas practicas para tareas administrativas iniciales.',
             N'Herramientas digitales', 6, N'INA', N'Nacional', 1, 1, DATEADD(DAY, -7, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @ServicioCliente)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@ServicioCliente, N'Servicio al cliente digital',
             N'Tecnicas de atencion, seguimiento de casos y comunicacion clara en canales digitales.',
             N'Habilidades blandas', 5, N'Fundacion Aliada CR', N'Nacional', 1, 1, DATEADD(DAY, -6, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @InglesEntrevistas)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@InglesEntrevistas, N'Ingles para entrevistas laborales',
             N'Frases y respuestas frecuentes para entrevistas de primer empleo en ingles.',
             N'Idiomas', 10, N'Coursera', N'Internacional', 1, 1, DATEADD(DAY, -5, @Now));

    IF NOT EXISTS (SELECT 1 FROM dbo.MicroCursos WHERE Id = @Comunicacion)
        INSERT INTO dbo.MicroCursos
            (Id, Titulo, Descripcion, Area, DuracionHoras, EntidadProveedora, TipoProveedor, OtorgaCertificacion, IsActive, CreatedAtUtc)
        VALUES
            (@Comunicacion, N'Comunicacion efectiva en equipos',
             N'Practicas para escuchar, sintetizar informacion y coordinar tareas en equipos de trabajo.',
             N'Habilidades blandas', 4, N'edX', N'Internacional', 1, 1, DATEADD(DAY, -4, @Now));

    INSERT INTO dbo.MicroCursoHabilidades (MicroCursoId, Nombre)
    SELECT Seed.MicroCursoId, Seed.Nombre
    FROM (VALUES
        (@React, N'React'),
        (@React, N'JavaScript'),
        (@React, N'CSS'),
        (@Excel, N'Excel basico'),
        (@Excel, N'Analisis de datos'),
        (@ServicioCliente, N'Servicio al cliente'),
        (@ServicioCliente, N'Comunicacion'),
        (@InglesEntrevistas, N'Ingles'),
        (@InglesEntrevistas, N'Comunicacion'),
        (@Comunicacion, N'Comunicacion'),
        (@Comunicacion, N'Trabajo en equipo')
    ) AS Seed(MicroCursoId, Nombre)
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.MicroCursoHabilidades Existing
        WHERE Existing.MicroCursoId = Seed.MicroCursoId
            AND Existing.Nombre = Seed.Nombre
    );

    INSERT INTO dbo.MicroCursoValidacionesEmpleador (MicroCursoId, EmployerProfileId, ValidatedAtUtc)
    SELECT Seed.MicroCursoId, Seed.EmployerProfileId, DATEADD(DAY, -2, @Now)
    FROM (VALUES
        (@React, @EmployerServicios), (@React, @EmployerComercio), (@React, @EmployerEducacion),
        (@Excel, @EmployerServicios), (@Excel, @EmployerComercio), (@Excel, @EmployerEducacion),
        (@ServicioCliente, @EmployerServicios), (@ServicioCliente, @EmployerComercio), (@ServicioCliente, @EmployerEducacion),
        (@InglesEntrevistas, @EmployerServicios), (@InglesEntrevistas, @EmployerComercio), (@InglesEntrevistas, @EmployerEducacion),
        (@Comunicacion, @EmployerServicios), (@Comunicacion, @EmployerComercio), (@Comunicacion, @EmployerEducacion)
    ) AS Seed(MicroCursoId, EmployerProfileId)
    WHERE Seed.EmployerProfileId IS NOT NULL
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.MicroCursoValidacionesEmpleador Existing
            WHERE Existing.MicroCursoId = Seed.MicroCursoId
                AND Existing.EmployerProfileId = Seed.EmployerProfileId
        );

    COMMIT TRANSACTION;

    SELECT
        (SELECT COUNT(*) FROM dbo.MicroCursos WHERE IsActive = 1) AS TotalMicroCursosActivos,
        (SELECT COUNT(*) FROM dbo.MicroCursoValidacionesEmpleador) AS TotalValidacionesMicroCursos;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

