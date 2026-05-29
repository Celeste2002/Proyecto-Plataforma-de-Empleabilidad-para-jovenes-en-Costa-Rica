USE Plataforma_Empleabilidad_BD;
GO

/* =============================================================
   PATCH 002: guardar contrasenas hasheadas para candidatos
   ============================================================= */

IF COL_LENGTH(N'dbo.Users', N'PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordHash NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordResetToken') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordResetToken NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenExpiresAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD PasswordResetTokenExpiresAtUtc DATETIME2(0) NULL;
END;
GO

PRINT 'Patch 002 aplicado: Users admite contrasenas protegidas con hash y recuperacion.';
GO
