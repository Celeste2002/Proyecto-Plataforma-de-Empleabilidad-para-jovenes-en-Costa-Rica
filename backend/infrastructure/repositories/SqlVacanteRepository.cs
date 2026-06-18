using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlVacanteRepository(string connectionString) : IVacanteRepository
{
    public async Task<IReadOnlyCollection<Vacante>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await QueryVacantesAsync("""
            WHERE v.IsActive = 1
            """, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Vacante>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await QueryVacantesAsync(string.Empty, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Vacante>> GetByEmployerProfileIdAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        return await QueryVacantesAsync("""
            WHERE v.EmployerProfileId = @EmployerProfileId
            """,
            cancellationToken,
            command => command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId));
    }

    public async Task<Vacante?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                v.Id,
                v.EmployerProfileId,
                v.JobTitle,
                v.Province,
                v.Sector,
                v.Modality,
                v.ExperienceLevel,
                v.Description,
                v.Requirements,
                v.SalaryRange,
                v.IsActive,
                v.PublishedAt,
                v.CreatedAtUtc,
                ep.CompanyName
            FROM dbo.Vacantes v
            INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
            WHERE v.Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapVacante(reader) : null;
    }

    public async Task SaveAsync(Vacante vacante, CancellationToken cancellationToken)
    {
        const string query = """
            INSERT INTO dbo.Vacantes
                (Id, EmployerProfileId, JobTitle, Province, Sector, Modality, ExperienceLevel,
                 Description, Requirements, SalaryRange, IsActive, PublishedAt, CreatedAtUtc)
            VALUES
                (@Id, @EmployerProfileId, @JobTitle, @Province, @Sector, @Modality, @ExperienceLevel,
                 @Description, @Requirements, @SalaryRange, @IsActive, @PublishedAt, @CreatedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", vacante.Id);
        command.Parameters.AddWithValue("@EmployerProfileId", vacante.EmployerProfileId);
        command.Parameters.AddWithValue("@JobTitle", vacante.JobTitle);
        command.Parameters.AddWithValue("@Province", vacante.Province);
        command.Parameters.AddWithValue("@Sector", vacante.Sector);
        command.Parameters.AddWithValue("@Modality", vacante.Modality);
        command.Parameters.AddWithValue("@ExperienceLevel", vacante.ExperienceLevel);
        command.Parameters.AddWithValue("@Description", (object?)vacante.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Requirements", (object?)vacante.Requirements ?? DBNull.Value);
        command.Parameters.AddWithValue("@SalaryRange", (object?)vacante.SalaryRange ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", vacante.IsActive);
        command.Parameters.AddWithValue("@PublishedAt", vacante.PublishedAt);
        command.Parameters.AddWithValue("@CreatedAtUtc", vacante.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        const string query = """
            UPDATE dbo.Vacantes
            SET IsActive = @IsActive
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@IsActive", isActive);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<Vacante>> QueryVacantesAsync(
        string whereClause,
        CancellationToken cancellationToken,
        Action<SqlCommand>? bindParameters = null)
    {
        string query = $"""
            SELECT
                v.Id,
                v.EmployerProfileId,
                v.JobTitle,
                v.Province,
                v.Sector,
                v.Modality,
                v.ExperienceLevel,
                v.Description,
                v.Requirements,
                v.SalaryRange,
                v.IsActive,
                v.PublishedAt,
                v.CreatedAtUtc,
                ep.CompanyName
            FROM dbo.Vacantes v
            INNER JOIN dbo.EmployerProfiles ep ON v.EmployerProfileId = ep.Id
            {whereClause}
            ORDER BY v.PublishedAt DESC;
            """;

        List<Vacante> vacantes = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        bindParameters?.Invoke(command);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vacantes.Add(MapVacante(reader));
        }

        return vacantes;
    }

    private static Vacante MapVacante(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            EmployerProfileId = reader.GetGuid(reader.GetOrdinal("EmployerProfileId")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            Sector = reader.GetString(reader.GetOrdinal("Sector")),
            Modality = reader.GetString(reader.GetOrdinal("Modality")),
            ExperienceLevel = reader.GetString(reader.GetOrdinal("ExperienceLevel")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString(reader.GetOrdinal("Description")),
            Requirements = reader.IsDBNull(reader.GetOrdinal("Requirements"))
                ? null
                : reader.GetString(reader.GetOrdinal("Requirements")),
            SalaryRange = reader.IsDBNull(reader.GetOrdinal("SalaryRange"))
                ? null
                : reader.GetString(reader.GetOrdinal("SalaryRange")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            PublishedAt = reader.GetDateTime(reader.GetOrdinal("PublishedAt")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName"))
        };
}
