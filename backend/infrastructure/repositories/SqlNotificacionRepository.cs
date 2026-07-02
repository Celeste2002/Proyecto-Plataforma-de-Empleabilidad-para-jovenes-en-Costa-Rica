using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlNotificacionRepository(string connectionString) : INotificacionRepository
{
    public async Task SaveAsync(Notificacion notificacion, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.Save);
        command.Parameters.AddWithValue("@Id", notificacion.Id);
        command.Parameters.AddNullableWithValue("@EmployerProfileId", notificacion.EmployerProfileId);
        command.Parameters.AddNullableWithValue("@CandidateProfileId", notificacion.CandidateProfileId);
        command.Parameters.AddWithValue("@PostulacionId", notificacion.PostulacionId);
        command.Parameters.AddWithValue("@VacanteId", notificacion.VacanteId);
        command.Parameters.AddWithValue("@Message", notificacion.Message);
        command.Parameters.AddWithValue("@IsRead", notificacion.IsRead);
        command.Parameters.AddWithValue("@CreatedAtUtc", notificacion.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Notificacion>> GetByEmployerProfileIdAsync(
        Guid employerProfileId,
        Guid? vacanteId,
        CancellationToken cancellationToken)
    {
        List<Notificacion> notificaciones = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.GetByEmployerProfileId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);
        command.Parameters.AddNullableWithValue("@VacanteId", vacanteId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            notificaciones.Add(MapNotificacion(reader));
        }

        return notificaciones;
    }

    public async Task<IReadOnlyCollection<Notificacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<Notificacion> notificaciones = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.GetByCandidateProfileId);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            notificaciones.Add(MapNotificacion(reader));
        }

        return notificaciones;
    }

    public async Task MarkAsReadAsync(
        Guid notificacionId,
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.MarkAsRead);
        command.Parameters.AddWithValue("@Id", notificacionId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEmployerVacanteAsReadAsync(
        Guid employerProfileId,
        Guid? vacanteId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.MarkEmployerVacanteAsRead);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);
        command.Parameters.AddNullableWithValue("@VacanteId", vacanteId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkCandidateNotificationsAsReadAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.MarkCandidateAsRead);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.GetUnreadCount);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        object? count = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(count);
    }

    public async Task<int> GetCandidateUnreadCountAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Notificaciones.GetCandidateUnreadCount);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        object? count = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(count);
    }

    private static Notificacion MapNotificacion(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            EmployerProfileId = reader.IsDBNull(reader.GetOrdinal("EmployerProfileId"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("EmployerProfileId")),
            CandidateProfileId = reader.IsDBNull(reader.GetOrdinal("CandidateProfileId"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            PostulacionId = reader.GetGuid(reader.GetOrdinal("PostulacionId")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            Message = reader.GetString(reader.GetOrdinal("Message")),
            IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
}
