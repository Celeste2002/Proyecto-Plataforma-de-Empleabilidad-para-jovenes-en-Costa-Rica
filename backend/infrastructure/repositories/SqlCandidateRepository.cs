using domain.entities;
using Microsoft.Data.SqlClient;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlCandidateRepository(string connectionString) : ICandidateRepository
{
    public async Task<CandidateProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                cp.Id,
                cp.UserId,
                cp.FullName,
                cp.DateOfBirth,
                cp.Province,
                cp.EducationLevel,
                cp.IsVisibleToPartnerEmployers,
                cp.IsAvailableForContact,
                cp.PhotoUrl,
                cp.CreatedAtUtc,
                u.Email,
                u.EmailConfirmed
            FROM dbo.CandidateProfiles cp
            INNER JOIN dbo.Users u ON cp.UserId = u.Id
            WHERE u.Email = @Email;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfile(reader) : null;
    }

    public async Task<CandidateProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT TOP (1)
                cp.Id,
                cp.UserId,
                cp.FullName,
                cp.DateOfBirth,
                cp.Province,
                cp.EducationLevel,
                cp.IsVisibleToPartnerEmployers,
                cp.IsAvailableForContact,
                cp.PhotoUrl,
                cp.CreatedAtUtc,
                u.Email,
                u.EmailConfirmed
            FROM dbo.CandidateProfiles cp
            INNER JOIN dbo.Users u ON cp.UserId = u.Id
            WHERE cp.UserId = @UserId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfile(reader) : null;
    }

    public async Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT
                Id,
                FullName,
                DateOfBirth,
                Age,
                Province,
                EducationLevel,
                Email,
                EmailConfirmed,
                CreatedAtUtc
            FROM dbo.PartnerEmployerVisibleCandidateProfiles
            ORDER BY CreatedAtUtc DESC;
            """;

        List<CandidateProfile> candidateProfiles = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            candidateProfiles.Add(MapCandidateProfileFromView(reader));
        }

        return candidateProfiles;
    }

    public async Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.CandidateProfiles
            (
                Id,
                UserId,
                FullName,
                DateOfBirth,
                Age,
                Province,
                EducationLevel,
                IsVisibleToPartnerEmployers,
                IsAvailableForContact,
                PhotoUrl,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @UserId,
                @FullName,
                @DateOfBirth,
                @Age,
                @Province,
                @EducationLevel,
                @IsVisibleToPartnerEmployers,
                @IsAvailableForContact,
                @PhotoUrl,
                @CreatedAtUtc
            );
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Age", CalculateAge(candidateProfile.DateOfBirth));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        command.Parameters.AddWithValue("@IsVisibleToPartnerEmployers", candidateProfile.IsVisibleToPartnerEmployers);
        command.Parameters.AddWithValue("@IsAvailableForContact", candidateProfile.IsAvailableForContact);
        command.Parameters.AddWithValue("@PhotoUrl", (object?)candidateProfile.PhotoUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAtUtc", candidateProfile.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.CandidateProfiles
            SET FullName = @FullName,
                DateOfBirth = @DateOfBirth,
                Age = @Age,
                Province = @Province,
                EducationLevel = @EducationLevel,
                PhotoUrl = @PhotoUrl
            WHERE Id = @Id
                AND UserId = @UserId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Age", CalculateAge(candidateProfile.DateOfBirth));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        command.Parameters.AddWithValue("@PhotoUrl", (object?)candidateProfile.PhotoUrl ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAvailabilityAsync(Guid profileId, bool isAvailableForContact, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.CandidateProfiles
            SET IsAvailableForContact = @IsAvailableForContact
            WHERE Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", profileId);
        command.Parameters.AddWithValue("@IsAvailableForContact", isAvailableForContact);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE u
            SET u.EmailConfirmed = 1
            FROM dbo.Users u
            INNER JOIN dbo.CandidateProfiles cp ON u.Id = cp.UserId
            WHERE cp.Id = @Id;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Experiencias laborales --

    public async Task<IReadOnlyCollection<ExperienciaLaboral>> GetExperienciasAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion
            FROM dbo.ExperienciasLaborales
            WHERE CandidateProfileId = @CandidateProfileId
            ORDER BY FechaInicio DESC;
            """;

        List<ExperienciaLaboral> result = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapExperienciaLaboral(reader));
        }

        return result;
    }

    public async Task SaveExperienciaAsync(ExperienciaLaboral experiencia, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.ExperienciasLaborales
            (Id, CandidateProfileId, Empresa, Cargo, FechaInicio, FechaFin, EsTrabajoActual, Descripcion)
            VALUES
            (@Id, @CandidateProfileId, @Empresa, @Cargo, @FechaInicio, @FechaFin, @EsTrabajoActual, @Descripcion);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", experiencia.Id);
        command.Parameters.AddWithValue("@CandidateProfileId", experiencia.CandidateProfileId);
        command.Parameters.AddWithValue("@Empresa", experiencia.Empresa);
        command.Parameters.AddWithValue("@Cargo", experiencia.Cargo);
        command.Parameters.AddWithValue("@FechaInicio", experiencia.FechaInicio.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@FechaFin", experiencia.FechaFin.HasValue
            ? (object)experiencia.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
            : DBNull.Value);
        command.Parameters.AddWithValue("@EsTrabajoActual", experiencia.EsTrabajoActual);
        command.Parameters.AddWithValue("@Descripcion", (object?)experiencia.Descripcion ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteExperienciaAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.ExperienciasLaborales
            WHERE Id = @Id AND CandidateProfileId = @CandidateProfileId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Habilidades --

    public async Task<IReadOnlyCollection<Habilidad>> GetHabilidadesAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT Id, CandidateProfileId, Nombre
            FROM dbo.Habilidades
            WHERE CandidateProfileId = @CandidateProfileId
            ORDER BY Nombre;
            """;

        List<Habilidad> result = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Habilidad
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre"))
            });
        }

        return result;
    }

    public async Task SaveHabilidadAsync(Habilidad habilidad, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Habilidades (Id, CandidateProfileId, Nombre)
            VALUES (@Id, @CandidateProfileId, @Nombre);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", habilidad.Id);
        command.Parameters.AddWithValue("@CandidateProfileId", habilidad.CandidateProfileId);
        command.Parameters.AddWithValue("@Nombre", habilidad.Nombre);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteHabilidadAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.Habilidades
            WHERE Id = @Id AND CandidateProfileId = @CandidateProfileId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Cursos completados --

    public async Task<IReadOnlyCollection<CursoCompletado>> GetCursosAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma
            FROM dbo.CursosCompletados
            WHERE CandidateProfileId = @CandidateProfileId
            ORDER BY FechaCompletado DESC;
            """;

        List<CursoCompletado> result = [];

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CursoCompletado
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
                NombreCurso = reader.GetString(reader.GetOrdinal("NombreCurso")),
                Institucion = reader.GetString(reader.GetOrdinal("Institucion")),
                FechaCompletado = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("FechaCompletado"))),
                EsDePlataforma = reader.GetBoolean(reader.GetOrdinal("EsDePlataforma"))
            });
        }

        return result;
    }

    public async Task SaveCursoAsync(CursoCompletado curso, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.CursosCompletados
            (Id, CandidateProfileId, NombreCurso, Institucion, FechaCompletado, EsDePlataforma)
            VALUES
            (@Id, @CandidateProfileId, @NombreCurso, @Institucion, @FechaCompletado, @EsDePlataforma);
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", curso.Id);
        command.Parameters.AddWithValue("@CandidateProfileId", curso.CandidateProfileId);
        command.Parameters.AddWithValue("@NombreCurso", curso.NombreCurso);
        command.Parameters.AddWithValue("@Institucion", curso.Institucion);
        command.Parameters.AddWithValue("@FechaCompletado", curso.FechaCompletado.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@EsDePlataforma", curso.EsDePlataforma);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteCursoAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.CursosCompletados
            WHERE Id = @Id AND CandidateProfileId = @CandidateProfileId;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Mappers privados --

    private static CandidateProfile MapCandidateProfile(SqlDataReader reader)
    {
        return new CandidateProfile
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DateOfBirth"))),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            EducationLevel = reader.GetString(reader.GetOrdinal("EducationLevel")),
            IsVisibleToPartnerEmployers = reader.GetBoolean(reader.GetOrdinal("IsVisibleToPartnerEmployers")),
            IsAvailableForContact = reader.GetBoolean(reader.GetOrdinal("IsAvailableForContact")),
            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl")) ? null : reader.GetString(reader.GetOrdinal("PhotoUrl")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            EmailConfirmationSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }

    private static ExperienciaLaboral MapExperienciaLaboral(SqlDataReader reader)
    {
        int fechaFinOrdinal = reader.GetOrdinal("FechaFin");

        return new ExperienciaLaboral
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            CandidateProfileId = reader.GetGuid(reader.GetOrdinal("CandidateProfileId")),
            Empresa = reader.GetString(reader.GetOrdinal("Empresa")),
            Cargo = reader.GetString(reader.GetOrdinal("Cargo")),
            FechaInicio = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("FechaInicio"))),
            FechaFin = reader.IsDBNull(fechaFinOrdinal)
                ? null
                : DateOnly.FromDateTime(reader.GetDateTime(fechaFinOrdinal)),
            EsTrabajoActual = reader.GetBoolean(reader.GetOrdinal("EsTrabajoActual")),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion"))
                ? null
                : reader.GetString(reader.GetOrdinal("Descripcion"))
        };
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static CandidateProfile MapCandidateProfileFromView(SqlDataReader reader)
    {
        return new CandidateProfile
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = Guid.Empty, // No disponible en la vista; solo se usa para lecturas de empleadores
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DateOfBirth"))),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            EducationLevel = reader.GetString(reader.GetOrdinal("EducationLevel")),
            IsVisibleToPartnerEmployers = true,
            IsAvailableForContact = true,
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            EmailConfirmationSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }
}
