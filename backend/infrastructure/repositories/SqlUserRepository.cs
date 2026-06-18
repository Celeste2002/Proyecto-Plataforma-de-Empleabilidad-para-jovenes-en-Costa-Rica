using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlUserRepository(string connectionString) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.FindByEmail);
        command.Parameters.AddWithValue("@Email", email);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.FindById);
        command.Parameters.AddWithValue("@Id", id);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<User> users = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.GetAll);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task<User?> FindByPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.FindByPasswordResetToken);
        command.Parameters.AddWithValue("@Token", token);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.Save);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddNullableWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
        command.Parameters.AddWithValue("@CreatedAtUtc", user.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.UpdatePassword);
        command.Parameters.AddWithValue("@Id", userId);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.UpdateRole);
        command.Parameters.AddWithValue("@Id", userId);
        command.Parameters.AddWithValue("@Role", newRole);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SavePasswordResetTokenAsync(
        Guid userId,
        string token,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.SavePasswordResetToken);
        command.Parameters.AddWithValue("@Id", userId);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@ExpiresAtUtc", expiresAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.ClearPasswordResetToken);
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Users.SetActive);
        command.Parameters.AddWithValue("@Id", userId);
        command.Parameters.AddWithValue("@IsActive", isActive);

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
