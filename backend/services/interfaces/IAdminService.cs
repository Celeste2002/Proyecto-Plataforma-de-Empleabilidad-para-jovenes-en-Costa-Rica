using services.dtos;

namespace services.interfaces;

public interface IAdminService
{
    Task<IReadOnlyCollection<UserSummaryResponse>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task UpdateUserRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken);
    Task<AdminReportResponse> GetReportDataAsync(CancellationToken cancellationToken);
}
