using domain.entities;

namespace services.interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindByPasswordResetTokenAsync(string token, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveAsync(User user, CancellationToken cancellationToken);
    Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken);
    Task UpdateRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken);
    Task SavePasswordResetTokenAsync(Guid userId, string token, DateTime expiresAtUtc, CancellationToken cancellationToken);
    Task ClearPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken);
}
