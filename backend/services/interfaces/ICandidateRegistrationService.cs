using services.dtos;

namespace services.interfaces;

public interface ICandidateRegistrationService
{
    Task<CandidateRegistrationResponse> RegisterAsync(
        RegisterCandidateRequest registerCandidateRequest,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CandidateProfileResponse>> GetProfilesVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken);

    Task<CandidateProfileResponse> GetProfileByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<CandidatoPerfilCompletoResponse> GetFullProfileAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<CandidateProfileResponse> UpdateProfileAsync(
        Guid userId,
        UpdateCandidateProfileRequest updateCandidateProfileRequest,
        CancellationToken cancellationToken);

    Task UpdateAvailabilityAsync(
        Guid userId,
        bool isAvailableForContact,
        CancellationToken cancellationToken);

    Task UpdatePasswordAsync(
        Guid userId,
        UpdateCandidatePasswordRequest updateCandidatePasswordRequest,
        CancellationToken cancellationToken);

    Task<ExperienciaLaboralResponse> AddExperienciaAsync(
        Guid userId,
        AddExperienciaLaboralRequest request,
        CancellationToken cancellationToken);

    Task DeleteExperienciaAsync(Guid userId, Guid experienciaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetHabilidadesBlandasSugeridasAsync(CancellationToken cancellationToken);

    Task<HabilidadResponse> AddHabilidadAsync(
        Guid userId,
        AddHabilidadRequest request,
        CancellationToken cancellationToken);

    Task DeleteHabilidadAsync(Guid userId, Guid habilidadId, CancellationToken cancellationToken);

    Task<CursoCompletadoResponse> AddCursoAsync(
        Guid userId,
        AddCursoCompletadoRequest request,
        CancellationToken cancellationToken);

    Task DeleteCursoAsync(Guid userId, Guid cursoId, CancellationToken cancellationToken);
}
