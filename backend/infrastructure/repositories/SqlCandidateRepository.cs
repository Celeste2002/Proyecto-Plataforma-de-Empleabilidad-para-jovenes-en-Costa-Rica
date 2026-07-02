using domain.entities;
using Microsoft.Data.SqlClient;
using services.dtos;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlCandidateRepository(string connectionString) : ICandidateRepository
{
    public Task<CandidateProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        => QuerySingleProfileAsync(
            StoredProcedures.Candidates.FindByEmail,
            command => command.Parameters.AddWithValue("@Email", email),
            cancellationToken);

    public Task<CandidateProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => QuerySingleProfileAsync(
            StoredProcedures.Candidates.FindByUserId,
            command => command.Parameters.AddWithValue("@UserId", userId),
            cancellationToken);

    public async Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken)
    {
        List<CandidateProfile> candidateProfiles = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.GetVisibleToPartnerEmployers);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            candidateProfiles.Add(MapCandidateProfileFromView(reader));
        }

        return candidateProfiles;
    }

    public async Task<CandidateProfile?> FindVisibleByIdAsync(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.FindVisibleById);
        command.Parameters.AddWithValue("@Id", candidateProfileId);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfileFromView(reader) : null;
    }

    public async Task<IReadOnlyCollection<CandidateSearchResult>> SearchVisibleToPartnerEmployersAsync(
        Guid employerProfileId,
        CandidateSearchFilters filters,
        CancellationToken cancellationToken)
    {
        List<CandidateSearchResult> results = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.SearchForEmployer);
        command.Parameters.AddWithValue("@EmployerProfileId", employerProfileId);
        command.Parameters.AddNullableWithValue("@SkillKeyword", filters.SkillKeyword);
        command.Parameters.AddNullableWithValue("@Province", filters.Province);
        command.Parameters.AddNullableWithValue("@EducationLevel", filters.EducationLevel);
        command.Parameters.AddNullableWithValue("@MinExperienceYears", filters.MinExperienceYears);
        command.Parameters.AddNullableWithValue("@IsAvailableForContact", filters.IsAvailableForContact);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapCandidateSearchResult(reader));
        }

        return results;
    }

    public async Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.Save);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        command.Parameters.AddWithValue("@IsVisibleToPartnerEmployers", candidateProfile.IsVisibleToPartnerEmployers);
        command.Parameters.AddWithValue("@IsAvailableForContact", candidateProfile.IsAvailableForContact);
        command.Parameters.AddNullableWithValue("@PhotoUrl", candidateProfile.PhotoUrl);
        command.Parameters.AddWithValue("@CreatedAtUtc", candidateProfile.CreatedAtUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.Update);
        command.Parameters.AddWithValue("@Id", candidateProfile.Id);
        command.Parameters.AddWithValue("@UserId", candidateProfile.UserId);
        command.Parameters.AddWithValue("@FullName", candidateProfile.FullName);
        command.Parameters.AddWithValue("@DateOfBirth", candidateProfile.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Province", candidateProfile.Province);
        command.Parameters.AddWithValue("@EducationLevel", candidateProfile.EducationLevel);
        command.Parameters.AddNullableWithValue("@PhotoUrl", candidateProfile.PhotoUrl);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAvailabilityAsync(Guid profileId, bool isAvailableForContact, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.UpdateAvailability);
        command.Parameters.AddWithValue("@Id", profileId);
        command.Parameters.AddWithValue("@IsAvailableForContact", isAvailableForContact);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.MarkEmailConfirmationSent);
        command.Parameters.AddWithValue("@Id", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Experiencias laborales --

    public async Task<IReadOnlyCollection<ExperienciaLaboral>> GetExperienciasAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<ExperienciaLaboral> result = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.GetExperiencias);
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
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.SaveExperiencia);
        command.Parameters.AddWithValue("@Id", experiencia.Id);
        command.Parameters.AddWithValue("@CandidateProfileId", experiencia.CandidateProfileId);
        command.Parameters.AddWithValue("@Empresa", experiencia.Empresa);
        command.Parameters.AddWithValue("@Cargo", experiencia.Cargo);
        command.Parameters.AddWithValue("@FechaInicio", experiencia.FechaInicio.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddNullableWithValue(
            "@FechaFin",
            experiencia.FechaFin.HasValue
                ? experiencia.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                : null);
        command.Parameters.AddWithValue("@EsTrabajoActual", experiencia.EsTrabajoActual);
        command.Parameters.AddNullableWithValue("@Descripcion", experiencia.Descripcion);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteExperienciaAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.DeleteExperiencia);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Habilidades --

    public async Task<IReadOnlyCollection<Habilidad>> GetHabilidadesAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<Habilidad> result = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.GetHabilidades);
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

    public async Task<IReadOnlyCollection<string>> GetHabilidadesBlandasSugeridasAsync(
        CancellationToken cancellationToken)
    {
        List<string> result = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.GetHabilidadesBlandasSugeridas);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(reader.GetOrdinal("Nombre")));
        }

        return result;
    }

    public async Task SaveHabilidadAsync(Habilidad habilidad, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.SaveHabilidad);
        command.Parameters.AddWithValue("@Id", habilidad.Id);
        command.Parameters.AddWithValue("@CandidateProfileId", habilidad.CandidateProfileId);
        command.Parameters.AddWithValue("@Nombre", habilidad.Nombre);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteHabilidadAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.DeleteHabilidad);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Cursos completados --

    public async Task<IReadOnlyCollection<CursoCompletado>> GetCursosAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        List<CursoCompletado> result = [];

        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.GetCursos);
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
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.SaveCurso);
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
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.Candidates.DeleteCurso);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CandidateProfileId", candidateProfileId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // -- Mappers privados --

    private async Task<CandidateProfile?> QuerySingleProfileAsync(
        string procedureName,
        Action<SqlCommand> bindParams,
        CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command = connection.CreateStoredProcedureCommand(procedureName);
        bindParams(command);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCandidateProfile(reader) : null;
    }

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
            IsAvailableForContact = reader.GetBoolean(reader.GetOrdinal("IsAvailableForContact")),
            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl")) ? null : reader.GetString(reader.GetOrdinal("PhotoUrl")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            EmailConfirmationSent = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed"))
        };
    }

    private static CandidateSearchResult MapCandidateSearchResult(SqlDataReader reader)
    {
        return new CandidateSearchResult
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DateOfBirth"))),
            Province = reader.GetString(reader.GetOrdinal("Province")),
            EducationLevel = reader.GetString(reader.GetOrdinal("EducationLevel")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            IsAvailableForContact = reader.GetBoolean(reader.GetOrdinal("IsAvailableForContact")),
            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl")) ? null : reader.GetString(reader.GetOrdinal("PhotoUrl")),
            ExperienceYears = reader.GetDecimal(reader.GetOrdinal("ExperienceYears")),
            HasAppliedToYourVacantes = reader.GetBoolean(reader.GetOrdinal("HasAppliedToYourVacantes"))
        };
    }
}
