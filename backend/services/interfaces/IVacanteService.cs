using services.dtos;

namespace services.interfaces;

public interface IVacanteService
{
    Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetAllVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(Guid employerUserId, CancellationToken cancellationToken);
    Task ApplyAsync(Guid candidateUserId, ApplyToVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateMyVacanteStatusAsync(
        Guid employerUserId,
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateVacanteStatusAsAdminAsync(
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken);
}
