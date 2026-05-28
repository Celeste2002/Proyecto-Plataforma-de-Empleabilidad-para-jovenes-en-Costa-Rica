using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class AdminService(IUserRepository userRepository) : IAdminService
{
    public async Task<IReadOnlyCollection<UserSummaryResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<User> users = await userRepository.GetAllAsync(cancellationToken);

        return users
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new UserSummaryResponse(
                u.Id,
                u.Email,
                u.Role,
                u.IsActive,
                u.EmailConfirmed,
                u.CreatedAtUtc))
            .ToArray();
    }

    public async Task UpdateUserRoleAsync(
        Guid userId,
        string newRole,
        CancellationToken cancellationToken)
    {
        if (!UserRoles.All.Contains(newRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new RequestValidationException(
                [$"El rol '{newRole}' no es valido. Roles permitidos: {string.Join(", ", UserRoles.All)}."]);
        }

        User? user = await userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"No se encontro un usuario con el Id '{userId}'.");
        }

        await userRepository.UpdateRoleAsync(userId, newRole.ToUpperInvariant(), cancellationToken);
    }
}
