using domain.constants;
using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlEmployerRepository(string connectionString) : IEmployerRepository
{
    public async Task<EmployerProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
                ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
                u.Email, u.EmailConfirmed
            FROM dbo.EmployerProfiles ep
            INNER JOIN dbo.Users u ON ep.UserId = u.Id
            WHERE u.Email = @Email;
            """;

        return await QuerySingleAsync(query, cmd => cmd.Parameters.AddWithValue("@Email", email), cancellationToken);
    }

    public async Task<EmployerProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
                ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
                u.Email, u.EmailConfirmed
            FROM dbo.EmployerProfiles ep
            INNER JOIN dbo.Users u ON ep.UserId = u.Id
            WHERE ep.UserId = @UserId;
            """;

        return await QuerySingleAsync(query, cmd => cmd.Parameters.AddWithValue("@UserId", userId), cancellationToken);
    }

    public async Task<EmployerProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                ep.Id, ep.UserId, ep.CompanyName, ep.LegalId, ep.Sector,
                ep.ContactName, ep.ContactPhone, ep.Location, ep.Status, ep.CreatedAtUtc,
                u.Email, u.EmailConfirmed
            FROM dbo.EmployerProfiles ep
            INNER JOIN dbo.Users u ON ep.UserId = u.Id
            WHERE ep.Id = @Id;
            """;

        return await QuerySingleAsync(query, cmd => cmd.Parameters.AddWithValue("@Id", id), cancellationToken);
    }

    public async Task SaveAsync(EmployerProfile employerProfile, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.EmployerProfiles
            (Id, UserId, CompanyName, LegalId, Sector, ContactName, ContactPhone, Location, Status, CreatedAtUtc)
            VALUES
            (@Id, @UserId, @CompanyName, @LegalId, @Sector, @ContactName, @ContactPhone, @Location, @Status, @CreatedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", employerProfile.Id);
        command.Parameters.AddWithValue("@UserId", employerProfile.UserId);
        command.Parameters.AddWithValue("@CompanyName", employerProfile.CompanyName);
        command.Parameters.AddWithValue("@LegalId", employerProfile.LegalId);
        command.Parameters.AddWithValue("@Sector", employerProfile.Sector);
        command.Parameters.AddWithValue("@ContactName", employerProfile.ContactName);
        command.Parameters.AddWithValue("@ContactPhone", employerProfile.ContactPhone);
        command.Parameters.AddWithValue("@Location", employerProfile.Location);
        command.Parameters.AddWithValue("@Status", employerProfile.Status);
        command.Parameters.AddWithValue("@CreatedAtUtc", employerProfile.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid employerProfileId, string status, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.EmployerProfiles
            SET Status = @Status
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Id", employerProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkActivationEmailSentAsync(Guid employerProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE u
            SET u.EmailConfirmed = 1
            FROM dbo.Users u
            INNER JOIN dbo.EmployerProfiles ep ON u.Id = ep.UserId
            WHERE ep.Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", employerProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<EmployerProfile?> QuerySingleAsync(
        string query,
        Action<SqlCommand> bindParams,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        bindParams(command);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapEmployerProfile(reader) : null;
    }

    private static EmployerProfile MapEmployerProfile(SqlDataReader reader)
    {
        return new EmployerProfile
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
            LegalId = reader.GetString(reader.GetOrdinal("LegalId")),
            Sector = reader.GetString(reader.GetOrdinal("Sector")),
            ContactName = reader.GetString(reader.GetOrdinal("ContactName")),
            ContactPhone = reader.GetString(reader.GetOrdinal("ContactPhone")),
            Location = reader.GetString(reader.GetOrdinal("Location")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            ActivationEmailSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }
}
