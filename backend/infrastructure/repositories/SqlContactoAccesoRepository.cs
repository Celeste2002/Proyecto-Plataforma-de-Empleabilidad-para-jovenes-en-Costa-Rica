using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlContactoAccesoRepository(string connectionString) : IContactoAccesoRepository
{
    public async Task SaveAsync(ContactoAcceso contactoAcceso, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.ContactoAccesos
                (Id, EmployerProfileId, PostulacionId, CandidateEmail, AccessedAtUtc)
            VALUES
                (@Id, @EmployerProfileId, @PostulacionId, @CandidateEmail, @AccessedAtUtc);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", contactoAcceso.Id);
        command.Parameters.AddWithValue("@EmployerProfileId", contactoAcceso.EmployerProfileId);
        command.Parameters.AddWithValue("@PostulacionId", contactoAcceso.PostulacionId);
        command.Parameters.AddWithValue("@CandidateEmail", contactoAcceso.CandidateEmail);
        command.Parameters.AddWithValue("@AccessedAtUtc", contactoAcceso.AccessedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
