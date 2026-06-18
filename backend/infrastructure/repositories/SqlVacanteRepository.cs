using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlVacanteRepository(string connectionString) : IVacanteRepository
{
    public async Task<IReadOnlyCollection<Vacante>> GetActiveAsync(CancellationToken cancellationToken)
    {
        List<Vacante> vacantes = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Vacantes.GetActive);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vacantes.Add(MapVacante(reader));
        }

        return vacantes;
    }

    public async Task<IReadOnlyCollection<Vacante>> GetByEmployerProfileIdAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        List<Vacante> vacantes = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Vacantes.GetByEmployerProfileId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vacantes.Add(MapVacante(reader));
        }

        return vacantes;
    }

    public async Task<Vacante?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Vacantes.FindById);
        command.Parameters.AddWithValue("@Id", id);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapVacante(reader) : null;
    }

    public async Task SaveAsync(Vacante vacante, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Vacantes.Save);
        command.Parameters.AddWithValue("@Id", vacante.Id);
        command.Parameters.AddWithValue("@EmployerProfileId", vacante.EmployerProfileId);
        command.Parameters.AddWithValue("@JobTitle", vacante.JobTitle);
        command.Parameters.AddWithValue("@Province", vacante.Province);
        command.Parameters.AddWithValue("@Sector", vacante.Sector);
        command.Parameters.AddWithValue("@Modality", vacante.Modality);
        command.Parameters.AddWithValue("@ExperienceLevel", vacante.ExperienceLevel);
        command.Parameters.AddNullableWithValue("@Description", vacante.Description);
        command.Parameters.AddNullableWithValue("@Requirements", vacante.Requirements);
        command.Parameters.AddNullableWithValue("@SalaryRange", vacante.SalaryRange);
        command.Parameters.AddWithValue("@IsActive", vacante.IsActive);
        command.Parameters.AddWithValue("@PublishedAt", vacante.PublishedAt);
        command.Parameters.AddWithValue("@CreatedAtUtc", vacante.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateEditableFieldsAsync(
        Guid id,
        Guid employerProfileId,
        string? description,
        string? requirements,
        string? salaryRange,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Vacantes.UpdateEditableFields);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);
        command.Parameters.AddNullableWithValue("@Description", description);
        command.Parameters.AddNullableWithValue("@Requirements", requirements);
        command.Parameters.AddNullableWithValue("@SalaryRange", salaryRange);

        object? affectedRows = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(affectedRows) > 0;
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
