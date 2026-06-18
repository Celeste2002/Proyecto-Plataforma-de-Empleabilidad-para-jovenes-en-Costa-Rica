using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlEmployerRepository(string connectionString) : IEmployerRepository
{
    public Task<EmployerProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        => QuerySingleAsync(
            StoredProcedures.Employers.FindByEmail,
            command => command.Parameters.AddWithValue("@Email", email),
            cancellationToken);

    public Task<EmployerProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => QuerySingleAsync(
            StoredProcedures.Employers.FindByUserId,
            command => command.Parameters.AddWithValue("@UserId", userId),
            cancellationToken);

    public Task<EmployerProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        => QuerySingleAsync(
            StoredProcedures.Employers.FindById,
            command => command.Parameters.AddWithValue("@Id", id),
            cancellationToken);

    public async Task SaveAsync(EmployerProfile employerProfile, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Employers.Save);
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
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Employers.UpdateStatus);
        command.Parameters.AddWithValue("@Id", employerProfileId);
        command.Parameters.AddWithValue("@Status", status);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkActivationEmailSentAsync(Guid employerProfileId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Employers.MarkActivationEmailSent);
        command.Parameters.AddWithValue("@Id", employerProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<EmployerProfile?> QuerySingleAsync(
        string procedureName,
        Action<SqlCommand> bindParams,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command = connection.CreateStoredProcedureCommand(procedureName);
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
