using services.dtos;

namespace services.interfaces;

public interface ICandidateRegistrationService
{
    Task<CandidateRegistrationResponse> RegisterAsync(
        RegisterCandidateRequest registerCandidateRequest,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CandidateProfileResponse>> GetProfilesVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken);
}
