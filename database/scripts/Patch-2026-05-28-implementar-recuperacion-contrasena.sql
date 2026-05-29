USE Plataforma_Empleabilidad_BD;
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

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_PasswordResetToken'
        AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_PasswordResetToken
        ON dbo.Users (PasswordResetToken)
        WHERE PasswordResetToken IS NOT NULL;
END;
GO
