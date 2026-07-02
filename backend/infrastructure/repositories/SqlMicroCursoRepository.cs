using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlMicroCursoRepository(string connectionString) : IMicroCursoRepository
{
    public async Task<IReadOnlyCollection<MicroCurso>> GetValidatedAsync(
        string? area,
        CancellationToken cancellationToken)
    {
        List<MicroCurso> microCursos = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.MicroCursos.GetValidated);
        command.Parameters.AddNullableWithValue("@Area", area);

        await using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                microCursos.Add(MapMicroCurso(reader));
            }
        }

        Dictionary<Guid, List<string>> habilidades =
            await LoadHabilidadesAsync(connection, microCursos.Select(c => c.Id), cancellationToken);

        return microCursos
            .Select(curso => curso with
            {
                Habilidades = habilidades.TryGetValue(curso.Id, out List<string>? values)
                    ? values
                    : []
            })
            .ToArray();
    }

    public async Task<MicroCurso?> FindValidatedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.MicroCursos.FindValidatedById);
        command.Parameters.AddWithValue("@Id", id);

        MicroCurso? microCurso = null;

        await using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                microCurso = MapMicroCurso(reader);
            }
        }

        if (microCurso is null)
        {
            return null;
        }

        Dictionary<Guid, List<string>> habilidades =
            await LoadHabilidadesAsync(connection, [microCurso.Id], cancellationToken);

        return microCurso with
        {
            Habilidades = habilidades.TryGetValue(microCurso.Id, out List<string>? values)
                ? values
                : []
        };
    }

    private static async Task<Dictionary<Guid, List<string>>> LoadHabilidadesAsync(
        SqlConnection connection,
        IEnumerable<Guid> microCursoIds,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, List<string>> habilidades = [];

        foreach (Guid microCursoId in microCursoIds.Distinct())
        {
            await using SqlCommand command =
                connection.CreateStoredProcedureCommand(StoredProcedures.MicroCursos.GetHabilidades);
            command.Parameters.AddWithValue("@MicroCursoId", microCursoId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                if (!habilidades.TryGetValue(microCursoId, out List<string>? values))
                {
                    values = [];
                    habilidades[microCursoId] = values;
                }

                values.Add(reader.GetString(reader.GetOrdinal("Nombre")));
            }
        }

        return habilidades;
    }

    private static MicroCurso MapMicroCurso(SqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
            Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
            Area = reader.GetString(reader.GetOrdinal("Area")),
            DuracionHoras = reader.GetInt32(reader.GetOrdinal("DuracionHoras")),
            EntidadProveedora = reader.GetString(reader.GetOrdinal("EntidadProveedora")),
            TipoProveedor = reader.GetString(reader.GetOrdinal("TipoProveedor")),
            OtorgaCertificacion = reader.GetBoolean(reader.GetOrdinal("OtorgaCertificacion")),
            EnlaceUrl = reader.IsDBNull(reader.GetOrdinal("EnlaceUrl")) ? null : reader.GetString(reader.GetOrdinal("EnlaceUrl")),
            CantidadValidaciones = reader.GetInt32(reader.GetOrdinal("CantidadValidaciones")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
}
