using services.dtos;

namespace services.interfaces;

public interface IEmployerRegistrationService
{
    Task<EmployerRegistrationResponse> RegisterAsync(
        RegisterEmployerRequest request,
        CancellationToken cancellationToken);

    Task<EmployerProfileResponse> GetProfileByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task ActivateAsync(Guid employerProfileId, CancellationToken cancellationToken);
}
