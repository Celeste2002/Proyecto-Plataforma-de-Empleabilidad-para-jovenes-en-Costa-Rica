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
                cp.Id,
                cp.UserId,
                cp.FullName,
                cp.DateOfBirth,
                cp.Province,
                cp.EducationLevel,
                cp.IsVisibleToPartnerEmployers,
                cp.CreatedAtUtc,
                u.Email,
                u.EmailConfirmed
            FROM dbo.CandidateProfiles cp
            INNER JOIN dbo.Users u ON cp.UserId = u.Id
            WHERE u.Email = @Email;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfile(reader) : null;
    }

    public async Task<CandidateProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                cp.Id,
                cp.UserId,
                cp.FullName,
                cp.DateOfBirth,
                cp.Province,
                cp.EducationLevel,
                cp.IsVisibleToPartnerEmployers,
                cp.CreatedAtUtc,
                u.Email,
                u.EmailConfirmed
            FROM dbo.CandidateProfiles cp
            INNER JOIN dbo.Users u ON cp.UserId = u.Id
            WHERE cp.UserId = @UserId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfile(reader) : null;
    }

    public async Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                Id,
                FullName,
                DateOfBirth,
                Age,
                Province,
                EducationLevel,
                Email,
                EmailConfirmed,
                CreatedAtUtc
            FROM dbo.PartnerEmployerVisibleCandidateProfiles
            ORDER BY CreatedAtUtc DESC;
            """;

        List<CandidateProfile> candidateProfiles = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            candidateProfiles.Add(MapCandidateProfileFromView(reader));
        }

        return candidateProfiles;
    }

    public async Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.CandidateProfiles
            (
                Id,
                UserId,
                FullName,
                DateOfBirth,
                Age,
                Province,
                EducationLevel,
                IsVisibleToPartnerEmployers,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @UserId,
                @FullName,
                @DateOfBirth,
                @Age,
                @Province,
                @EducationLevel,
                @IsVisibleToPartnerEmployers,
                @CreatedAtUtc
            );
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Age", CalculateAge(candidateProfile.DateOfBirth));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        command.Parameters.AddWithValue("@IsVisibleToPartnerEmployers", candidateProfile.IsVisibleToPartnerEmployers);
        command.Parameters.AddWithValue("@CreatedAtUtc", candidateProfile.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.CandidateProfiles
            SET FullName = @FullName,
                DateOfBirth = @DateOfBirth,
                Age = @Age,
                Province = @Province,
                EducationLevel = @EducationLevel
            WHERE Id = @Id
                AND UserId = @UserId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Age", CalculateAge(candidateProfile.DateOfBirth));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE u
            SET u.EmailConfirmed = 1
            FROM dbo.Users u
            INNER JOIN dbo.CandidateProfiles cp ON u.Id = cp.UserId
            WHERE cp.Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static CandidateProfile MapCandidateProfile(SqlDataReader reader)
    {
        return new CandidateProfile
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DateOfBirth"))),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            EducationLevel = reader.GetString(reader.GetOrdinal("EducationLevel")),
            IsVisibleToPartnerEmployers = reader.GetBoolean(reader.GetOrdinal("IsVisibleToPartnerEmployers")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            EmailConfirmationSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static CandidateProfile MapCandidateProfileFromView(SqlDataReader reader)
    {
        return new CandidateProfile
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = Guid.Empty, // No disponible en la vista; solo se usa para lecturas de empleadores
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DateOfBirth"))),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            EducationLevel = reader.GetString(reader.GetOrdinal("EducationLevel")),
            IsVisibleToPartnerEmployers = true,
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            EmailConfirmationSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }
}
