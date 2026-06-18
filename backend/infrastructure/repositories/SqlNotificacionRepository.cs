using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlNotificacionRepository(string connectionString) : INotificacionRepository
{
    public async Task SaveAsync(Notificacion notificacion, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Notificaciones
                (Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc)
            VALUES
                (@Id, @EmployerProfileId, @PostulacionId, @VacanteId, @Message, @IsRead, @CreatedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
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
        string query = """
            SELECT Id, EmployerProfileId, PostulacionId, VacanteId, Message, IsRead, CreatedAtUtc
            FROM dbo.Notificaciones
            WHERE EmployerProfileId = @EmployerProfileId
            """ +
            (vacanteId.HasValue ? " AND VacanteId = @VacanteId" : string.Empty) +
            " ORDER BY CreatedAtUtc DESC;";

        List<Notificacion> notificaciones = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        if (vacanteId.HasValue)
            command.Parameters.AddWithValue("@VacanteId", vacanteId.Value);

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
        const string sql = """
            UPDATE dbo.Notificaciones
            SET IsRead = 1
            WHERE Id = @Id
              AND EmployerProfileId = @EmployerProfileId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", notificacionId);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT COUNT(1)
            FROM dbo.Notificaciones
            WHERE EmployerProfileId = @EmployerProfileId
              AND IsRead = 0;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);

        int count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count;
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
