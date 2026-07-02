using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlPostulacionRepository(string connectionString) : IPostulacionRepository
{
    public async Task SaveAsync(Postulacion postulacion, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.Save);
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
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.ExistsByVacanteAndCandidate);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        object? count = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(count) > 0;
    }

    public async Task<IReadOnlyCollection<Postulacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<Postulacion> postulaciones = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.GetByCandidateProfileId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            postulaciones.Add(MapPostulacion(reader));
        }

        return postulaciones;
    }

    public async Task<IReadOnlyCollection<Postulacion>> GetByVacanteForEmployerAsync(
        Guid vacanteId,
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        List<Postulacion> postulaciones = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.GetByVacanteForEmployer);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            postulaciones.Add(MapEmployerPostulacion(reader));
        }

        return postulaciones;
    }

    public async Task<IReadOnlyCollection<Postulacion>> GetByVacanteForClosureAsync(
        Guid vacanteId,
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        List<Postulacion> postulaciones = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.GetByVacanteForClosure);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            postulaciones.Add(MapEmployerPostulacion(reader));
        }

        return postulaciones;
    }

    public async Task<Postulacion?> FindByIdForEmployerAsync(
        Guid id,
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.FindByIdForEmployer);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapEmployerPostulacion(reader) : null;
    }

    public async Task<bool> UpdateStatusForEmployerAsync(
        Guid id,
        Guid employerProfileId,
        string status,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.UpdateStatusForEmployer);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@UpdatedAtUtc", updatedAtUtc);

        object? affectedRows = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(affectedRows) > 0;
    }

    public async Task<bool> DeleteForCandidateAsync(
        Guid id,
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.DeleteForCandidate);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        object? affectedRows = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(affectedRows) > 0;
    }

    public async Task<IReadOnlyCollection<Guid>> GetAppliedVacanteIdsForEmployerAsync(
        Guid candidateProfileId,
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        List<Guid> vacanteIds = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Postulaciones.GetAppliedVacanteIdsForEmployer);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vacanteIds.Add(reader.GetGuid(reader.GetOrdinal("VacanteId")));
        }

        return vacanteIds;
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

    private static Postulacion MapEmployerPostulacion(SqlDataReader reader) =>
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
            CandidateDateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("CandidateDateOfBirth")))
        };
}
