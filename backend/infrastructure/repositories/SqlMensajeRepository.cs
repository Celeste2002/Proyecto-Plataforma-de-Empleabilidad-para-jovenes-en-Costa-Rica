using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlMensajeRepository(string connectionString) : IMensajeRepository
{
    public async Task SaveAsync(Mensaje mensaje, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Mensajes
                (Id, PostulacionId, SenderEmployerProfileId, RecipientCandidateProfileId, Body, SentAtUtc, IsReadByCandidate)
            VALUES
                (@Id, @PostulacionId, @SenderEmployerProfileId, @RecipientCandidateProfileId, @Body, @SentAtUtc, 0);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", mensaje.Id);
        command.Parameters.AddWithValue("@PostulacionId", mensaje.PostulacionId);
        command.Parameters.AddWithValue("@SenderEmployerProfileId", mensaje.SenderEmployerProfileId);
        command.Parameters.AddWithValue("@RecipientCandidateProfileId", mensaje.RecipientCandidateProfileId);
        command.Parameters.AddWithValue("@Body", mensaje.Body);
        command.Parameters.AddWithValue("@SentAtUtc", mensaje.SentAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Mensaje>> GetByPostulacionIdAsync(
        Guid postulacionId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                m.Id,
                m.PostulacionId,
                m.SenderEmployerProfileId,
                m.RecipientCandidateProfileId,
                m.Body,
                m.SentAtUtc,
                m.IsReadByCandidate,
                ep.CompanyName AS SenderCompanyName,
                v.JobTitle
            FROM dbo.Mensajes m
            INNER JOIN dbo.EmployerProfiles ep ON m.SenderEmployerProfileId = ep.Id
            INNER JOIN dbo.Postulaciones p     ON m.PostulacionId = p.Id
            INNER JOIN dbo.Vacantes v          ON p.VacanteId = v.Id
            WHERE m.PostulacionId = @PostulacionId
            ORDER BY m.SentAtUtc ASC;
            """;

        List<Mensaje> mensajes = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@PostulacionId", postulacionId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            mensajes.Add(MapMensaje(reader));
        }

        return mensajes;
    }

    public async Task<IReadOnlyCollection<Mensaje>> GetByRecipientCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                m.Id,
                m.PostulacionId,
                m.SenderEmployerProfileId,
                m.RecipientCandidateProfileId,
                m.Body,
                m.SentAtUtc,
                m.IsReadByCandidate,
                ep.CompanyName AS SenderCompanyName,
                v.JobTitle
            FROM dbo.Mensajes m
            INNER JOIN dbo.EmployerProfiles ep ON m.SenderEmployerProfileId = ep.Id
            INNER JOIN dbo.Postulaciones p     ON m.PostulacionId = p.Id
            INNER JOIN dbo.Vacantes v          ON p.VacanteId = v.Id
            WHERE m.RecipientCandidateProfileId = @RecipientCandidateProfileId
            ORDER BY m.SentAtUtc DESC;
            """;

        List<Mensaje> mensajes = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@RecipientCandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            mensajes.Add(MapMensaje(reader));
        }

        return mensajes;
    }

    private static Mensaje MapMensaje(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            PostulacionId = reader.GetGuid(reader.GetOrdinal("PostulacionId")),
            SenderEmployerProfileId = reader.GetGuid(reader.GetOrdinal("SenderEmployerProfileId")),
            RecipientCandidateProfileId = reader.GetGuid(reader.GetOrdinal("RecipientCandidateProfileId")),
            Body = reader.GetString(reader.GetOrdinal("Body")),
            SentAtUtc = reader.GetDateTime(reader.GetOrdinal("SentAtUtc")),
            IsReadByCandidate = reader.GetBoolean(reader.GetOrdinal("IsReadByCandidate")),
            SenderCompanyName = reader.GetString(reader.GetOrdinal("SenderCompanyName")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle"))
        };
}
