using domain.entities;

namespace services.interfaces;

public interface IEmailConfirmationSender
{
    Task SendRegistrationConfirmationAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);
}
