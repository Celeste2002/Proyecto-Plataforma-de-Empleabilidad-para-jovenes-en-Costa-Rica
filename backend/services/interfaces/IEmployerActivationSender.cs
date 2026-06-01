using domain.entities;

namespace services.interfaces;

public interface IEmployerActivationSender
{
    Task SendRegistrationConfirmationAsync(EmployerProfile employerProfile, CancellationToken cancellationToken);
    Task SendActivationNotificationAsync(EmployerProfile employerProfile, CancellationToken cancellationToken);
}
