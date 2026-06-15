using services.dtos;

namespace services.interfaces;

public interface IAdminReportRepository
{
    Task<AdminReportResponse> GetReportDataAsync(CancellationToken cancellationToken);
}
