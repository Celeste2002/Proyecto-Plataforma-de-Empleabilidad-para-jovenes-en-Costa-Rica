using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlMicroCursoRepository(string connectionString) : IMicroCursoRepository
{
    public Task<IReadOnlyCollection<MicroCurso>> GetValidatedAsync(
        string? area,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                mc.Id,
                mc.Titulo,
                mc.Descripcion,
                mc.Area,
                mc.DuracionHoras,
                mc.EntidadProveedora,
                mc.TipoProveedor,
                mc.OtorgaCertificacion,
                COUNT(DISTINCT ep.Id) AS CantidadValidaciones,
                mc.IsActive,
                mc.CreatedAtUtc
            FROM dbo.MicroCursos mc
            LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
                ON mc.Id = mcv.MicroCursoId
            LEFT JOIN dbo.EmployerProfiles ep
                ON mcv.EmployerProfileId = ep.Id
                AND ep.Status = N'Active'
            WHERE mc.IsActive = 1
                AND (@Area IS NULL OR mc.Area = @Area)
            GROUP BY
                mc.Id,
                mc.Titulo,
                mc.Descripcion,
                mc.Area,
                mc.DuracionHoras,
                mc.EntidadProveedora,
                mc.TipoProveedor,
                mc.OtorgaCertificacion,
                mc.IsActive,
                mc.CreatedAtUtc
            HAVING COUNT(DISTINCT ep.Id) >= 3
            ORDER BY mc.Area, mc.Titulo;
            """;

        return QueryMicroCursosAsync(
            query,
            command => command.Parameters.AddWithValue("@Area", (object?)area ?? DBNull.Value),
            cancellationToken);
    }

    public async Task<MicroCurso?> FindValidatedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                mc.Id,
                mc.Titulo,
                mc.Descripcion,
                mc.Area,
                mc.DuracionHoras,
                mc.EntidadProveedora,
                mc.TipoProveedor,
                mc.OtorgaCertificacion,
                COUNT(DISTINCT ep.Id) AS CantidadValidaciones,
                mc.IsActive,
                mc.CreatedAtUtc
            FROM dbo.MicroCursos mc
            LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
                ON mc.Id = mcv.MicroCursoId
            LEFT JOIN dbo.EmployerProfiles ep
                ON mcv.EmployerProfileId = ep.Id
                AND ep.Status = N'Active'
            WHERE mc.Id = @Id
                AND mc.IsActive = 1
            GROUP BY
                mc.Id,
                mc.Titulo,
                mc.Descripcion,
                mc.Area,
                mc.DuracionHoras,
                mc.EntidadProveedora,
                mc.TipoProveedor,
                mc.OtorgaCertificacion,
                mc.IsActive,
                mc.CreatedAtUtc
            HAVING COUNT(DISTINCT ep.Id) >= 3;
            """;

        IReadOnlyCollection<MicroCurso> microCursos = await QueryMicroCursosAsync(
            query,
            command => command.Parameters.AddWithValue("@Id", id),
            cancellationToken);

        return microCursos.SingleOrDefault();
    }

    private async Task<IReadOnlyCollection<MicroCurso>> QueryMicroCursosAsync(
        string query,
        Action<SqlCommand> bindParameters,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        bindParameters(command);

        List<MicroCurso> microCursos = [];

        await using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                microCursos.Add(MapMicroCurso(reader));
            }
        }

        Dictionary<Guid, List<string>> habilidades =
            await LoadHabilidadesAsync(connection, microCursos.Select(c => c.Id).ToArray(), cancellationToken);

        return microCursos
            .Select(curso => curso with
            {
                Habilidades = habilidades.TryGetValue(curso.Id, out List<string>? values)
                    ? values
                    : []
            })
            .ToArray();
    }

    private static async Task<Dictionary<Guid, List<string>>> LoadHabilidadesAsync(
        SqlConnection connection,
        IReadOnlyCollection<Guid> microCursoIds,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, List<string>> habilidades = [];

        if (microCursoIds.Count == 0)
        {
            return habilidades;
        }

        string[] parameterNames = microCursoIds
            .Select((_, index) => $"@Id{index}")
            .ToArray();

        string query = $"""
            SELECT MicroCursoId, Nombre
            FROM dbo.MicroCursoHabilidades
            WHERE MicroCursoId IN ({string.Join(", ", parameterNames)})
            ORDER BY Nombre;
            """;

        await using SqlCommand command = new(query, connection);

        int i = 0;
        foreach (Guid id in microCursoIds)
        {
            command.Parameters.AddWithValue(parameterNames[i], id);
            i++;
        }

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            Guid microCursoId = reader.GetGuid(reader.GetOrdinal("MicroCursoId"));
            string nombre = reader.GetString(reader.GetOrdinal("Nombre"));

            if (!habilidades.TryGetValue(microCursoId, out List<string>? values))
            {
                values = [];
                habilidades[microCursoId] = values;
            }

            values.Add(nombre);
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
            CantidadValidaciones = reader.GetInt32(reader.GetOrdinal("CantidadValidaciones")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };
}

