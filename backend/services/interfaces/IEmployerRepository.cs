using domain.entities;

namespace services.interfaces;

public interface IEmployerRepository
{
    Task<EmployerProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<EmployerProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<EmployerProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(EmployerProfile employerProfile, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid employerProfileId, string status, CancellationToken cancellationToken);
    Task MarkActivationEmailSentAsync(Guid employerProfileId, CancellationToken cancellationToken);
}
