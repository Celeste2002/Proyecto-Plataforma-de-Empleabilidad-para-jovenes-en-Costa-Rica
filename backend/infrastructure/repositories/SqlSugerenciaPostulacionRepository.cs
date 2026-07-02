using domain.entities;
using Microsoft.Data.SqlClient;
using services.exceptions;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlSugerenciaPostulacionRepository(string connectionString) : ISugerenciaPostulacionRepository
{
    public async Task SaveAsync(SugerenciaPostulacion sugerencia, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.SugerenciasPostulacion.Save);
        command.Parameters.AddWithValue("@Id", sugerencia.Id);
        command.Parameters.AddWithValue("@VacanteId", sugerencia.VacanteId);
        command.Parameters.AddWithValue("@CandidateProfileId", sugerencia.CandidateProfileId);
        command.Parameters.AddNullableWithValue("@Message", sugerencia.Message);
        command.Parameters.AddWithValue("@CreatedAtUtc", sugerencia.CreatedAtUtc);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException sqlException) when (sqlException.Number is 2601 or 2627)
        {
            // Violacion del UNIQUE (VacanteId, CandidateProfileId): dos solicitudes casi
            // simultaneas para el mismo par pasaron el chequeo de duplicados en el servicio.
            throw new RequestValidationException(["Ya se envio una sugerencia de esta vacante a este candidato."]);
        }
    }

    public async Task<bool> ExistsByVacanteAndCandidateAsync(
        Guid vacanteId,
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.SugerenciasPostulacion.ExistsByVacanteAndCandidate);
        command.Parameters.AddWithValue("@VacanteId", vacanteId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        object? count = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(count) > 0;
    }

    public async Task<IReadOnlyCollection<SugerenciaPostulacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<SugerenciaPostulacion> sugerencias = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.SugerenciasPostulacion.GetByCandidateProfileId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sugerencias.Add(MapSugerenciaPostulacion(reader));
        }

        return sugerencias;
    }

    private static SugerenciaPostulacion MapSugerenciaPostulacion(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            Message = reader.IsDBNull(reader.GetOrdinal("Message")) ? null : reader.GetString(reader.GetOrdinal("Message")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            VacanteIsActive = reader.GetBoolean(reader.GetOrdinal("VacanteIsActive")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
            EmployerContactName = reader.GetString(reader.GetOrdinal("EmployerContactName")),
            AlreadyApplied = reader.GetBoolean(reader.GetOrdinal("AlreadyApplied"))
        };
}
