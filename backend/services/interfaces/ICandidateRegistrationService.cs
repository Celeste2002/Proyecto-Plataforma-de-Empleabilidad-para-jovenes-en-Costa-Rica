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

    Task<CandidateProfileResponse> UpdateProfileAsync(
        Guid userId,
        UpdateCandidateProfileRequest updateCandidateProfileRequest,
        CancellationToken cancellationToken);

    Task UpdatePasswordAsync(
        Guid userId,
        UpdateCandidatePasswordRequest updateCandidatePasswordRequest,
        CancellationToken cancellationToken);
}
