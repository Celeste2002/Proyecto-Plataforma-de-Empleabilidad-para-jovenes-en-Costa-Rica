using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlUserRepository(string connectionString) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                Id, Email, PasswordHash, PasswordResetToken,
                PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
            FROM dbo.Users
            WHERE Email = @Email;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                Id, Email, PasswordHash, PasswordResetToken,
                PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
            FROM dbo.Users
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                Id, Email, PasswordHash, PasswordResetToken,
                PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
            FROM dbo.Users
            ORDER BY CreatedAtUtc DESC;
            """;

        List<User> users = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task<User?> FindByPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                Id, Email, PasswordHash, PasswordResetToken,
                PasswordResetTokenExpiresAtUtc, Role, IsActive, EmailConfirmed, CreatedAtUtc
            FROM dbo.Users
            WHERE PasswordResetToken = @Token;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Token", token);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Users
                (Id, Email, PasswordHash, Role, IsActive, EmailConfirmed, CreatedAtUtc)
            VALUES
                (@Id, @Email, @PasswordHash, @Role, @IsActive, @EmailConfirmed, @CreatedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", (object?)user.PasswordHash ?? DBNull.Value);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
        command.Parameters.AddWithValue("@CreatedAtUtc", user.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordHash = @PasswordHash
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET Role = @Role
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Role", newRole);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SavePasswordResetTokenAsync(
        Guid userId,
        string token,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordResetToken = @Token,
                PasswordResetTokenExpiresAtUtc = @ExpiresAtUtc
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@ExpiresAtUtc", expiresAtUtc);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordResetToken = NULL,
                PasswordResetTokenExpiresAtUtc = NULL
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET IsActive = @IsActive
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@IsActive", isActive);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static User MapUser(SqlDataReader reader)
    {
        int resetTokenOrdinal = reader.GetOrdinal("PasswordResetToken");
        int resetExpiryOrdinal = reader.GetOrdinal("PasswordResetTokenExpiresAtUtc");
        int passwordHashOrdinal = reader.GetOrdinal("PasswordHash");

        return new User
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.IsDBNull(passwordHashOrdinal) ? null : reader.GetString(passwordHashOrdinal),
            PasswordResetToken = reader.IsDBNull(resetTokenOrdinal) ? null : reader.GetString(resetTokenOrdinal),
            PasswordResetTokenExpiresAtUtc = reader.IsDBNull(resetExpiryOrdinal)
                ? null
                : reader.GetDateTime(resetExpiryOrdinal),
            Role = reader.GetString(reader.GetOrdinal("Role")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            EmailConfirmed = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
    }
}
