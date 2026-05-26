using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlCandidateRepository(string connectionString) : ICandidateRepository
{
    public async Task<CandidateProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                Id,
                FullName,
                Age,
                Province,
                EducationLevel,
                Email,
                IsVisibleToPartnerEmployers,
                EmailConfirmationSent,
                CreatedAtUtc
            FROM dbo.CandidateProfiles
            WHERE Email = @Email;
            """;

        await using SqlConnection sqlConnection = new(connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Email", email);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        return await sqlDataReader.ReadAsync(cancellationToken)
            ? MapCandidateProfile(sqlDataReader)
            : null;
    }

    public async Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                Id,
                FullName,
                Age,
                Province,
                EducationLevel,
                Email,
                CAST(1 AS bit) AS IsVisibleToPartnerEmployers,
                EmailConfirmationSent,
                CreatedAtUtc
            FROM dbo.PartnerEmployerVisibleCandidateProfiles
            ORDER BY CreatedAtUtc DESC;
            """;

        List<CandidateProfile> candidateProfiles = [];

        await using SqlConnection sqlConnection = new(connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            candidateProfiles.Add(MapCandidateProfile(sqlDataReader));
        }

        return candidateProfiles;
    }

    public async Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        const string command = """
            INSERT INTO dbo.CandidateProfiles
            (
                Id,
                FullName,
                Age,
                Province,
                EducationLevel,
                Email,
                IsVisibleToPartnerEmployers,
                EmailConfirmationSent,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @FullName,
                @Age,
                @Province,
                @EducationLevel,
                @Email,
                @IsVisibleToPartnerEmployers,
                @EmailConfirmationSent,
                @CreatedAtUtc
            );
            """;

        await using SqlConnection sqlConnection = new(connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new(command, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Id", candidateProfile.Id);
        sqlCommand.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        sqlCommand.Parameters.AddWithValue("@Age", candidateProfile.Age);
        sqlCommand.Parameters.AddWithValue("@Province", candidateProfile.Province);
        sqlCommand.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        sqlCommand.Parameters.AddWithValue("@Email", candidateProfile.Email);
        sqlCommand.Parameters.AddWithValue("@IsVisibleToPartnerEmployers", candidateProfile.IsVisibleToPartnerEmployers);
        sqlCommand.Parameters.AddWithValue("@EmailConfirmationSent", candidateProfile.EmailConfirmationSent);
        sqlCommand.Parameters.AddWithValue("@CreatedAtUtc", candidateProfile.CreatedAtUtc);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string command = """
            UPDATE dbo.CandidateProfiles
            SET EmailConfirmationSent = 1
            WHERE Id = @Id;
            """;

        await using SqlConnection sqlConnection = new(connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new(command, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Id", candidateProfileId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static CandidateProfile MapCandidateProfile(SqlDataReader sqlDataReader)
    {
        return new CandidateProfile
        {
            Id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Id")),
            FullName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FullName")),
            Age = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Age")),
            Province = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Province")),
            EducationLevel = sqlDataReader.GetString(sqlDataReader.GetOrdinal("EducationLevel")),
            Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email")),
            IsVisibleToPartnerEmployers = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVisibleToPartnerEmployers")),
            EmailConfirmationSent = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("EmailConfirmationSent")),
            CreatedAtUtc = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("CreatedAtUtc"))
        };
    }
}
