using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlPostulacionRepository(string connectionString) : IPostulacionRepository
{
    public async Task SaveAsync(Postulacion postulacion, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Postulaciones
                (Id, VacanteId, CandidateProfileId, Status, AppliedAt, UpdatedAtUtc)
            VALUES
                (@Id, @VacanteId, @CandidateProfileId, @Status, @AppliedAt, @UpdatedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", postulacion.Id);
        command.Parameters.AddWithValue("@VacanteId", postulacion.VacanteId);
        command.Parameters.AddWithValue("@CandidateProfileId", postulacion.CandidateProfileId);
        command.Parameters.AddWithValue("@Status", postulacion.Status);
        command.Parameters.AddWithValue("@AppliedAt", postulacion.AppliedAt);
        command.Parameters.AddWithValue("@UpdatedAtUtc", postulacion.UpdatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExistsByVacanteAndCandidateAsync(
        Guid vacanteId,
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT COUNT(1)
            FROM dbo.Postulaciones
            WHERE VacanteId = @VacanteId
                AND CandidateProfileId = @CandidateProfileId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        int count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count > 0;
    }

    public async Task<IReadOnlyCollection<Postulacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                p.Id,
                p.VacanteId,
                p.CandidateProfileId,
                p.Status,
                p.AppliedAt,
                p.UpdatedAtUtc,
                v.JobTitle,
                v.Province,
                ep.CompanyName
            FROM dbo.Postulaciones p
            INNER JOIN dbo.Vacantes v ON p.VacanteId = v.Id
            INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
            WHERE p.CandidateProfileId = @CandidateProfileId
            ORDER BY p.AppliedAt DESC;
            """;

        List<Postulacion> postulaciones = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            postulaciones.Add(MapPostulacion(reader));
        }

        return postulaciones;
    }

    public async Task<IReadOnlyCollection<Postulacion>> GetByVacanteIdAsync(
        Guid vacanteId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                p.Id,
                p.VacanteId,
                p.CandidateProfileId,
                p.Status,
                p.AppliedAt,
                p.UpdatedAtUtc,
                cp.FullName AS CandidateFullName
            FROM dbo.Postulaciones p
            INNER JOIN dbo.CandidateProfiles cp ON p.CandidateProfileId = cp.Id
            WHERE p.VacanteId = @VacanteId
            ORDER BY p.AppliedAt DESC;
            """;

        List<Postulacion> postulaciones = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            postulaciones.Add(new Postulacion
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
                CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
                UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc")),
                CandidateFullName = reader.GetString(reader.GetOrdinal("CandidateFullName"))
            });
        }

        return postulaciones;
    }

    public async Task<Postulacion?> FindByIdAsync(
        Guid postulacionId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                p.Id,
                p.VacanteId,
                p.CandidateProfileId,
                p.Status,
                p.AppliedAt,
                p.UpdatedAtUtc,
                v.JobTitle,
                cp.FullName            AS CandidateFullName,
                cp.Province            AS CandidateProvince,
                cp.EducationLevel      AS CandidateEducationLevel,
                cp.DateOfBirth         AS CandidateDateOfBirth,
                u.Email                AS CandidateEmail
            FROM dbo.Postulaciones p
            INNER JOIN dbo.Vacantes         v  ON p.VacanteId          = v.Id
            INNER JOIN dbo.CandidateProfiles cp ON p.CandidateProfileId = cp.Id
            INNER JOIN dbo.Users             u  ON cp.UserId            = u.Id
            WHERE p.Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", postulacionId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new Postulacion
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            CandidateFullName = reader.GetString(reader.GetOrdinal("CandidateFullName")),
            CandidateEmail = reader.GetString(reader.GetOrdinal("CandidateEmail")),
            CandidateProvince = reader.GetString(reader.GetOrdinal("CandidateProvince")),
            CandidateEducationLevel = reader.GetString(reader.GetOrdinal("CandidateEducationLevel")),
            CandidateDateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CandidateDateOfBirth")))
        };
    }

    public async Task UpdateStatusAsync(
        Guid postulacionId,
        string newStatus,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Postulaciones
            SET Status = @Status, UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Status", newStatus);
        command.Parameters.AddWithValue("@UpdatedAtUtc", updatedAtUtc);
        command.Parameters.AddWithValue("@Id", postulacionId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Postulacion MapPostulacion(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
            Province = reader.GetString(reader.GetOrdinal("Province"))
        };

    private static Postulacion MapPostulacionWithCandidate(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            CandidateFullName = reader.GetString(reader.GetOrdinal("CandidateFullName")),
            CandidateEmail = reader.GetString(reader.GetOrdinal("CandidateEmail")),
            CandidateProvince = reader.GetString(reader.GetOrdinal("CandidateProvince")),
            CandidateEducationLevel = reader.GetString(reader.GetOrdinal("CandidateEducationLevel")),
            CandidateAge = reader.GetInt32(reader.GetOrdinal("CandidateAge"))
        };
}
