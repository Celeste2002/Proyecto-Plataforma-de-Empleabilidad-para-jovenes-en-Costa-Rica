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
        command.Parameters.AddWithValue("@EmployerProfileId", notificacion.EmployerProfileId);
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

    private static Notificacion MapNotificacion(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            EmployerProfileId = reader.GetGuid(reader.GetOrdinal("EmployerProfileId")),
            PostulacionId = reader.GetGuid(reader.GetOrdinal("PostulacionId")),
            VacanteId = reader.GetGuid(reader.GetOrdinal("VacanteId")),
            Message = reader.GetString(reader.GetOrdinal("Message")),
            IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
}
