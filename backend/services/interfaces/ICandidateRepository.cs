using domain.entities;

namespace services.interfaces;

public interface ICandidateRepository
{
    Task<CandidateProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(CancellationToken cancellationToken);

    Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);

    Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken);
}
